<script setup lang="ts">
import UiAlert from '../components/ui/UiAlert.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';
import UiSpinner from '../components/ui/UiSpinner.vue';

const checks = [
  { label: 'API reachable', detail: 'No request sent yet.' },
  { label: 'Catalog database', detail: 'Connection string not resolved.' },
  { label: 'Tenant databases', detail: 'No tenants provisioned.' },
];
</script>

<template>
  <div>
    <UiPageHeader title="Health" subtitle="Local environment status." />

    <UiAlert tone="info" title="Not implemented">
      Health checks run against the API once the admin endpoints exist. Nothing is polled
      from the browser in this round.
    </UiAlert>

    <div class="mt-4">
      <UiCard title="Checks">
        <ul class="divide-y divide-line">
          <li
            v-for="check in checks"
            :key="check.label"
            class="flex items-center gap-3 py-3 first:pt-0 last:pb-0"
          >
            <UiSpinner class="size-4 text-muted" />
            <div class="min-w-0">
              <p class="text-[13px] font-semibold text-ink">{{ check.label }}</p>
              <p class="text-[12px] text-muted">{{ check.detail }}</p>
            </div>
            <span class="ml-auto text-[12px] font-semibold tracking-wider text-muted uppercase">
              Pending
            </span>
          </li>
        </ul>
      </UiCard>
    </div>
  </div>
</template>
