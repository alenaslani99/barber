<script setup lang="ts">
import { RouterLink } from 'vue-router';
import { Building2, Plus } from '@lucide/vue';
import UiButton from '../components/ui/UiButton.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';
</script>

<template>
  <div>
    <UiPageHeader
      title="All Clients"
      subtitle="Every tenant registered in the catalog database."
    >
      <template #actions>
        <RouterLink to="/tenants/new">
          <UiButton>
            <template #icon><Plus class="size-4" aria-hidden="true" /></template>
            New Tenant
          </UiButton>
        </RouterLink>
      </template>
    </UiPageHeader>

    <UiCard>
      <div class="grid place-items-center px-6 py-16 text-center">
        <div class="grid size-12 place-items-center rounded-card bg-canvas text-muted">
          <Building2 class="size-6" aria-hidden="true" />
        </div>
        <h2 class="mt-4 text-[15px] font-semibold text-ink">No tenants yet</h2>
        <p class="mt-1 max-w-sm text-[13px] text-muted">
          Provisioning a tenant creates its database, applies migrations and seeds the
          shop. Start with step one.
        </p>
        <RouterLink to="/tenants/new" class="mt-5">
          <UiButton>
            <template #icon><Plus class="size-4" aria-hidden="true" /></template>
            Create first tenant
          </UiButton>
        </RouterLink>
      </div>
    </UiCard>
  </div>
</template>
