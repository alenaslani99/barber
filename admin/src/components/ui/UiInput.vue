<script setup lang="ts">
import { computed, ref } from 'vue';
import { cn } from '../../utils/cn';

const props = withDefaults(
  defineProps<{
    id: string;
    label: string;
    type?: 'text' | 'email' | 'tel' | 'password' | 'search';
    placeholder?: string;
    hint?: string;
    disabled?: boolean;
    required?: boolean;
    rules?: Array<(value: string) => string>;
  }>(),
  {
    type: 'text',
    placeholder: '',
    hint: '',
    disabled: false,
    required: false,
    rules: () => [],
  },
);

const value = defineModel<string>({ default: '' });
const error = ref('');

const revealed = ref(false);

const inputType = computed(() =>
  props.type === 'password' && revealed.value ? 'text' : props.type,
);

function validate(): boolean {
  for (const rule of props.rules) {
    const message = rule(value.value);
    if (message) {
      error.value = message;
      return false;
    }
  }
  error.value = '';
  return true;
}

function setError(message: string): void {
  error.value = message;
}

function reset(): void {
  error.value = '';
}

defineExpose({ validate, setError, reset });
</script>

<template>
  <div>
    <label
      :for="id"
      class="mb-1.5 block text-[12px] font-semibold tracking-wider text-muted uppercase"
    >
      {{ label }}
      <span v-if="required" class="text-danger" aria-hidden="true">*</span>
    </label>

    <input
      :id="id"
      v-model="value"
      :type="inputType"
      :placeholder="placeholder"
      :disabled="disabled"
      :required="required"
      :aria-invalid="!!error"
      :aria-describedby="error ? `${id}-error` : hint ? `${id}-hint` : undefined"
      :class="
        cn(
          'w-full rounded-field border bg-surface px-3 py-2 text-[14px] text-ink',
          'placeholder:text-muted/60 disabled:bg-canvas disabled:text-muted',
          error
            ? 'border-danger focus:border-danger focus:ring-2 focus:ring-danger/25'
            : 'border-line-strong hover:border-muted focus:border-brand focus:ring-2 focus:ring-brand/20',
        )
      "
      @input="reset"
    />

    <p
      v-if="error"
      :id="`${id}-error`"
      aria-live="polite"
      class="mt-1.5 text-[12px] font-medium text-danger"
    >
      {{ error }}
    </p>
    <p v-else-if="hint" :id="`${id}-hint`" class="mt-1.5 text-[12px] text-muted">
      {{ hint }}
    </p>
  </div>
</template>
