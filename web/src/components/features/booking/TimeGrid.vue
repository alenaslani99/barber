<script setup lang="ts">
import { cn } from '../../../utils/cn';
import type { TimeSlot } from '../../../data/mock';

defineProps<{ slots: TimeSlot[]; selectedTime: string | null }>();
defineEmits<{ (e: 'select', time: string): void }>();
</script>

<template>
  <div class="grid grid-cols-3 gap-2 sm:grid-cols-4" role="group" aria-label="Pick a time">
    <button
      v-for="s in slots"
      :key="s.id"
      type="button"
      :disabled="!s.available"
      :aria-pressed="selectedTime === s.time"
      :class="
        cn(
          'border py-2 text-xl tracking-widest',
          selectedTime === s.time
            ? 'border-bone bg-bone text-ink'
            : 'border-line bg-surface text-bone hover:border-bone',
          !s.available && 'cursor-not-allowed opacity-30 hover:border-line',
          !s.available && 'line-through',
        )
      "
      @click="$emit('select', s.time)"
    >
      {{ s.time }}
    </button>
  </div>
</template>
