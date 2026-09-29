<script setup lang="ts">
withDefaults(defineProps<{ title: string; subtitle?: string }>(), {
  subtitle: '',
});
</script>

<template>
  <div
    class="mb-6 flex flex-wrap items-start justify-between gap-4 border-b border-line pb-4"
  >
    <div class="min-w-0">
      <h1 class="text-xl font-semibold tracking-tight text-ink">{{ title }}</h1>
      <p v-if="subtitle" class="mt-1 text-[13px] text-muted">{{ subtitle }}</p>
      <slot name="meta" />
    </div>
    <div v-if="$slots.actions" class="flex shrink-0 flex-wrap gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>
