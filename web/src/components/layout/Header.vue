<script setup lang="ts">
import Container from './Container.vue';

interface Props {
  shopName: string;
  status?: string;
}

withDefaults(defineProps<Props>(), {
  status: 'Open today',
});
</script>

<template>
  <header class="border-b-2 border-accent bg-surface">
    <Container class="flex min-h-20 flex-col justify-center py-4">
      <p class="font-display text-2xl leading-tight sm:text-3xl">{{ shopName }}</p>
      <p class="mt-1 text-xs font-medium tracking-wide text-muted uppercase">{{ status }}</p>
    </Container>
  </header>
</template>
