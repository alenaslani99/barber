<script setup lang="ts">
import { Check } from '@lucide/vue';
import type { Service } from '../../../types/booking';
import { formatDuration, formatPrice } from '../../../utils/format';
import { cn } from '../../../utils/cn';

interface Props {
  services: Service[];
  selectedId: string | null;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  select: [id: string];
}>();
</script>

<template>
  <div role="radiogroup" aria-label="Choose a service" class="divide-y divide-white/8">
    <button
      v-for="service in props.services"
      :key="service.id"
      type="button"
      role="radio"
      :aria-checked="service.id === props.selectedId"
      :class="
        cn(
          'flex min-h-14 w-full items-center gap-4 px-2 py-4 text-left',
          service.id === props.selectedId && 'bg-accent/10',
        )
      "
      @click="emit('select', service.id)"
    >
      <span
        :class="
          cn(
            'flex h-6 w-6 shrink-0 items-center justify-center rounded-full border-2',
            service.id === props.selectedId ? 'border-accent bg-accent text-black' : 'border-muted text-transparent',
          )"
        aria-hidden="true"
      >
        <Check :size="14" :stroke-width="3" />
      </span>
      <span class="min-w-0 flex-1">
        <span class="block truncate text-base font-semibold">{{ service.name }}</span>
        <span class="mt-0.5 block text-sm text-muted">{{ formatDuration(service.durationMinutes) }}</span>
      </span>
      <span class="shrink-0 text-base font-semibold tabular-nums">{{ formatPrice(service.priceCents) }}</span>
    </button>
  </div>
</template>
