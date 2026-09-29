<script setup lang="ts">
import { computed } from 'vue';
import { cn } from '../../utils/cn';

type Tone = 'neutral' | 'brand' | 'success' | 'warning' | 'danger';

const props = withDefaults(
  defineProps<{ tone?: Tone; dot?: boolean }>(),
  { tone: 'neutral', dot: false },
);

const TONES: Record<Tone, string> = {
  neutral: 'bg-canvas text-muted border-line',
  brand: 'bg-brand-soft text-brand border-brand/25',
  success: 'bg-success-soft text-success border-success/25',
  warning: 'bg-warning-soft text-warning border-warning/25',
  danger: 'bg-danger-soft text-danger border-danger/25',
};

const DOTS: Record<Tone, string> = {
  neutral: 'bg-muted',
  brand: 'bg-brand',
  success: 'bg-success',
  warning: 'bg-warning',
  danger: 'bg-danger',
};

const classes = computed(() =>
  cn(
    'inline-flex items-center gap-1.5 rounded-field border px-2 py-0.5 text-[11px] font-semibold tracking-wider uppercase',
    TONES[props.tone],
  ),
);
</script>

<template>
  <span :class="classes">
    <span
      v-if="dot"
      class="size-1.5 rounded-full"
      :class="DOTS[tone]"
      aria-hidden="true"
    />
    <slot />
  </span>
</template>
