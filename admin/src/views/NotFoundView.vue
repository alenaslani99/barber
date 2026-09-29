<script setup lang="ts">
import { RouterLink } from 'vue-router';
import { FileQuestion } from '@lucide/vue';
import UiButton from '../components/ui/UiButton.vue';
import UiCard from '../components/ui/UiCard.vue';
</script>

<template>
  <div class="grid place-items-center py-20">
    <UiCard class="w-full max-w-md">
      <div class="px-6 py-12 text-center">
        <div class="mx-auto grid size-12 place-items-center rounded-card bg-canvas text-muted">
          <FileQuestion class="size-6" aria-hidden="true" />
        </div>
        <h1 class="mt-4 text-lg font-semibold text-ink">Page not found</h1>
        <p class="mt-1 text-[13px] text-muted">
          That route does not exist in the admin panel.
        </p>
        <RouterLink to="/" class="mt-6 inline-block">
          <UiButton variant="secondary">Back to dashboard</UiButton>
        </RouterLink>
      </div>
    </UiCard>
  </div>
</template>
