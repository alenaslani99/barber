<script setup lang="ts">
import { useAuthStore } from '../stores/auth';

const auth = useAuthStore();
</script>

<template>
  <main class="bg-ink text-bone">
    <div class="mx-auto w-full px-4 py-10 lg:w-1/2">
      <h1 class="font-display text-6xl leading-none tracking-wide">
        {{ auth.firstName }} {{ auth.lastName }}
      </h1>
    </div>
  </main>
</template>
