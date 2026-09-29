<script setup lang="ts">
import { Building2, ClipboardList, Server, UserCog } from '@lucide/vue';
import UiCard from '../components/ui/UiCard.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';

interface StatTile {
  label: string;
  value: string;
  hint: string;
  icon: typeof Building2;
}

const tiles: StatTile[] = [
  { label: 'Tenants', value: '—', hint: 'awaiting API', icon: Building2 },
  { label: 'Shop Setup', value: '—', hint: 'awaiting API', icon: ClipboardList },
  { label: 'Owner Accounts', value: '—', hint: 'awaiting API', icon: UserCog },
  { label: 'API Status', value: '—', hint: 'not checked', icon: Server },
];
</script>

<template>
  <div>
    <UiPageHeader
      title="Dashboard"
      subtitle="Platform overview across all barbershop tenants."
    />

    <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <UiCard v-for="tile in tiles" :key="tile.label">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-[12px] font-semibold tracking-wider text-muted uppercase">
              {{ tile.label }}
            </p>
            <p class="mt-2 text-2xl font-semibold text-ink">{{ tile.value }}</p>
            <p class="mt-0.5 text-[12px] text-muted">{{ tile.hint }}</p>
          </div>
          <div class="grid size-9 place-items-center rounded-card bg-brand-soft text-brand">
            <component :is="tile.icon" class="size-4" aria-hidden="true" />
          </div>
        </div>
      </UiCard>
    </div>

    <div class="mt-6">
      <UiCard title="Recent activity" description="Provisioning events will appear here.">
        <p class="py-8 text-center text-[13px] text-muted">
          Nothing yet. Create your first tenant to get started.
        </p>
      </UiCard>
    </div>
  </div>
</template>
