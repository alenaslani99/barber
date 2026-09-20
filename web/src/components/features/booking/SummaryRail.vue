<script setup lang="ts">
import { formatDuration, formatPrice } from '../../../utils/format';

interface Props {
  serviceName: string | null;
  durationMinutes: number | null;
  priceCents: number | null;
}

defineProps<Props>();
</script>

<template>
  <div class="rounded-lg border border-white/8 bg-surface p-6 shadow-subtle lg:sticky lg:top-6">
    <h2 class="font-display text-xl">Your chair</h2>
    <dl class="mt-4 space-y-3 text-sm">
      <div class="flex items-baseline gap-2">
        <dt class="shrink-0 text-muted">Service</dt>
        <span aria-hidden="true" class="mx-1 flex-1 border-b border-dotted border-white/15" />
        <dd class="font-semibold">{{ serviceName ?? '—' }}</dd>
      </div>
      <div class="flex items-baseline gap-2">
        <dt class="shrink-0 text-muted">Duration</dt>
        <span aria-hidden="true" class="mx-1 flex-1 border-b border-dotted border-white/15" />
        <dd class="font-semibold tabular-nums">
          {{ durationMinutes === null ? '—' : formatDuration(durationMinutes) }}
        </dd>
      </div>
      <div class="flex items-baseline gap-2 text-base">
        <dt class="shrink-0 text-muted">Total</dt>
        <span aria-hidden="true" class="mx-1 flex-1 border-b border-dotted border-white/15" />
        <dd class="font-bold text-accent tabular-nums">
          {{ priceCents === null ? '—' : formatPrice(priceCents) }}
        </dd>
      </div>
    </dl>
  </div>
</template>
