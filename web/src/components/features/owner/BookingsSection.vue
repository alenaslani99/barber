<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Check } from '@lucide/vue';
import { useRouter } from 'vue-router';
import { api } from '../../../lib/api';
import { STATUS_KEYS, STATUS_SR, formatWhen, statusLabel } from '../../../lib/bookings';
import type { AdminBooking, AdminBookingsPage } from '../../../lib/bookings';
import type { ApiBarber } from '../../../lib/catalog';
import { useAuthStore } from '../../../stores/auth';
import UiCheckbox from '../../ui/UiCheckbox.vue';

const props = defineProps<{ staff: ApiBarber[]; isOwner: boolean }>();

const PAGE_SIZE = 10;
const VISIBLE_WHEN_UPCOMING = ['Pending', 'Confirmed'];

const auth = useAuthStore();
const router = useRouter();

const selectedStaff = ref<string[]>([]);
const selectedStatuses = ref<string[]>([]);
const showUpcoming = ref(true);
const bookings = ref<AdminBooking[]>([]);
const total = ref(0);
const loading = ref(true);
const loadingMore = ref(false);
const loadError = ref('');
const actionError = ref('');

const activeStaff = computed(() => props.staff.filter((b) => b.isActive));
const canShowMore = computed(() => bookings.value.length < total.value);

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

onMounted(async () => {
  try {
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
  <p v-if="loading" class="text-xl tracking-widest text-ash">UČITAVANJE...</p>
  <p
    v-else-if="loadError"
    role="alert"
    class="border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
  >
    {{ loadError }}
  </p>
  <div v-else>
  <section v-if="props.isOwner" aria-label="Filter po berberinu">
    <h2 class="text-3xl tracking-widest text-bone">BERBERIN</h2>
    <div class="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
      <label
        v-for="b in activeStaff.filter((x) => x.isActive)"
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
  </div>
</template>
