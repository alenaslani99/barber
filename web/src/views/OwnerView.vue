<script setup lang="ts">
import { computed, onMounted, ref, useTemplateRef } from 'vue';
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

type Validator = { validate: () => boolean; setError: (message: string) => void };
type Section = 'bookings' | 'daysoff' | 'services' | 'staff' | 'hours';
const section = ref<Section>('bookings');

const SECTIONS: ReadonlyArray<{ id: Section; label: string }> = [
  { id: 'bookings', label: 'REZERVACIJE' },
  { id: 'daysoff', label: 'NERADNI DANI' },
  { id: 'services', label: 'USLUGE' },
  { id: 'staff', label: 'OSOBLJE' },
  { id: 'hours', label: 'RADNO VREME' },
];

const SENIORITIES = [
  { id: '11111111-1111-1111-1111-111111111111', name: 'Junior' },
  { id: '22222222-2222-2222-2222-222222222222', name: 'Barber' },
  { id: '33333333-3333-3333-3333-333333333333', name: 'Senior' },
  { id: '44444444-4444-4444-4444-444444444444', name: 'Master' },
];

const DURATIONS = [10, 15, 20, 30, 40, 45, 55, 60, 90];
const SLOT_STEPS = [5, 10, 15, 20, 30, 60];
const WEEKDAYS_FULL = [
  'NEDELJA',
  'PONEDELJAK',
  'UTORAK',
  'SREDA',
  'ČETVRTAK',
  'PETAK',
  'SUBOTA',
];

interface ManagedService {
  id: string;
  name: string;
  price: number;
  durationMinutes: number;
  slotMinutes: number;
  isActive: boolean;
}

interface ShopHoursRow {
  day: number;
  open: string;
  close: string;
  closed: boolean;
}
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

const shopServices = ref<ManagedService[]>([]);
const svcError = ref('');
const svcLoading = ref(false);
const svcEditingId = ref<string | null>(null);
const svcName = ref('');
const svcPrice = ref('');
const svcDuration = ref('30');
const svcSlot = ref('30');
const svcActive = ref(true);

const staffEmail = ref('');
const staffSeniority = ref('22222222-2222-2222-2222-222222222222');
const staffFormError = ref('');
const staffFormLoading = ref(false);

const hoursRows = ref<ShopHoursRow[]>([]);
const hoursError = ref('');
const hoursSuccess = ref('');
const hoursLoading = ref(false);

