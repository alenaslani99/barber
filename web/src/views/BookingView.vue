<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { api, ApiRequestError } from '../lib/api';
import { getNextDays, type TimeSlot } from '../data/mock';
import {
  fetchAvailability,
  type ApiBarber,
  type ApiService,
  type ApiShop,
  type Availability,
} from '../lib/catalog';
import { useShopStore } from '../stores/shop';
import BookingStepper from '../components/features/booking/BookingStepper.vue';
import ShopHeader from '../components/layout/ShopHeader.vue';
import BarberOption from '../components/features/booking/BarberOption.vue';
import ServiceOption from '../components/features/booking/ServiceOption.vue';
import DateStrip from '../components/features/booking/DateStrip.vue';
import TimeGrid from '../components/features/booking/TimeGrid.vue';
import BookingSummary from '../components/features/booking/BookingSummary.vue';
import { useAuthStore } from '../stores/auth';

interface BookingDraft {
  barberId: string | null;
  serviceId: string | null;
  dateIso: string | null;
  time: string | null;
  notes: string;
  step: number;
}

const DRAFT_KEY = 'barber.booking.draft';

const auth = useAuthStore();
const router = useRouter();
const shopStore = useShopStore();

type Step = 1 | 2 | 3 | 4;

const step = ref<Step>(1);
const barberId = ref<string | null>(null);
const serviceId = ref<string | null>(null);
const dateIso = ref<string | null>(null);
const time = ref<string | null>(null);
const notes = ref('');
const booked = ref(false);
const bookingRef = ref('');
const reserveLoading = ref(false);
const reserveError = ref('');
const barbers = ref<ApiBarber[]>([]);
const services = ref<ApiService[]>([]);
const shop = ref<ApiShop | null>(null);
const catalogLoading = ref(true);
const catalogError = ref('');

async function loadShop(): Promise<void> {
  catalogLoading.value = true;
  catalogError.value = '';
  const data = await shopStore.load();
  if (!data) {
    shop.value = null;
    barbers.value = [];
    services.value = [];
    catalogError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } else {
    shop.value = data;
    barbers.value = data.staff;
    services.value = data.services;
  }
  catalogLoading.value = false;
}

const closedDays = computed(() => {
  if (!shop.value) return [0];
  return shop.value.hours.filter((h) => h.closed).map((h) => h.day);
});
const days = computed(() => getNextDays(14, closedDays.value));

const selectedBarber = computed(() => barbers.value.find((b) => b.id === barberId.value) ?? null);
const selectedService = computed(
  () => services.value.find((s) => s.id === serviceId.value) ?? null,
);
const avail = ref<Availability | null>(null);
const availLoading = ref(false);
const availError = ref('');

