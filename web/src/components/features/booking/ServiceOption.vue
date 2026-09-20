<script setup lang="ts">
import { cn } from '../../../utils/cn';
import type { Service } from '../../../data/mock';

defineProps<{ service: Service; selected: boolean }>();
defineEmits<{ (e: 'select', id: string): void }>();
</script>

<template>
  <button
    type="button"
    :aria-pressed="selected"
    :class="
      cn(
        'flex w-full items-center justify-between gap-3 border p-4 text-left',
        selected
          ? 'border-bone bg-bone text-ink'
          : 'border-line bg-surface text-bone hover:border-bone',
      )
    "
    @click="$emit('select', service.id)"
  >
    <span>
      <span class="block text-2xl leading-none tracking-widest">{{ service.name }}</span>
      <span :class="cn('block text-lg tracking-widest', selected ? 'text-ink' : 'text-ash')">
        {{ service.description }} / {{ service.durationMin }} MIN
      </span>
    </span>
    <span class="shrink-0 text-3xl tracking-widest">{{ service.price }}</span>
  </button>
</template>
