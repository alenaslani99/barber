<script setup lang="ts">
import UiAlert from '../components/ui/UiAlert.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';

const rows = [
  { label: 'API base URL', value: 'not configured' },
  { label: 'Mode', value: 'Localhost only' },
  { label: 'Auth', value: 'None (localhost session)' },
];
</script>

<template>
  <div>
    <UiPageHeader title="Settings" subtitle="Local session configuration." />

    <div class="grid gap-4 lg:grid-cols-3">
      <div class="lg:col-span-2">
        <UiCard title="Session" description="Read-only. No secrets are stored in the browser.">
          <dl class="divide-y divide-line">
            <div
              v-for="row in rows"
              :key="row.label"
              class="flex items-center justify-between gap-4 py-3 first:pt-0 last:pb-0"
            >
              <dt class="text-[13px] text-muted">{{ row.label }}</dt>
              <dd class="font-mono text-[13px] text-ink">{{ row.value }}</dd>
            </div>
          </dl>
        </UiCard>
      </div>

      <div>
        <UiAlert tone="warning" title="Planned">
          Admin authentication is deliberately not wired yet. The router already reads
          <code class="font-mono">meta.requiresAuth</code>, so adding it is a policy change
          rather than a refactor.
        </UiAlert>
      </div>
    </div>
  </div>
</template>
