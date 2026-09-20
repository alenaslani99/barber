<script setup lang="ts">
import { ArrowLeft } from '@lucide/vue';
import { RouterLink } from 'vue-router';

defineProps<{ title: string; description: string }>();
</script>

<template>
  <main class="flex min-h-screen flex-col justify-center bg-ink text-bone">
    <div class="mx-auto w-full px-4 py-10 lg:w-1/2">
      <RouterLink
        to="/"
        class="inline-flex items-center gap-1 text-lg tracking-widest text-ash hover:text-bone"
        aria-label="Nazad na početnu"
      >
        <ArrowLeft class="h-4 w-4" aria-hidden="true" />
        POČETNA
      </RouterLink>
      <h1 class="text-center font-display text-6xl leading-none tracking-wide">
        {{ title }}
      </h1>
      <p class="mt-2 text-center text-xl tracking-widest text-ash">{{ description }}</p>
      <div class="mt-8">
        <slot name="fields" />
      </div>
      <div class="mt-6 text-center">
        <slot name="footer" />
      </div>
    </div>
  </main>
</template>
