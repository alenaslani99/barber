<script setup lang="ts">
import { onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { useShopStore } from '../../stores/shop';

defineProps<{ title: string; description: string }>();

const shopStore = useShopStore();

onMounted(() => {
  void shopStore.load();
});
</script>

<template>
  <main class="flex min-h-screen flex-col justify-center bg-ink text-bone">
    <div class="mx-auto w-full px-4 py-10 lg:w-1/2">
      <p v-if="shopStore.shop" class="text-center text-lg tracking-widest">
        <RouterLink to="/" class="text-ash hover:text-bone" aria-label="Nazad na početnu">
          {{ shopStore.shop.name }}
        </RouterLink>
      </p>
      <h1 class="mt-2 text-center font-display text-6xl leading-none tracking-wide">
        {{ title }}
      </h1>
      <p class="mt-2 text-center text-xl tracking-widest text-ash">{{ description }}</p>
      <div class="mx-auto mt-8 w-full max-w-md">
        <slot name="fields" />
      </div>
      <div class="mt-6 text-center">
        <slot name="footer" />
      </div>
    </div>
  </main>
</template>
