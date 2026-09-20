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
  <ol aria-label="Booking progress" class="grid grid-cols-5 gap-2">
    <li
      v-for="(step, index) in props.steps"
      :key="step.key"
      :aria-current="index === props.currentIndex ? 'step' : undefined"
      class="min-w-0"
    >
      <span
        :class="
          cn(
            'block h-2 rounded-full',
            index <= props.currentIndex ? 'bg-accent' : 'bg-white/10',
          )
        "
      />
      <span
        :class="
          cn(
            'mt-2 block truncate text-[11px] font-semibold sm:text-xs',
            index === props.currentIndex ? 'text-accent' : index < props.currentIndex ? 'text-text' : 'text-muted',
          )
        "
      >
        {{ step.label }}
      </span>
    </li>
  </ol>
</template>
