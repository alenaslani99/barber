<script setup lang="ts">
import type { BookingStep } from '../../../types/booking';
import { cn } from '../../../utils/cn';

interface Props {
  steps: BookingStep[];
  currentIndex: number;
}

const props = defineProps<Props>();
</script>

<template>
  <ol aria-label="Booking progress" class="flex items-center gap-2">
    <li
      v-for="(step, index) in props.steps"
      :key="step.key"
      :aria-current="index === props.currentIndex ? 'step' : undefined"
      class="flex flex-1 items-center gap-2 last:flex-none"
    >
      <span
        :class="
          cn(
            'h-1.5 flex-1 rounded-full',
            index < props.currentIndex ? 'bg-primary' : index === props.currentIndex ? 'bg-accent' : 'bg-black/10',
          )
        "
      />
      <span
        :class="
          cn(
            'hidden text-xs font-semibold sm:inline',
            index === props.currentIndex ? 'text-text' : 'text-text/50',
          )
        "
      >
        {{ step.label }}
      </span>
    </li>
  </ol>
</template>
