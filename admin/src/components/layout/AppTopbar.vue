<script setup lang="ts">
import { Menu } from '@lucide/vue';

/** Mobile drawer state, owned by AppShell. */
const open = defineModel<boolean>('open', { default: false });
</script>

<template>
  <header
    class="sticky top-0 z-20 flex items-center gap-4 border-b border-line bg-surface px-4 py-2.5 lg:px-6"
  >
    <button
      type="button"
      class="rounded-field p-2 text-muted hover:bg-canvas hover:text-ink lg:hidden"
      aria-label="Open navigation"
      @click="open = !open"
    >
      <Menu class="size-5" aria-hidden="true" />
    </button>

    <div class="flex-1" />

    <span
      class="inline-flex items-center gap-2 rounded-field border border-line bg-canvas px-2.5 py-1"
      title="Local session, no remote access"
    >
      <span class="size-2 rounded-full bg-success" aria-hidden="true" />
      <span class="text-[12px] font-semibold tracking-wide text-ink uppercase">
        Local
      </span>
    </span>
  </header>
</template>
