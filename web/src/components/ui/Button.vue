<script setup lang="ts">
import { cn } from '../../utils/cn';

type ButtonVariant = 'primary' | 'accent' | 'ghost';

interface Props {
  variant?: ButtonVariant;
  type?: 'button' | 'submit';
  disabled?: boolean;
  class?: string;
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  type: 'button',
  disabled: false,
  class: '',
});

const variantClasses: Record<ButtonVariant, string> = {
  primary: 'bg-primary text-white',
  accent: 'bg-accent text-white',
  ghost: 'border border-black/8 bg-white text-text',
};
</script>

<template>
  <button
    :type="props.type"
    :disabled="props.disabled"
    :class="
      cn(
        'rounded-md px-4 py-3 text-base font-semibold disabled:cursor-not-allowed disabled:opacity-50',
        variantClasses[props.variant],
        props.class,
      )
    "
  >
    <slot />
  </button>
</template>
