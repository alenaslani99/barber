<script setup lang="ts">
import Container from './Container.vue';

interface Props {
  shopName: string;
}

defineProps<Props>();
</script>

<template>
  <header class="border-b border-black/8 bg-white">
    <Container class="flex h-16 items-center">
      <p class="text-lg font-bold tracking-tight">{{ shopName }}</p>
    </Container>
  </header>
</template>
