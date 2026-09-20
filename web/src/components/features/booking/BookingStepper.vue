<script setup lang="ts">
import { Clock, Scissors, Sparkles, Ticket } from '@lucide/vue';
import { cn } from '../../../utils/cn';

defineProps<{ step: number }>();
defineEmits<{ (e: 'go', step: number): void }>();

const steps = [
  { n: 1, label: 'BERBERIN', icon: Scissors },
  { n: 2, label: 'USLUGA', icon: Sparkles },
  { n: 3, label: 'TERMIN', icon: Clock },
  { n: 4, label: 'REZERVACIJA', icon: Ticket },
];
</script>

<template>
  <nav aria-label="Booking steps" class="mt-6 grid grid-cols-4 border border-line">
    <button
      v-for="s in steps"
      :key="s.n"
      type="button"
      :disabled="s.n > step"
      :aria-current="s.n === step ? 'step' : undefined"
      :class="
        cn(
          'flex flex-col items-center border-r border-line py-2 tracking-widest last:border-r-0',
          s.n === step ? 'bg-bone text-ink' : s.n < step ? 'text-bone' : 'text-ash',
          s.n > step && 'opacity-60',
        )
      "
      @click="$emit('go', s.n)"
    >
      <span class="text-sm tracking-widest">0{{ s.n }}</span>
      <span class="text-lg tracking-widest">{{ s.label }}</span>
      <component :is="s.icon" class="mt-1 h-4.5 w-4.5" aria-hidden="true" />
    </button>
  </nav>
</template>