const todayIso = computed(() => {
  const now = new Date();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${m}-${day}`;
});

const canShowMore = computed(() => bookings.value.length < total.value);
const activeStaff = computed(() => staff.value.filter((b) => b.isActive));

const svcNameInput = useTemplateRef<Validator>('svcNameInput');
const svcPriceInput = useTemplateRef<Validator>('svcPriceInput');
const staffEmailInput = useTemplateRef<Validator>('staffEmailInput');

const svcNameRules = [(v: string) => (!v ? 'NAZIV JE OBAVEZAN' : '')];
const svcPriceRules = [
  (v: string) => (!v ? 'CENA JE OBAVEZNA' : ''),
  (v: string) => (!/^\d+(\.\d{1,2})?$/.test(v) ? 'UNESI ISPRAVAN IZNOS' : ''),
];
const staffEmailRules = [
  (v: string) => (!v ? 'EMAIL JE OBAVEZAN' : ''),
  (v: string) => (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v) ? 'UNESI ISPRAVAN EMAIL' : ''),
];

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

async function loadServices(): Promise<void> {
  try {
    const list = await api<ManagedService[]>('/api/service', { auth: false });
    shopServices.value = list;
  } catch {
    svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

function resetServiceForm(): void {
  svcEditingId.value = null;
  svcName.value = '';
  svcPrice.value = '';
  svcDuration.value = '30';
  svcSlot.value = '30';
  svcActive.value = true;
}

async function saveService(): Promise<void> {
  svcError.value = '';
  const nameOk = svcNameInput.value?.validate() ?? false;
  const priceOk = svcPriceInput.value?.validate() ?? false;
  if (!nameOk || !priceOk) return;
  svcLoading.value = true;
  try {
    const body = {
      name: svcName.value,
      price: Number(svcPrice.value),
      durationMinutes: Number(svcDuration.value),
      slotMinutes: Number(svcSlot.value),
      isActive: svcActive.value,
    };
    if (svcEditingId.value) {
      await api(`/api/service/${svcEditingId.value}`, {
        method: 'PATCH',
        token: auth.accessToken,
        body,
      });
    } else {
      const shopId = shopStore.shop?.id;
      if (!shopId) {
        svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
        return;
      }
      await api('/api/service', {
        method: 'POST',
        token: auth.accessToken,
        body: { barbershopId: shopId, ...body },
      });
    }
    resetServiceForm();
    await loadServices();
  } catch {
    svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    svcLoading.value = false;
  }
}

function editService(s: ManagedService): void {
  svcError.value = '';
  svcEditingId.value = s.id;
  svcName.value = s.name;
  svcPrice.value = String(s.price);
  svcDuration.value = String(s.durationMinutes);
  svcSlot.value = String(s.slotMinutes);
  svcActive.value = s.isActive;
}

async function toggleService(s: ManagedService): Promise<void> {
  svcError.value = '';
  try {
    await api(`/api/service/${s.id}`, {
      method: 'PATCH',
      token: auth.accessToken,
      body: { isActive: !s.isActive },
    });
    await loadServices();
  } catch {
    svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

async function addStaffMember(): Promise<void> {
  staffFormError.value = '';
  if (!(staffEmailInput.value?.validate() ?? false)) return;
  staffFormLoading.value = true;
  try {
    const shopId = shopStore.shop?.id;
    if (!shopId) {
      staffFormError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
      return;
    }
    await api('/api/staff', {
      method: 'POST',
      token: auth.accessToken,
      body: { email: staffEmail.value, barbershopId: shopId, seniorityId: staffSeniority.value },
    });
    staffEmail.value = '';
    staff.value = await fetchBarbers(true, auth.accessToken);
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 404) {
      staffFormError.value = 'KORISNIK NE POSTOJI, MORA SE PRVO REGISTROVATI';
      return;
    }
    staffFormError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    staffFormLoading.value = false;
  }
}

async function toggleStaff(b: ApiBarber): Promise<void> {
  staffFormError.value = '';
  try {
    await api(`/api/staff/${b.id}`, {
      method: 'PATCH',
      token: auth.accessToken,
      body: { isActive: !b.isActive },
    });
    staff.value = await fetchBarbers(true, auth.accessToken);
  } catch {
    staffFormError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

async function loadHours(): Promise<void> {
  const shopId = shopStore.shop?.id;
  if (!shopId) return;
  try {
    const rows = await api<ShopHoursRow[]>(`/api/workinghours?barbershopId=${shopId}`, {
      token: auth.accessToken,
    });
    hoursRows.value = [1, 2, 3, 4, 5, 6, 0].map((day) => {
      const found = rows.find((r) => r.day === day);
      return found ?? { day, open: '09:00', close: '20:00', closed: day === 0 };
    });
  } catch {
    hoursError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

async function saveHours(): Promise<void> {
  hoursError.value = '';
  hoursSuccess.value = '';
  const shopId = shopStore.shop?.id;
  if (!shopId) {
    hoursError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
    return;
  }
  hoursLoading.value = true;
  try {
    await api('/api/workinghours', {
      method: 'PUT',
      token: auth.accessToken,
      body: {
        barbershopId: shopId,
        hours: hoursRows.value.map((r) => ({
          day: r.day,
          open: r.closed ? '00:00' : r.open,
          close: r.closed ? '00:00' : r.close,
          isClosed: r.closed,
        })),
      },
    });
    hoursSuccess.value = 'SAČUVANO';
  } catch {
    hoursError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    hoursLoading.value = false;
  }
}

onMounted(async () => {
  void shopStore.load();
  try {
    if (isOwner.value) {
      staff.value = await fetchBarbers(true, auth.accessToken);
      doDate.value = todayIso.value;
      await loadDaysOff();
      await loadServices();
      await loadHours();
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
        class="mt-8 flex flex-col border border-line lg:hidden"
      >
        <button
          v-for="s in SECTIONS"
          :key="s.id"
          type="button"
          :aria-current="section === s.id ? 'page' : undefined"
          :class="section === s.id ? 'bg-bone text-ink' : 'text-ash'"
          class="border-b border-line py-2 text-xl tracking-widest last:border-b-0"
          @click="section = s.id"
        >
          {{ s.label }}
        </button>
      </nav>

      <div class="mt-8 lg:mt-10 lg:grid lg:grid-cols-[240px_1fr] lg:gap-8">
        <aside v-if="isOwner" class="hidden lg:block" aria-label="Sekcije panela">
          <nav class="flex flex-col border border-line">
            <button
              v-for="s in SECTIONS"
              :key="s.id"
              type="button"
              :aria-current="section === s.id ? 'page' : undefined"
              :class="section === s.id ? 'bg-bone text-ink' : 'text-ash hover:text-bone'"
              class="border-b border-line px-4 py-3 text-left text-xl tracking-widest last:border-b-0"
              @click="section = s.id"
            >
              {{ s.label }}
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
                    v-for="b in activeStaff"
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
                    <option v-for="b in activeStaff" :key="b.id" :value="b.id">
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

            <section v-if="isOwner && section === 'services'" aria-label="Usluge">
              <h2 class="text-3xl tracking-widest text-bone">USLUGE</h2>
              <p
                v-if="svcError"
                role="alert"
                class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
              >
                {{ svcError }}
              </p>
              <ul aria-label="Usluge" class="mt-4 grid gap-3 xl:grid-cols-2">
                <li
                  v-for="s in shopServices"
                  :key="s.id"
                  :class="
                    s.isActive
                      ? 'border border-line bg-surface p-4'
                      : 'border border-line bg-surface p-4 opacity-50'
                  "
                >
                  <div class="flex items-center justify-between gap-3">
                    <span class="text-2xl leading-none tracking-widest text-bone">
                      {{ s.name }}
                    </span>
                    <span class="shrink-0 text-2xl tracking-widest text-bone">{{ s.price }}</span>
                  </div>
                  <p class="mt-1 text-lg tracking-widest text-ash">
                    {{ s.durationMinutes }} MIN / KORAK {{ s.slotMinutes }} MIN
                  </p>
                  <div class="mt-3 flex gap-2">
                    <button
                      type="button"
                      class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                      @click="editService(s)"
                    >
                      UREDI
                    </button>
                    <button
                      type="button"
                      class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                      @click="toggleService(s)"
                    >
                      {{ s.isActive ? 'DEAKTIVIRAJ' : 'AKTIVIRAJ' }}
                    </button>
                  </div>
                </li>
              </ul>

              <h3 class="mt-8 text-2xl tracking-widest text-bone">
                {{ svcEditingId ? 'IZMENI USLUGU' : 'NOVA USLUGA' }}
              </h3>
              <div class="mt-4 grid gap-3 sm:grid-cols-2">
                <UiInput
                  id="svc-name"
                  ref="svcNameInput"
                  v-model="svcName"
                  label="NAZIV"
                  type="text"
                  placeholder="KLASIČNO ŠIŠANJE"
                  :rules="svcNameRules"
                />
                <UiInput
                  id="svc-price"
                  ref="svcPriceInput"
                  v-model="svcPrice"
                  label="CENA (RSD)"
                  type="text"
                  placeholder="2500"
                  autocomplete="off"
                  :rules="svcPriceRules"
                />
                <div>
                  <label for="svc-duration" class="block text-lg tracking-widest text-ash">
                    TRAJANJE (MIN)
                  </label>
                  <select
                    id="svc-duration"
                    v-model="svcDuration"
                    class="mt-1 w-full border border-line bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
                  >
                    <option v-for="d in DURATIONS" :key="d" :value="String(d)">
                      {{ d }} MIN
                    </option>
                  </select>
                  <p aria-hidden="true" class="mt-1 min-h-6 text-lg tracking-widest"> </p>
                </div>
                <div>
                  <label for="svc-slot" class="block text-lg tracking-widest text-ash">
                    KORAK (MIN)
                  </label>
                  <select
                    id="svc-slot"
                    v-model="svcSlot"
                    class="mt-1 w-full border border-line bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
                  >
                    <option v-for="m in SLOT_STEPS" :key="m" :value="String(m)">
                      {{ m }} MIN
                    </option>
                  </select>
                  <p aria-hidden="true" class="mt-1 min-h-6 text-lg tracking-widest"> </p>
                </div>
              </div>
              <div class="mt-2">
                <UiCheckbox v-model="svcActive" label="AKTIVNA" />
              </div>
              <div class="mt-4 flex gap-2">
                <button
                  type="button"
                  :disabled="svcLoading"
                  class="flex-1 bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
                  @click="saveService"
                >
                  {{ svcLoading ? 'UČITAVANJE...' : svcEditingId ? 'SAČUVAJ' : 'DODAJ' }}
                </button>
                <button
                  v-if="svcEditingId"
                  type="button"
                  class="flex-1 border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
                  @click="resetServiceForm"
                >
                  OTKAŽI
                </button>
              </div>
            </section>

            <section v-if="isOwner && section === 'staff'" aria-label="Osoblje">
              <h2 class="text-3xl tracking-widest text-bone">OSOBLJE</h2>
              <p
                v-if="staffFormError"
                role="alert"
                class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
              >
                {{ staffFormError }}
              </p>
              <ul aria-label="Osoblje" class="mt-4 grid gap-3 xl:grid-cols-2">
                <li
                  v-for="b in staff"
                  :key="b.id"
                  :class="
                    b.isActive
                      ? 'border border-line bg-surface p-4'
                      : 'border border-line bg-surface p-4 opacity-50'
                  "
                >
                  <div class="flex items-center justify-between gap-3">
                    <span class="text-2xl leading-none tracking-widest text-bone">
                      {{ b.firstName }} {{ b.lastName }}
                    </span>
                    <span class="shrink-0 text-lg tracking-widest text-ash">
                      {{ b.seniority.toUpperCase() }}
                    </span>
                  </div>
                  <button
                    type="button"
                    class="mt-3 w-full border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
                    @click="toggleStaff(b)"
                  >
                    {{ b.isActive ? 'DEAKTIVIRAJ' : 'AKTIVIRAJ' }}
                  </button>
                </li>
              </ul>

              <h3 class="mt-8 text-2xl tracking-widest text-bone">DODAJ BERBERINA</h3>
              <p class="mt-1 text-lg tracking-widest text-ash">
                KORISNIK MORA BITI REGISTROVAN
              </p>
              <div class="mt-4 grid gap-3 sm:grid-cols-2">
                <UiInput
                  id="staff-email"
                  ref="staffEmailInput"
                  v-model="staffEmail"
                  label="EMAIL"
                  type="email"
                  placeholder="BERBERIN@MAIL.COM"
                  autocomplete="email"
                  :rules="staffEmailRules"
                />
                <div>
                  <label for="staff-seniority" class="block text-lg tracking-widest text-ash">
                    NIVO
                  </label>
                  <select
                    id="staff-seniority"
                    v-model="staffSeniority"
                    class="mt-1 w-full border border-line bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
                  >
                    <option v-for="s in SENIORITIES" :key="s.id" :value="s.id">
                      {{ s.name.toUpperCase() }}
                    </option>
                  </select>
                  <p aria-hidden="true" class="mt-1 min-h-6 text-lg tracking-widest"> </p>
                </div>
              </div>
              <button
                type="button"
                :disabled="staffFormLoading"
                class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
                @click="addStaffMember"
              >
                {{ staffFormLoading ? 'UČITAVANJE...' : 'DODAJ' }}
              </button>
            </section>

            <section v-if="isOwner && section === 'hours'" aria-label="Radno vreme">
              <h2 class="text-3xl tracking-widest text-bone">RADNO VREME</h2>
              <p
                v-if="hoursError"
                role="alert"
                class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
              >
                {{ hoursError }}
              </p>
              <p
                v-if="hoursSuccess"
                role="status"
                class="mt-4 border border-bone p-3 text-center text-lg tracking-widest text-bone"
              >
                {{ hoursSuccess }}
              </p>
              <ul aria-label="Radno vreme po danima" class="mt-4 flex flex-col gap-3">
                <li
                  v-for="r in hoursRows"
                  :key="r.day"
                  class="grid items-center gap-3 border border-line bg-surface p-4 sm:grid-cols-[1fr_auto_auto_auto]"
                >
                  <span class="text-2xl leading-none tracking-widest text-bone">
                    {{ WEEKDAYS_FULL[r.day] }}
                  </span>
                  <UiCheckbox
                    :model-value="r.closed"
                    label="ZATVORENO"
                    @update:model-value="(v: boolean) => { r.closed = v; }"
                  />
                  <label class="flex items-center gap-2 text-lg tracking-widest text-ash">
                    OD
                    <input
                      v-model="r.open"
                      type="time"
                      :disabled="r.closed"
                      class="border border-line bg-ink px-2 py-1 font-form text-xl text-bone outline-none disabled:opacity-40"
                    />
                  </label>
                  <label class="flex items-center gap-2 text-lg tracking-widest text-ash">
                    DO
                    <input
                      v-model="r.close"
                      type="time"
                      :disabled="r.closed"
                      class="border border-line bg-ink px-2 py-1 font-form text-xl text-bone outline-none disabled:opacity-40"
                    />
                  </label>
                </li>
              </ul>
              <button
                type="button"
                :disabled="hoursLoading"
                class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
                @click="saveHours"
              >
                {{ hoursLoading ? 'UČITAVANJE...' : 'SAČUVAJ' }}
              </button>
            </section>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>
