<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Check } from '@lucide/vue';
import { RouterLink, useRouter } from 'vue-router';
import { api, ApiRequestError } from '../lib/api';
import { fetchBarbers, type ApiBarber } from '../lib/catalog';
import { useAuthStore } from '../stores/auth';
import { useShopStore } from '../stores/shop';
import UiCheckbox from '../components/ui/UiCheckbox.vue';
import UiInput from '../components/ui/UiInput.vue';

interface AdminBooking {
  id: string;
  serviceName: string;
  barberName: string;
  clientName: string;
  startsAt: string;
  endsAt: string;
  status: string;
}

interface AdminBookingsPage {
  items: AdminBooking[];
  total: number;
}

interface DayOff {
  id: string;
  staffId: string | null;
  staffName: string;
  date: string;
  reason: string | null;
}

const STATUS_SR: Record<string, string> = {
  Pending: 'NA ČEKANJU',
  Confirmed: 'POTVRĐENA',
  Cancelled: 'OTKAZANA',
  Completed: 'ZAVRŠENA',
  NoShow: 'NEDOLAZAK',
};

const STATUS_KEYS = ['Pending', 'Confirmed', 'Cancelled', 'Completed', 'NoShow'];
const PAGE_SIZE = 10;

const auth = useAuthStore();
const router = useRouter();
const shopStore = useShopStore();

type Section = 'bookings' | 'daysoff';
const section = ref<Section>('bookings');
const isOwner = computed(() => auth.role === 'Owner');
const staff = ref<ApiBarber[]>([]);
const selectedStaff = ref<string[]>([]);
const selectedStatuses = ref<string[]>([]);
const showUpcoming = ref(true);
const bookings = ref<AdminBooking[]>([]);
const total = ref(0);
const loading = ref(true);
const loadingMore = ref(false);
const loadError = ref('');
const actionError = ref('');

const dayOffs = ref<DayOff[]>([]);
const doStaff = ref('');
const doDate = ref('');
const doReason = ref('');
const doError = ref('');
const doStaffError = ref('');
const doDateError = ref('');
const doLoading = ref(false);

