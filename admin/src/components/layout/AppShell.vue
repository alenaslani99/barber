<script setup lang="ts">
import { ref } from 'vue';
import { RouterView } from 'vue-router';
import AppSidebar from './AppSidebar.vue';
import AppTopbar from './AppTopbar.vue';

const navOpen = ref(false);
</script>

<template>
  <div class="min-h-screen bg-canvas">
    <AppSidebar v-model:open="navOpen" />

    <div class="flex min-h-screen flex-col lg:pl-64">
      <AppTopbar v-model:open="navOpen" />

      <main class="flex-1 px-4 py-6 lg:px-6">
        <div class="mx-auto w-full max-w-[1400px]">
          <RouterView />
        </div>
      </main>
    </div>
  </div>
</template>
