<script setup lang="ts">
import { onBeforeUnmount, ref } from 'vue';
import { Check, Copy, Eye, EyeOff } from '@lucide/vue';

const props = withDefaults(
  defineProps<{
    label: string;
    value: string;
    /** Masked until the eye toggle is pressed. Copy always copies the real value. */
    secret?: boolean;
  }>(),
  { secret: false },
);

const revealed = ref(false);
const copied = ref(false);
const copyFailed = ref(false);
let resetTimer: ReturnType<typeof setTimeout> | undefined;

async function copy(): Promise<void> {
  try {
    await navigator.clipboard.writeText(props.value);
    copied.value = true;
    copyFailed.value = false;
  } catch {
    copyFailed.value = true;
  }
  clearTimeout(resetTimer);
  resetTimer = setTimeout(() => {
    copied.value = false;
    copyFailed.value = false;
  }, 1500);
}

onBeforeUnmount(() => clearTimeout(resetTimer));
</script>

<template>
  <div>
    <p class="mb-1.5 text-[12px] font-semibold tracking-wider text-muted uppercase">{{ label }}</p>
    <div class="flex items-center gap-1 rounded-field border border-line-strong bg-canvas py-1 pr-1 pl-3">
      <span class="min-w-0 flex-1 truncate font-mono text-[14px] text-ink">
        {{ secret && !revealed ? '•'.repeat(value.length) : value }}
      </span>

      <button
        v-if="secret"
        type="button"
        class="grid size-8 shrink-0 place-items-center rounded-field text-muted hover:bg-surface hover:text-ink"
        :aria-label="revealed ? `Hide ${label}` : `Show ${label}`"
        :aria-pressed="revealed"
        :title="revealed ? 'Hide' : 'Show'"
        @click="revealed = !revealed"
      >
        <EyeOff v-if="revealed" class="size-4" aria-hidden="true" />
        <Eye v-else class="size-4" aria-hidden="true" />
      </button>

      <button
        type="button"
        class="grid size-8 shrink-0 place-items-center rounded-field text-muted hover:bg-surface hover:text-ink"
        :aria-label="`Copy ${label}`"
        :title="copied ? 'Copied' : copyFailed ? 'Copy failed' : 'Copy'"
        @click="copy"
      >
        <Check v-if="copied" class="size-4 text-success" aria-hidden="true" />
        <Copy v-else :class="['size-4', copyFailed && 'text-danger']" aria-hidden="true" />
      </button>
    </div>
    <p class="sr-only" aria-live="polite">{{ copied ? `${label} copied` : '' }}</p>
  </div>
</template>
