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
      title="Owner Account"
      subtitle="Step 3 — create the shop owner login."
    >
      <template #meta>
        <div class="mt-2">
          <UiBadge tone="warning">Not configured</UiBadge>
        </div>
      </template>
    </UiPageHeader>

    <StepShell :step="3" next-to="/tenants/:slug/seed">
      <template #default="{ slug }">
        <UiAlert tone="info" title="Step 3 of 4">
          Working on <strong class="font-mono">{{ slug }}</strong>. The owner password is
          returned once at creation and never stored in the browser.
        </UiAlert>

        <div class="mt-4">
          <UiCard title="Owner details" description="Name, email, phone and password.">
            <p class="py-6 text-center text-[13px] text-muted">
              Form fields land with the API.
            </p>
          </UiCard>
        </div>
      </template>
    </StepShell>
  </div>
</template>
