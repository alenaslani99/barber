<script setup lang="ts">
import { computed } from 'vue';
import { CircleAlert, CircleCheck, Info, TriangleAlert } from '@lucide/vue';
import { cn } from '../../utils/cn';

type Tone = 'info' | 'success' | 'warning' | 'danger';

const props = withDefaults(
  defineProps<{ tone?: Tone; title?: string }>(),
  { tone: 'info', title: '' },
);

const ICONS = {
  info: Info,
  success: CircleCheck,
  warning: TriangleAlert,
  danger: CircleAlert,
} as const;

const TONES: Record<Tone, string> = {
  info: 'border-brand/30 bg-brand-soft text-ink',
  success: 'border-success/30 bg-success-soft text-ink',
  warning: 'border-warning/30 bg-warning-soft text-ink',
  danger: 'border-danger/30 bg-danger-soft text-ink',
};

const ACCENTS: Record<Tone, string> = {
  info: 'text-brand',
  success: 'text-success',
  warning: 'text-warning',
  danger: 'text-danger',
};

const icon = computed(() => ICONS[props.tone]);
</script>

<template>
  <div
    role="status"
    :class="cn('flex gap-3 rounded-card border px-4 py-3', TONES[tone])"
  >
    <component :is="icon" :class="cn('mt-0.5 size-4 shrink-0', ACCENTS[tone])" aria-hidden="true" />
    <div class="min-w-0 text-[13px] leading-5">
      <p v-if="title" class="font-semibold">{{ title }}</p>
      <div :class="title ? 'mt-0.5 text-ink/80' : 'text-ink/80'">
        <slot />
      </div>
    </div>
  </div>
</template>
