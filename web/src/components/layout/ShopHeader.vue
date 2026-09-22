<script setup lang="ts">
import { computed, ref } from 'vue';
import { ChevronDown, ChevronUp, User } from '@lucide/vue';
import { RouterLink } from 'vue-router';
import { cn } from '../../utils/cn';
import { useAuthStore } from '../../stores/auth';
import type { ApiShopHour } from '../../lib/catalog';

const DAY_NAMES = [
  'NEDELJA',
  'PONEDELJAK',
  'UTORAK',
  'SREDA',
  'ČETVRTAK',
  'PETAK',
  'SUBOTA',
];

const props = defineProps<{
  name: string;
  tagline: string;
  description: string;
  address: string;
  phone: string;
  hours: ApiShopHour[];
}>();

const auth = useAuthStore();
const hoursOpen = ref(false);

const orderedHours = computed(() => {
  const byDay = new Map(props.hours.map((h) => [h.day, h]));
  const order = [1, 2, 3, 4, 5, 6, 0];
  return order
    .map((day) => byDay.get(day))
    .filter((h): h is ApiShopHour => h !== undefined);
});

const todaySummary = computed(() => {
  const today = props.hours.find((h) => h.day === new Date().getDay());
  if (!today || today.closed) return 'DANAS ZATVORENO';
  return `DANAS ${today.open} - ${today.close}`;
});

const phoneHref = computed(() => `tel:${props.phone.replace(/[^+\d]/g, '')}`);
const addressHref = computed(
  () => `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(props.address)}`,
);
</script>

<template>
  <header class="bg-ink text-bone" aria-label="Barbershop info">
    <div class="mx-auto w-full border-b border-line px-4 lg:w-1/2">
      <div class="flex items-center justify-between py-3">
        <p class="text-lg tracking-widest text-ash">{{ tagline }}</p>
        <div class="flex items-center gap-2">
          <RouterLink
            v-if="auth.canManage"
            to="/owner"
            class="border border-line px-4 py-1 text-lg tracking-widest text-ash hover:border-bone hover:text-bone"
            aria-label="Panel"
          >
            PANEL
          </RouterLink>
          <RouterLink
            v-if="auth.isAuthenticated"
            to="/account"
            class="flex items-center gap-2 border border-blaze px-4 py-1 text-lg tracking-widest text-blaze hover:bg-blaze hover:text-ink"
            aria-label="Nalog"
          >
            <User class="h-4 w-4" aria-hidden="true" />
            {{ auth.displayName }}
          </RouterLink>
          <RouterLink
            v-else
            to="/login"
            class="flex items-center gap-2 border border-blaze px-4 py-1 text-lg tracking-widest text-blaze hover:bg-blaze hover:text-ink"
            aria-label="Prijava"
          >
            <User class="h-4 w-4" aria-hidden="true" />
            PRIJAVA
          </RouterLink>
        </div>
      </div>

      <h1 class="font-display text-6xl leading-none tracking-wide sm:text-7xl">
        {{ name }}
      </h1>
      <p class="mt-2 text-xl tracking-widest text-ash">{{ description }}</p>

      <div class="mt-6 border-y border-line">
        <button
          type="button"
          :aria-expanded="hoursOpen"
          aria-controls="shop-hours"
          class="group flex w-full items-center justify-between gap-4 py-2 text-left"
          @click="hoursOpen = !hoursOpen"
        >
          <span class="text-lg tracking-widest text-ash">RADNO VREME</span>
          <span
            class="flex items-center gap-1 text-lg tracking-widest text-ash group-hover:text-bone"
          >
            {{ hoursOpen ? 'PRIKAŽI MANJE' : 'PRIKAŽI VIŠE' }}
            <ChevronUp v-if="hoursOpen" class="h-4 w-4" aria-hidden="true" />
            <ChevronDown v-else class="h-4 w-4" aria-hidden="true" />
          </span>
          <span class="text-lg tracking-widest text-bone">{{ todaySummary }}</span>
        </button>
        <ul
          v-if="hoursOpen"
          id="shop-hours"
          aria-label="Working hours"
          class="border-t border-line"
        >
          <li
            v-for="h in orderedHours"
            :key="h.day"
            :class="cn('flex items-center justify-between py-2', h.closed && 'opacity-50')"
          >
            <span class="text-lg tracking-widest text-ash">{{ DAY_NAMES[h.day] }}</span>
            <span class="text-lg tracking-widest text-bone">
              {{ h.closed ? 'ZATVORENO' : `${h.open} - ${h.close}` }}
            </span>
          </li>
        </ul>
      </div>

      <div class="flex items-center justify-between gap-4 py-3">
        <a
          :href="addressHref"
          target="_blank"
          rel="noopener"
          class="text-lg tracking-widest text-ash hover:text-bone"
        >
          {{ address }}
        </a>
        <a :href="phoneHref" class="text-lg tracking-widest text-bone hover:text-blaze">
          {{ phone }}
        </a>
      </div>
    </div>
  </header>
</template>
