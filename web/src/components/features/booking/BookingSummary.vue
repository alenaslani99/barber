<script setup lang="ts">
defineProps<{
  barberName: string;
  serviceName: string;
  durationMin: number;
  price: number;
  dateLabel: string;
  time: string;
}>();
</script>

<template>
  <ul aria-label="Rezime rezervacije" class="border-y border-line">
    <li class="flex items-center justify-between py-2">
      <span class="text-lg tracking-widest text-ash">BERBERIN</span>
      <span class="text-xl tracking-widest text-bone">{{ barberName }}</span>
    </li>
    <li class="flex items-center justify-between border-t border-line py-2">
      <span class="text-lg tracking-widest text-ash">USLUGA</span>
      <span class="text-xl tracking-widest text-bone">{{ serviceName }}</span>
    </li>
    <li class="flex items-center justify-between border-t border-line py-2">
      <span class="text-lg tracking-widest text-ash">KADA</span>
      <span class="text-xl tracking-widest text-bone">{{ dateLabel }} / {{ time }}</span>
    </li>
    <li class="flex items-center justify-between border-t border-line py-2">
      <span class="text-lg tracking-widest text-ash">TRAJANJE</span>
      <span class="text-xl tracking-widest text-bone">{{ durationMin }} MIN</span>
    </li>
    <li class="flex items-center justify-between border-t border-line py-2">
      <span class="text-lg tracking-widest text-ash">UKUPNO</span>
      <span class="text-3xl tracking-widest text-bone">{{ price }}</span>
    </li>
  </ul>
</template>