const todayIso = computed(() => {
  const now = new Date();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${m}-${day}`;
});

const canShowMore = computed(() => bookings.value.length < total.value);

function statusLabel(status: string): string {
  return STATUS_SR[status] ?? status.toUpperCase();
}

function formatWhen(iso: string): string {
  return new Date(iso).toLocaleString('sr-Latn', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function buildQuery(): string {
  const params = new URLSearchParams();
  for (const id of selectedStaff.value) params.append('staffId', id);
  for (const s of selectedStatuses.value) params.append('status', s);
  params.append('upcoming', String(showUpcoming.value));
  params.append('skip', String(bookings.value.length));
  params.append('take', String(PAGE_SIZE));
  return params.toString();
}

function resetList(): void {
  bookings.value = [];
  total.value = 0;
}

async function loadBookings(): Promise<void> {
  loadingMore.value = true;
  try {
    const page = await api<AdminBookingsPage>(`/api/booking?${buildQuery()}`, {
      token: auth.accessToken,
    });
    bookings.value = [...bookings.value, ...page.items];
    total.value = page.total;
  } catch {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { next: '/owner' } });
      return;
    }
    loadError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    loadingMore.value = false;
  }
}

function reload(): void {
  resetList();
  loadError.value = '';
  actionError.value = '';
  void loadBookings();
}

const VISIBLE_WHEN_UPCOMING = ['Pending', 'Confirmed'];

async function changeStatus(b: AdminBooking, status: string): Promise<void> {
  actionError.value = '';
  try {
    const updated = await api<AdminBooking>(`/api/booking/${b.id}/status`, {
      method: 'PATCH',
      token: auth.accessToken,
      body: { status },
    });
    const idx = bookings.value.findIndex((x) => x.id === b.id);
    if (idx < 0) return;
    if (showUpcoming.value && !VISIBLE_WHEN_UPCOMING.includes(updated.status)) {
      bookings.value.splice(idx, 1);
      total.value = Math.max(0, total.value - 1);
    } else {
      bookings.value[idx] = updated;
    }
  } catch {
    actionError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

async function loadDaysOff(): Promise<void> {
  try {
    dayOffs.value = await api<DayOff[]>(`/api/daysoff?from=${todayIso.value}`, {
      token: auth.accessToken,
    });
  } catch {
    doError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

async function addDayOff(): Promise<void> {
  doError.value = '';
  doStaffError.value = doStaff.value ? '' : 'IZABERI BERBERINA';
  if (!doDate.value) doDateError.value = 'IZABERI DATUM';
  else if (doDate.value < todayIso.value) doDateError.value = 'DATUM MORA BITI DANAS ILI KASNIJE';
  else doDateError.value = '';
  if (doStaffError.value || doDateError.value) return;
  doLoading.value = true;
  try {
    await api<unknown>('/api/daysoff', {
      method: 'POST',
      token: auth.accessToken,
      body: {
        staffId: doStaff.value,
        date: doDate.value,
        reason: doReason.value || undefined,
      },
    });
    doReason.value = '';
    await loadDaysOff();
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 409) {
      doError.value = 'DAN JE VEĆ ZAUZET';
      return;
    }
    doError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    doLoading.value = false;
  }
}

async function removeDayOff(id: string): Promise<void> {
  doError.value = '';
  try {
    await api<void>(`/api/daysoff/${id}`, { method: 'DELETE', token: auth.accessToken });
    await loadDaysOff();
  } catch {
    doError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

onMounted(async () => {
  void shopStore.load();
  try {
    if (isOwner.value) {
      staff.value = await fetchBarbers();
      doDate.value = todayIso.value;
      await loadDaysOff();
    }
    await loadBookings();
  } catch {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { next: '/owner' } });
      return;
    }
    loadError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <main class="bg-ink text-bone">
    <div class="mx-auto w-full max-w-6xl px-4 py-10">
      <p v-if="shopStore.shop" class="text-center text-lg tracking-widest">
        <RouterLink to="/" class="text-ash hover:text-bone" aria-label="Nazad na početnu">
          {{ shopStore.shop.name }}
        </RouterLink>
      </p>
      <h1 class="mt-2 text-center font-display text-6xl leading-none tracking-wide">PANEL</h1>

      <nav
        v-if="isOwner"
        aria-label="Sekcije panela"
        class="mt-8 grid grid-cols-2 border border-line lg:hidden"
      >
        <button
          type="button"
          :aria-current="section === 'bookings' ? 'page' : undefined"
          :class="section === 'bookings' ? 'bg-bone text-ink' : 'text-ash'"
          class="border-r border-line py-2 text-xl tracking-widest last:border-r-0"
          @click="section = 'bookings'"
        >
          REZERVACIJE
        </button>
        <button
          type="button"
          :aria-current="section === 'daysoff' ? 'page' : undefined"
          :class="section === 'daysoff' ? 'bg-bone text-ink' : 'text-ash'"
          class="border-r border-line py-2 text-xl tracking-widest last:border-r-0"
          @click="section = 'daysoff'"
        >
          NERADNI DANI
        </button>
      </nav>

      <div class="mt-8 lg:mt-10 lg:grid lg:grid-cols-[240px_1fr] lg:gap-8">
        <aside v-if="isOwner" class="hidden lg:block" aria-label="Sekcije panela">
          <nav class="flex flex-col border border-line">
            <button
              type="button"
              :aria-current="section === 'bookings' ? 'page' : undefined"
              :class="
                section === 'bookings'
                  ? 'bg-bone text-ink'
                  : 'text-ash hover:text-bone'
              "
              class="border-b border-line px-4 py-3 text-left text-xl tracking-widest last:border-b-0"
              @click="section = 'bookings'"
            >
              REZERVACIJE
            </button>
            <button
              type="button"
              :aria-current="section === 'daysoff' ? 'page' : undefined"
              :class="
                section === 'daysoff' ? 'bg-bone text-ink' : 'text-ash hover:text-bone'
              "
              class="border-b border-line px-4 py-3 text-left text-xl tracking-widest last:border-b-0"
              @click="section = 'daysoff'"
            >
              NERADNI DANI
            </button>
          </nav>
        </aside>

        <div>
          <p v-if="loading" class="text-xl tracking-widest text-ash">UČITAVANJE...</p>
          <p
            v-else-if="loadError"
            role="alert"
            class="border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
          >
            {{ loadError }}
          </p>

          <div v-else>
            <section v-if="section === 'bookings'">
              <section v-if="isOwner" aria-label="Filter po berberinu">
                <h2 class="text-3xl tracking-widest text-bone">BERBERIN</h2>
                <div class="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
                  <label
                    v-for="b in staff"
                    :key="b.id"
                    class="flex cursor-pointer items-center gap-3 border border-line bg-surface p-4"
                  >
                    <input
                      v-model="selectedStaff"
                      type="checkbox"
                      :value="b.id"
                      class="peer sr-only"
                      @change="reload"
                    />
                    <span
                      aria-hidden="true"
                      class="flex h-6 w-6 shrink-0 items-center justify-center border border-line bg-ink text-transparent peer-checked:border-bone peer-checked:bg-bone peer-checked:text-ink peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-bone"
                    >
                      <Check class="h-4 w-4" />
                    </span>
                    <span class="text-2xl leading-none tracking-widest text-bone">
                      {{ b.firstName }} {{ b.lastName }}
                    </span>
                  </label>
                </div>
              </section>

              <section aria-label="Filter po statusu" class="mt-8">
                <h2 class="text-3xl tracking-widest text-bone">STATUS</h2>
                <div class="mt-4 grid grid-cols-2 gap-2 sm:grid-cols-3">
                  <label
                    v-for="s in STATUS_KEYS"
                    :key="s"
                    class="flex cursor-pointer items-center gap-2 border border-line bg-surface px-3 py-2"
                  >
                    <input
                      v-model="selectedStatuses"
                      type="checkbox"
                      :value="s"
                      class="peer sr-only"
                      @change="reload"
                    />
                    <span
                      aria-hidden="true"
                      class="flex h-6 w-6 shrink-0 items-center justify-center border border-line bg-ink text-transparent peer-checked:border-bone peer-checked:bg-bone peer-checked:text-ink peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-bone"
                    >
                      <Check class="h-4 w-4" />
                    </span>
                    <span class="text-lg tracking-widest text-bone">{{ STATUS_SR[s] }}</span>
                  </label>
                </div>
              </section>

              <div class="mt-6">
                <UiCheckbox
                  :model-value="showUpcoming"
                  label="SAMO BUDUĆE I NEZAVRŠENE"
                  @update:model-value="(v: boolean) => { showUpcoming = v; reload(); }"
                />
              </div>

              <h2 class="mt-8 text-3xl tracking-widest text-bone">
                REZERVACIJE
                <span class="text-blaze">({{ total }})</span>
              </h2>
              <p
                v-if="actionError"
                role="alert"
                class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
              >
                {{ actionError }}
              </p>
              <p v-if="bookings.length === 0" class="mt-4 text-xl tracking-widest text-ash">
                NEMA REZERVACIJA
              </p>
              <ul v-else aria-label="Rezervacije" class="mt-4 grid gap-3 xl:grid-cols-2">
                <li v-for="b in bookings" :key="b.id" class="border border-line bg-surface p-4">
                  <div class="flex items-center justify-between gap-3">
                    <span class="text-2xl leading-none tracking-widest text-bone">
                      {{ b.serviceName }}
                    </span>
                    <span class="shrink-0 text-lg tracking-widest text-blaze">
                      {{ statusLabel(b.status) }}
                    </span>
                  </div>
                  <p class="mt-1 text-lg tracking-widest text-ash">
                    {{ b.barberName }} / {{ b.clientName }} / {{ formatWhen(b.startsAt) }}
                  </p>
                  <div v-if="b.status === 'Pending'" class="mt-3 flex gap-2">
                    <button
                      type="button"
                      class="flex-1 bg-blaze py-2 text-xl tracking-widest text-ink hover:opacity-90"
                      @click="changeStatus(b, 'Confirmed')"
                    >
                      POTVRDI
                    </button>
                    <button
                      type="button"
                      class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                      @click="changeStatus(b, 'Cancelled')"
                    >
                      OTKAŽI
                    </button>
                  </div>
                  <div v-else-if="b.status === 'Confirmed'" class="mt-3 flex gap-2">
                    <button
                      type="button"
                      class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                      @click="changeStatus(b, 'Completed')"
                    >
                      ZAVRŠI
                    </button>
                    <button
                      type="button"
                      class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                      @click="changeStatus(b, 'NoShow')"
                    >
                      NIJE DOŠAO
                    </button>
                    <button
                      type="button"
                      class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                      @click="changeStatus(b, 'Cancelled')"
                    >
                      OTKAŽI
                    </button>
                  </div>
                </li>
              </ul>
              <button
                v-if="canShowMore"
                type="button"
                :disabled="loadingMore"
                class="mt-4 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone disabled:opacity-40"
                @click="loadBookings"
              >
                {{ loadingMore ? 'UČITAVANJE...' : 'PRIKAŽI JOŠ' }}
              </button>
            </section>

            <section v-if="isOwner && section === 'daysoff'" aria-label="Neradni dani">
              <h2 class="text-3xl tracking-widest text-bone">NERADNI DANI</h2>
              <div class="mt-4 grid gap-3 sm:grid-cols-3">
                <div>
                  <label for="dayoff-staff" class="block text-lg tracking-widest text-ash">
                    BERBERIN
                  </label>
                  <select
                    id="dayoff-staff"
                    v-model="doStaff"
                    :aria-invalid="!!doStaffError"
                    aria-describedby="dayoff-staff-error"
                    :class="doStaffError ? 'border-alarm' : 'border-line'"
                    class="mt-1 w-full bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
                    @change="doStaffError = ''"
                  >
                    <option value="" disabled>IZABERI BERBERINA</option>
                    <option v-for="b in staff" :key="b.id" :value="b.id">
                      {{ b.firstName }} {{ b.lastName }}
                    </option>
                  </select>
                  <p
                    id="dayoff-staff-error"
                    aria-live="polite"
                    class="mt-1 min-h-6 text-lg tracking-widest text-alarm"
                  >
                    {{ doStaffError }}
                  </p>
                </div>
                <div>
                  <label for="dayoff-date" class="block text-lg tracking-widest text-ash">
                    DATUM
                  </label>
                  <input
                    id="dayoff-date"
                    v-model="doDate"
                    type="date"
                    :min="todayIso"
                    :aria-invalid="!!doDateError"
                    aria-describedby="dayoff-date-error"
                    :class="doDateError ? 'border-alarm' : 'border-line'"
                    class="mt-1 w-full bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
                    @change="doDateError = ''"
                  />
                  <p
                    id="dayoff-date-error"
                    aria-live="polite"
                    class="mt-1 min-h-6 text-lg tracking-widest text-alarm"
                  >
                    {{ doDateError }}
                  </p>
                </div>
                <div>
                  <UiInput
                    id="dayoff-reason"
                    v-model="doReason"
                    label="RAZLOG (OPCIONO)"
                    type="text"
                    placeholder="BOLOVANJE"
                  />
                </div>
              </div>
              <p
                v-if="doError"
                role="alert"
                class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
              >
                {{ doError }}
              </p>
              <button
                type="button"
                :disabled="doLoading"
                class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
                @click="addDayOff"
              >
                {{ doLoading ? 'UČITAVANJE...' : 'DODAJ NERADAN DAN' }}
              </button>
              <ul
                v-if="dayOffs.length > 0"
                aria-label="Neradni dani"
                class="mt-4 grid gap-2 xl:grid-cols-2"
              >
                <li
                  v-for="d in dayOffs"
                  :key="d.id"
                  class="flex items-center justify-between gap-3 border border-line bg-surface px-3 py-2"
                >
                  <span class="text-lg tracking-widest text-bone">
                    {{ d.date }} / {{ d.staffName }}{{ d.reason ? ` / ${d.reason}` : '' }}
                  </span>
                  <button
                    type="button"
                    class="shrink-0 border border-line px-3 py-1 text-lg tracking-widest text-ash hover:border-alarm hover:text-alarm"
                    @click="removeDayOff(d.id)"
                  >
                    UKLONI
                  </button>
                </li>
              </ul>
            </section>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>