function toLocalIsoDay(d: Date): string {
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${d.getFullYear()}-${m}-${day}`;
}

async function loadAvailability(): Promise<void> {
  availError.value = '';
  if (!barberId.value || !serviceId.value || !dateIso.value) {
    avail.value = null;
    return;
  }
  availLoading.value = true;
  try {
    avail.value = await fetchAvailability(barberId.value, dateIso.value, serviceId.value);
  } catch {
    avail.value = null;
    availError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    availLoading.value = false;
  }
}

const slots = computed<TimeSlot[]>(() => {
  if (!avail.value || avail.value.closed || !dateIso.value) return [];
  const taken = new Set(avail.value.taken);
  const [oh, om] = avail.value.open.split(':').map(Number);
  const [ch, cm] = avail.value.close.split(':').map(Number);
  const step = avail.value.slotMinutes > 0 ? avail.value.slotMinutes : 30;
  const now = new Date();
  const isToday = dateIso.value === toLocalIsoDay(now);
  const [y, mo, d] = dateIso.value.split('-').map(Number);
  const result: TimeSlot[] = [];
  for (let m = oh * 60 + om; m + step <= ch * 60 + cm; m += step) {
    const hh = String(Math.floor(m / 60)).padStart(2, '0');
    const mm = String(m % 60).padStart(2, '0');
    const time = `${hh}:${mm}`;
    let available = !taken.has(time);
    if (available && isToday) {
      available = new Date(y, mo - 1, d, Number(hh), Number(mm)).getTime() > now.getTime();
    }
    result.push({ id: `${dateIso.value}-${time}`, time, available });
  }
  return result;
});
const dateLabel = computed(() => {
  const d = days.value.find((day) => day.iso === dateIso.value);
  return d ? `${d.weekday} ${d.dayNum} ${d.month}` : '';
});

const canContinue = computed(() => {
  if (step.value === 1) return barberId.value !== null;
  if (step.value === 2) return serviceId.value !== null;
  if (step.value === 3) return dateIso.value !== null && time.value !== null;
  return true;
});

const recap = computed(() => {
  const parts: string[] = [];
  if (selectedBarber.value) parts.push(`${selectedBarber.value.firstName} ${selectedBarber.value.lastName}`);
  if (selectedService.value) parts.push(selectedService.value.name);
  if (dateLabel.value && time.value) parts.push(`${dateLabel.value} ${time.value}`);
  else if (dateLabel.value) parts.push(dateLabel.value);
  return parts.join(' / ');
});

function scrollTop(): void {  window.scrollTo({ top: 0 });
}

function selectBarber(id: string): void {
  barberId.value = id;
  step.value = 2;
  scrollTop();
  void loadAvailability();
}

function selectService(id: string): void {
  serviceId.value = id;
  step.value = 3;
  scrollTop();
  void loadAvailability();
}

function selectDate(iso: string): void {
  dateIso.value = iso;
  time.value = null;
  void loadAvailability();
}

function selectTime(t: string): void {
  time.value = t;
}

function next(): void {
  if (step.value < 4 && canContinue.value) {
    step.value = (step.value + 1) as Step;
    scrollTop();
    void loadAvailability();
  }
}

function back(): void {
  if (step.value > 1) {
    step.value = (step.value - 1) as Step;
    scrollTop();
  }
}

function goTo(s: number): void {
  if (s >= 1 && s <= 4 && s < step.value) {
    step.value = s as Step;
    void loadAvailability();
  }
}

function buildStartsAt(): string | null {
  if (!dateIso.value || !time.value) return null;
  const [y, m, d] = dateIso.value.split('-').map(Number);
  const [hh, mm] = time.value.split(':').map(Number);
  return new Date(y, m - 1, d, hh, mm).toISOString();
}

async function reserve(): Promise<void> {
  reserveError.value = '';
  if (!auth.isAuthenticated) {
    const draft: BookingDraft = {
      barberId: barberId.value,
      serviceId: serviceId.value,
      dateIso: dateIso.value,
      time: time.value,
      notes: notes.value,
      step: step.value,
    };
    localStorage.setItem(DRAFT_KEY, JSON.stringify(draft));
    void router.push({ path: '/login', query: { next: '/' } });
    return;
  }
  const startsAt = buildStartsAt();
  if (!barberId.value || !serviceId.value || !startsAt) return;
  reserveLoading.value = true;
  try {
    const booking = await api<{ id: string; status: string; startsAt: string; endsAt: string }>(
      '/api/booking',
      {
        method: 'POST',
        token: auth.accessToken,
        body: {
          serviceId: serviceId.value,
          staffId: barberId.value,
          startsAt,
          notes: notes.value || undefined,
        },
      },
    );
    localStorage.removeItem(DRAFT_KEY);
    bookingRef.value = booking.id.replace(/-/g, '').slice(0, 8).toUpperCase();
    booked.value = true;
    scrollTop();
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 409) {
      reserveError.value = 'TERMIN JE ZAUZET, IZABERI DRUGI';
    } else if (error instanceof ApiRequestError && error.status === 400) {
      reserveError.value = 'NEISPRAVNA REZERVACIJA';
    } else {
      reserveError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
    }
  } finally {
    reserveLoading.value = false;
  }
}

function reset(): void {
  step.value = 1;
  barberId.value = null;
  serviceId.value = null;
  dateIso.value = null;
  time.value = null;
  notes.value = '';
  booked.value = false;
  bookingRef.value = '';
  localStorage.removeItem(DRAFT_KEY);
  scrollTop();
}

onMounted(() => {
  void loadShop();
  try {
    const raw = localStorage.getItem(DRAFT_KEY);
    if (!raw) return;
    const draft = JSON.parse(raw) as BookingDraft;
    barberId.value = draft.barberId;
    serviceId.value = draft.serviceId;
    dateIso.value = draft.dateIso;
    time.value = draft.time;
    notes.value = draft.notes;
    if (draft.step >= 1 && draft.step <= 4) step.value = draft.step as Step;
    localStorage.removeItem(DRAFT_KEY);
  } catch {
    localStorage.removeItem(DRAFT_KEY);
  }
});
</script>

<template>
  <ShopHeader
    v-if="shop"
    :name="shop.name"
    :tagline="shop.tagline"
    :description="shop.description"
    :address="shop.address"
    :phone="shop.phone"
    :hours="shop.hours"
  />
  <main class="bg-ink text-bone">
    <div class="mx-auto mt-16 w-full px-4 pb-8 lg:w-1/2">
      <div v-if="booked">
        <h2 class="mt-8 font-display text-7xl leading-none tracking-wide">ZAKAZANO</h2>
        <p class="mt-2 text-xl tracking-widest text-ash">POKAŽI OVAJ KOD U SALONU</p>
        <p
          class="mt-4 border border-line bg-surface p-4 text-center text-4xl tracking-widest text-bone"
        >
          {{ bookingRef }}
        </p>
        <div v-if="selectedBarber && selectedService && dateLabel && time" class="mt-6">
          <BookingSummary
            :barber-name="selectedBarber.firstName + ' ' + selectedBarber.lastName"
            :service-name="selectedService.name"
            :duration-min="selectedService.durationMinutes"
            :price="selectedService.price"
            :date-label="dateLabel"
            :time="time"
          />
        </div>
        <button
          type="button"
          class="mt-6 w-full bg-bone py-3 text-2xl tracking-widest text-ink hover:opacity-90"
          @click="reset"
        >
          NOVA REZERVACIJA
        </button>
      </div>

      <div v-else>
        <BookingStepper :step="step" @go="goTo" />

        <section v-if="step === 1" aria-label="Choose barber">
          <h2 class="mt-8 text-3xl tracking-widest text-bone">01 / IZABERI BERBERINA</h2>
          <p v-if="catalogLoading" class="mt-4 text-xl tracking-widest text-ash">
            UČITAVANJE...
          </p>
          <div v-else-if="catalogError" class="mt-4">
            <p
              role="alert"
              class="border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
            >
              {{ catalogError }}
            </p>
            <button
              type="button"
              class="mt-3 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
              @click="loadShop"
            >
              POKUŠAJ PONOVO
            </button>
          </div>
          <div v-else class="mt-4 flex flex-col gap-3">
            <BarberOption
              v-for="b in barbers"
              :key="b.id"
              :barber="b"
              :selected="barberId === b.id"
              @select="selectBarber"
            />
          </div>
        </section>

        <section v-if="step === 2" aria-label="Choose service">
          <h2 class="mt-8 text-3xl tracking-widest text-bone">02 / IZABERI USLUGU</h2>
          <p v-if="catalogLoading" class="mt-4 text-xl tracking-widest text-ash">
            UČITAVANJE...
          </p>
          <div v-else-if="catalogError" class="mt-4">
            <p
              role="alert"
              class="border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
            >
              {{ catalogError }}
            </p>
            <button
              type="button"
              class="mt-3 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
              @click="loadShop"
            >
              POKUŠAJ PONOVO
            </button>
          </div>
          <div v-else class="mt-4 flex flex-col gap-3">
            <ServiceOption
              v-for="s in services"
              :key="s.id"
              :service="s"
              :selected="serviceId === s.id"
              @select="selectService"
            />
          </div>
        </section>

        <section v-if="step === 3" aria-label="Choose time">
          <h2 class="mt-8 text-3xl tracking-widest text-bone">03 / IZABERI TERMIN</h2>
          <div class="mt-4">
            <DateStrip :days="days" :selected-iso="dateIso" @select="selectDate" />
          </div>
          <p v-if="!dateIso" class="mt-4 text-xl tracking-widest text-ash">
            PRVO IZABERI DAN
          </p>
          <div v-else class="mt-4">
            <p v-if="availLoading" class="text-xl tracking-widest text-ash">UČITAVANJE...</p>
            <div v-else-if="availError">
              <p
                role="alert"
                class="border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
              >
                {{ availError }}
              </p>
              <button
                type="button"
                class="mt-3 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
                @click="loadAvailability"
              >
                POKUŠAJ PONOVO
              </button>
            </div>
            <p v-else-if="avail?.closed" class="text-xl tracking-widest text-ash">
              ZATVORENO
            </p>
            <div v-else>
              <TimeGrid :slots="slots" :selected-time="time" @select="selectTime" />
              <p v-if="!time" class="mt-4 text-xl tracking-widest text-ash">IZABERI TERMIN</p>
            </div>
          </div>
        </section>

        <section v-if="step === 4" aria-label="Reserve">
          <h2 class="mt-8 text-3xl tracking-widest text-bone">04 / REZERVACIJA</h2>
          <div
            v-if="selectedBarber && selectedService && dateLabel && time"
            class="mt-4"
          >
            <BookingSummary
              :barber-name="selectedBarber.firstName + ' ' + selectedBarber.lastName"
              :service-name="selectedService.name"
              :duration-min="selectedService.durationMinutes"
              :price="selectedService.price"
              :date-label="dateLabel"
              :time="time"
            />
          </div>
          <label
            for="booking-notes"
            class="mt-6 block text-lg tracking-widest text-ash"
          >
            NAPOMENA (OPCIONO)
          </label>
          <textarea
            id="booking-notes"
            v-model="notes"
            rows="3"
            placeholder="NEŠTO ŠTO BERBERIN TREBA DA ZNA"
            maxlength="500"
            class="mt-2 w-full border border-line bg-ink px-3 py-2 text-lg tracking-widest text-bone placeholder:text-ash"
          />
          <p
            v-if="reserveError"
            role="alert"
            class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
          >
            {{ reserveError }}
          </p>
          <button
            type="button"
            :disabled="reserveLoading"
            class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
            @click="reserve"
          >
            {{ reserveLoading ? 'UČITAVANJE...' : 'REZERVIŠI' }}
          </button>
          <button
            type="button"
            class="mt-3 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
            @click="back"
          >
            NAZAD
          </button>
        </section>
      </div>
    </div>

    <div
      v-if="!booked && step < 4"
      class="sticky bottom-0 mx-auto w-full border-t border-line bg-ink lg:w-1/2"
    >
      <div class="flex items-center justify-between gap-3 px-4 py-3">
        <p class="truncate text-lg tracking-widest text-ash">{{ recap || 'ZAKAŽI ŠIŠANJE' }}</p>
        <div class="flex shrink-0 gap-2">
          <button
            v-if="step > 1"
            type="button"
            class="border border-line px-5 py-2 text-xl tracking-widest text-bone hover:border-bone"
            @click="back"
          >
            NAZAD
          </button>
          <button
            v-if="step === 3"
            type="button"
            :disabled="!canContinue"
            class="bg-bone px-6 py-2 text-xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
            @click="next"
          >
            DALJE
          </button>
        </div>
      </div>
    </div>
  </main>
</template>
