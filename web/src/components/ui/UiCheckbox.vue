<script setup lang="ts">
import { Check } from '@lucide/vue';

defineProps<{ label: string }>();

const checked = defineModel<boolean>({ default: false });
</script>

<template>
  <label class="flex cursor-pointer items-center gap-3">
    <input v-model="checked" type="checkbox" class="peer sr-only" />
    <span
      aria-hidden="true"
      class="flex h-6 w-6 items-center justify-center border border-line bg-ink text-transparent peer-checked:border-bone peer-checked:bg-bone peer-checked:text-ink peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-bone"
    >
      <Check class="h-4 w-4" />
    </span>
    <span class="text-lg tracking-widest text-ash">{{ label }}</span>
  </label>
</template>
