<script setup lang="ts">
import { computed } from 'vue';
import { cn } from '../../utils/cn';

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost';
type Size = 'sm' | 'md';

const props = withDefaults(
  defineProps<{
    variant?: Variant;
    size?: Size;
    type?: 'button' | 'submit';
    disabled?: boolean;
    loading?: boolean;
    block?: boolean;
  }>(),
  {
    variant: 'primary',
    size: 'md',
    type: 'button',
    disabled: false,
    loading: false,
    block: false,
  },
);

const VARIANTS: Record<Variant, string> = {
  primary: 'bg-brand text-white hover:bg-brand-hover border-transparent',
  secondary: 'bg-surface text-ink hover:bg-canvas border-line-strong',
  danger: 'bg-danger text-white hover:brightness-90 border-transparent',
  ghost: 'bg-transparent text-muted hover:bg-canvas hover:text-ink border-transparent',
};

const SIZES: Record<Size, string> = {
  sm: 'px-2.5 py-1.5 text-[12px]',
  md: 'px-4 py-2 text-[13px]',
};

const classes = computed(() =>
  cn(
    'inline-flex items-center justify-center gap-2 rounded-field border font-semibold tracking-wide uppercase',
    'transition-colors disabled:cursor-not-allowed disabled:opacity-55',
    VARIANTS[props.variant],
    SIZES[props.size],
    props.block && 'w-full',
  ),
);
</script>

<template>
  <button
    :type="type"
    :class="classes"
    :disabled="disabled || loading"
    :aria-busy="loading"
  >
    <slot name="icon" />
    <slot />
  </button>
</template>
