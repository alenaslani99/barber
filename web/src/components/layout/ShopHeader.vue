<script setup lang="ts">
import { computed, ref } from 'vue';
import { ChevronDown, ChevronUp, User } from '@lucide/vue';
import { RouterLink } from 'vue-router';
import { shop } from '../../data/mock';
import { cn } from '../../utils/cn';

const hoursOpen = ref(false);

const todaySummary = computed(() => {
  const idx = (new Date().getDay() + 6) % 7;
  const h = shop.hours[idx];
  return h.closed ? 'DANAS ZATVORENO' : `DANAS ${h.open} - ${h.close}`;
});
</script>

<template>
  <header class="bg-ink text-bone" aria-label="Barbershop info">
    <div class="mx-auto w-full border-b border-line px-4 lg:w-1/2">
      <div class="flex items-center justify-between py-3">
        <p class="text-lg tracking-widest text-ash">{{ shop.tagline }}</p>
        <RouterLink
          to="/login"
          class="flex items-center gap-2 border border-blaze px-4 py-1 text-lg tracking-widest text-blaze hover:bg-blaze hover:text-ink"
          aria-label="Prijava"
        >
          <User class="h-4 w-4" aria-hidden="true" />
          PRIJAVA
        </RouterLink>
      </div>

      <h1 class="font-display text-6xl leading-none tracking-wide sm:text-7xl">
        {{ shop.name }}
      </h1>
      <p class="mt-2 text-xl tracking-widest text-ash">{{ shop.description }}</p>

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
            v-for="h in shop.hours"
            :key="h.day"
            :class="cn('flex items-center justify-between py-2', h.closed && 'opacity-50')"
          >
            <span class="text-lg tracking-widest text-ash">{{ h.day }}</span>
            <span class="text-lg tracking-widest text-bone">
              {{ h.closed ? 'CLOSED' : `${h.open} - ${h.close}` }}
            </span>
          </li>
        </ul>
      </div>

      <div class="flex items-center justify-between gap-4 py-3">
        <a
          :href="shop.addressHref"
          target="_blank"
          rel="noopener"
          class="text-lg tracking-widest text-ash hover:text-bone"
        >
          {{ shop.address }}
        </a>
        <a :href="shop.phoneHref" class="text-lg tracking-widest text-bone hover:text-blaze">
          {{ shop.phone }}
        </a>
      </div>
    </div>
  </header>
</template>
