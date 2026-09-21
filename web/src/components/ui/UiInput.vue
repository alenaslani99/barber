<script setup lang="ts">
import { computed, ref } from 'vue';
import { Eye, EyeOff } from '@lucide/vue';
import { cn } from '../../utils/cn';

const props = withDefaults(
  defineProps<{
    id: string;
    label: string;
    type?: 'text' | 'email' | 'tel' | 'password';
    placeholder?: string;
    autocomplete?: string;
    rules?: Array<(value: string) => string>;
  }>(),
  { type: 'text', placeholder: '', autocomplete: 'off', rules: () => [] },
);

const value = defineModel<string>({ default: '' });
const error = ref('');
const showPassword = ref(false);

const isPassword = computed(() => props.type === 'password');
const inputType = computed(() =>
  isPassword.value && showPassword.value ? 'text' : props.type,
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

defineExpose({ validate, setError });
</script>

<template>
  <div>
    <label :for="id" class="block text-lg tracking-widest text-ash">{{ label }}</label>
    <div class="relative mt-1">
      <input
        :id="id"
        v-model="value"
        :type="inputType"
        :placeholder="placeholder"
        :autocomplete="autocomplete"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${id}-error` : undefined"
        :class="
          cn(
            'w-full border bg-ink px-3 py-2 font-form text-xl tracking-normal text-bone normal-case outline-none placeholder:text-ash placeholder:opacity-50',
            error ? 'border-alarm focus:border-alarm' : 'border-line focus:border-bone',
            isPassword && 'pr-11',
          )
        "
      />
      <button
        v-if="isPassword"
        type="button"
        :aria-label="showPassword ? 'Sakrij lozinku' : 'Prikaži lozinku'"
        class="absolute right-3 top-1/2 -translate-y-1/2 text-ash hover:text-bone"
        @click="showPassword = !showPassword"
      >
        <EyeOff v-if="showPassword" class="h-5 w-5" aria-hidden="true" />
        <Eye v-else class="h-5 w-5" aria-hidden="true" />
      </button>
    </div>
    <p
      :id="`${id}-error`"
      aria-live="polite"
      class="mt-1 min-h-6 text-lg tracking-widest text-alarm"
    >
      {{ error }}
    </p>
  </div>
</template>
