<script setup lang="ts">
import { RouterView } from 'vue-router';
</script>

<template>
  <div class="min-h-screen bg-ink text-bone">
    <RouterView />
    <footer class="mt-4 py-6 text-center">
      <a
        href="https://ginstudio.rs"
        target="_blank"
        rel="noopener"
        class="text-lg tracking-widest text-ash hover:text-bone"
      >
        BUILT BY GINSTUDIO.RS
      </a>
    </footer>
  </div>
</template>
