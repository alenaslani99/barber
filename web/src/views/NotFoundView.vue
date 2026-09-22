<script setup lang="ts">
import { onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { useShopStore } from '../stores/shop';

const shopStore = useShopStore();

onMounted(() => {
  void shopStore.load();
});
</script>

<template>
  <main class="flex min-h-screen flex-col justify-center bg-ink text-bone">
    <div class="mx-auto w-full px-4 py-10 text-center lg:w-1/2">
      <p v-if="shopStore.shop" class="text-lg tracking-widest">
        <RouterLink to="/" class="text-ash hover:text-bone" aria-label="Nazad na početnu">
          {{ shopStore.shop.name }}
        </RouterLink>
      </p>
      <h1 class="mt-2 font-display text-8xl leading-none tracking-wide">404</h1>
      <p class="mt-2 text-xl tracking-widest text-ash">STRANICA NE POSTOJI</p>
      <RouterLink
        to="/"
        class="mt-8 block w-full bg-bone py-3 text-2xl tracking-widest text-ink hover:opacity-90"
      >
        POČETNA
      </RouterLink>
    </div>
  </main>
</template>
