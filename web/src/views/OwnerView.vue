<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Check } from '@lucide/vue';
import { RouterLink, useRouter } from 'vue-router';
import { api } from '../lib/api';
import { fetchBarbers, type ApiBarber } from '../lib/catalog';
import { useAuthStore } from '../stores/auth';
import { shop } from '../data/mock';
import UiCheckbox from '../components/ui/UiCheckbox.vue';

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
  void loadBookings();
}

onMounted(async () => {
  try {
    if (isOwner.value) staff.value = await fetchBarbers();
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
    <div class="mx-auto w-full px-4 py-10 lg:w-1/2">
      <p class="text-center text-lg tracking-widest">
        <RouterLink to="/" class="text-ash hover:text-bone" aria-label="Nazad na početnu">
          {{ shop.name }}
        </RouterLink>
      </p>
      <h1 class="mt-2 text-center font-display text-6xl leading-none tracking-wide">PANEL</h1>

      <p v-if="loading" class="mt-8 text-xl tracking-widest text-ash">UČITAVANJE...</p>
      <p
        v-else-if="loadError"
        role="alert"
        class="mt-8 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
      >
        {{ loadError }}
      </p>

      <div v-else>
        <section v-if="isOwner" aria-label="Filter po berberinu" class="mt-8">
          <h2 class="text-3xl tracking-widest text-bone">BERBERIN</h2>
          <div class="mt-4 flex flex-col gap-3">
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
                class="flex h-6 w-6 items-center justify-center border border-line bg-ink text-transparent peer-checked:border-bone peer-checked:bg-bone peer-checked:text-ink peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-bone"
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
          <div class="mt-4 grid grid-cols-2 gap-2">
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
                class="flex h-6 w-6 items-center justify-center border border-line bg-ink text-transparent peer-checked:border-bone peer-checked:bg-bone peer-checked:text-ink peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-bone"
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
        <p v-if="bookings.length === 0" class="mt-4 text-xl tracking-widest text-ash">
          NEMA REZERVACIJA
        </p>
        <ul v-else aria-label="Rezervacije" class="mt-4 flex flex-col gap-3">
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
    </div>
  </main>
</template>
