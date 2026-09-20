<script setup lang="ts">
import { cn } from '../../../utils/cn';
import type { DayOption } from '../../../data/mock';

defineProps<{ days: DayOption[]; selectedIso: string | null }>();
defineEmits<{ (e: 'select', iso: string): void }>();
</script>

<template>
  <div class="flex gap-2 overflow-x-auto pb-2" role="group" aria-label="Pick a day">
    <button
      v-for="d in days"
      :key="d.iso"
      type="button"
      :disabled="d.closed"
      :aria-pressed="selectedIso === d.iso"
      :class="
        cn(
          'flex w-16 shrink-0 flex-col items-center border py-2 tracking-widest',
          selectedIso === d.iso
            ? 'border-bone bg-bone text-ink'
            : 'border-line bg-surface text-bone hover:border-bone',
          d.closed && 'cursor-not-allowed opacity-40 hover:border-line',
        )
      "
      @click="$emit('select', d.iso)"
    >
      <span :class="cn('text-base tracking-widest', selectedIso === d.iso ? 'text-ink' : 'text-ash')">
        {{ d.weekday }}
      </span>
      <span class="text-3xl leading-none tracking-widest">{{ d.dayNum }}</span>
      <span :class="cn('text-base tracking-widest', selectedIso === d.iso ? 'text-ink' : 'text-ash')">
        {{ d.closed ? 'NE RADI' : d.month }}
      </span>
    </button>
  </div>
</template>
