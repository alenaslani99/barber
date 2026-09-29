<script setup lang="ts">
import { onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import StepShell from '../components/features/onboarding/StepShell.vue';
import UiAlert from '../components/ui/UiAlert.vue';
import UiBadge from '../components/ui/UiBadge.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';
import { useTenantStore } from '../stores/tenant';

const route = useRoute();
const { setSlug } = useTenantStore();

function syncFromRoute(slug: unknown): void {
  if (typeof slug === 'string' && slug.length > 0) setSlug(slug);
}

onMounted(() => syncFromRoute(route.params.slug));
watch(() => route.params.slug, syncFromRoute);
</script>

<template>
  <div>
    <UiPageHeader
      title="Staff & Services"
      subtitle="Step 4 — first barber and first service so the shop is bookable."
    >
      <template #meta>
        <div class="mt-2">
          <UiBadge tone="neutral">Decided later</UiBadge>
        </div>
      </template>
    </UiPageHeader>

    <StepShell :step="4">
      <template #default="{ slug }">
        <UiAlert tone="warning" title="Deferred">
          Scope for this step is not decided yet. Without a barber and a service the shop
          is live but not bookable.
        </UiAlert>

        <div class="mt-4">
          <UiCard title="Seed data">
            <p class="py-6 text-center text-[13px] text-muted">
              Decided in a later pass. Current tenant: <span class="font-mono">{{ slug }}</span>
            </p>
          </UiCard>
        </div>
      </template>
    </StepShell>
  </div>
</template>
