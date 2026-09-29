<script setup lang="ts">
withDefaults(defineProps<{ title?: string; description?: string }>(), {
  title: '',
  description: '',
});
</script>

<template>
  <section
    class="rounded-card border border-line bg-surface shadow-[0_1px_2px_rgba(22,32,46,0.04)]"
  >
    <header
      v-if="title || $slots.actions"
      class="flex items-start justify-between gap-4 border-b border-line px-5 py-4"
    >
      <div>
        <h2 v-if="title" class="text-[15px] font-semibold text-ink">{{ title }}</h2>
        <p v-if="description" class="mt-0.5 text-[13px] text-muted">
          {{ description }}
        </p>
      </div>
      <div v-if="$slots.actions" class="flex shrink-0 gap-2">
        <slot name="actions" />
      </div>
    </header>
    <div :class="$slots.default ? 'p-5' : ''">
      <slot />
    </div>
  </section>
</template>
