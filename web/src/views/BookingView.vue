<script setup lang="ts">
import { computed, ref } from 'vue';
import { barbers, getNextDays, getSlots, services } from '../data/mock';
import BookingStepper from '../components/features/booking/BookingStepper.vue';
import ShopHeader from '../components/layout/ShopHeader.vue';
import BarberOption from '../components/features/booking/BarberOption.vue';
import ServiceOption from '../components/features/booking/ServiceOption.vue';
import DateStrip from '../components/features/booking/DateStrip.vue';
import TimeGrid from '../components/features/booking/TimeGrid.vue';
import BookingSummary from '../components/features/booking/BookingSummary.vue';

type Step = 1 | 2 | 3 | 4;

const step = ref<Step>(1);
const barberId = ref<string | null>(null);
const serviceId = ref<string | null>(null);
const dateIso = ref<string | null>(null);
const time = ref<string | null>(null);
const notes = ref('');
const booked = ref(false);
const bookingRef = ref('');

const days = getNextDays(14);

const selectedBarber = computed(() => barbers.find((b) => b.id === barberId.value) ?? null);
const selectedService = computed(
  () => services.find((s) => s.id === serviceId.value) ?? null,
);
const slots = computed(() => (dateIso.value ? getSlots(dateIso.value, barberId.value) : []));
const dateLabel = computed(() => {
  const d = days.find((day) => day.iso === dateIso.value);
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
  if (selectedBarber.value) parts.push(selectedBarber.value.name);
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
}

function selectService(id: string): void {
  serviceId.value = id;
  step.value = 3;
  scrollTop();
}

function selectDate(iso: string): void {
  dateIso.value = iso;
  time.value = null;
}

function selectTime(t: string): void {
  time.value = t;
}

function next(): void {
  if (step.value < 4 && canContinue.value) {
    step.value = (step.value + 1) as Step;
    scrollTop();
  }
}

function back(): void {
  if (step.value > 1) {
    step.value = (step.value - 1) as Step;
    scrollTop();
  }
}

function goTo(s: number): void {
  if (s >= 1 && s <= 4 && s < step.value) step.value = s as Step;
}

function reserve(): void {
  const code = Math.random().toString(36).slice(2, 8).toUpperCase();
  bookingRef.value = `BK-${code}`;
  booked.value = true;
  scrollTop();
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
  scrollTop();
}
</script>

<template>
  <ShopHeader />
  <main class="bg-ink text-bone">
    <div class="mx-auto mt-16 w-full px-4 pb-32 lg:w-1/2">
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
            :barber-name="selectedBarber.name"
            :service-name="selectedService.name"
            :duration-min="selectedService.durationMin"
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
          <div class="mt-4 flex flex-col gap-3">
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
          <div class="mt-4 flex flex-col gap-3">
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
            <TimeGrid :slots="slots" :selected-time="time" @select="selectTime" />
            <p v-if="!time" class="mt-4 text-xl tracking-widest text-ash">IZABERI TERMIN</p>
          </div>
        </section>

        <section v-if="step === 4" aria-label="Reserve">
          <h2 class="mt-8 text-3xl tracking-widest text-bone">04 / REZERVACIJA</h2>
          <div
            v-if="selectedBarber && selectedService && dateLabel && time"
            class="mt-4"
          >
            <BookingSummary
              :barber-name="selectedBarber.name"
              :service-name="selectedService.name"
              :duration-min="selectedService.durationMin"
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
            class="mt-2 w-full border border-line bg-ink px-3 py-2 text-lg tracking-widest text-bone placeholder:text-ash"
          />
          <button
            type="button"
            class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90"
            @click="reserve"
          >
            REZERVIŠI
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
      class="fixed inset-x-0 bottom-0 border-t border-line bg-ink"
    >
      <div class="mx-auto flex w-full items-center justify-between gap-3 px-4 py-3 lg:w-1/2">
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
