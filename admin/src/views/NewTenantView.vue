<script setup lang="ts">
import { ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { Database, Info } from '@lucide/vue';
import UiAlert from '../components/ui/UiAlert.vue';
import UiButton from '../components/ui/UiButton.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiInput from '../components/ui/UiInput.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';
import { useTenantStore } from '../stores/tenant';

const router = useRouter();
const { setSlug } = useTenantStore();

const slug = ref('');
const databaseName = ref('');

const SLUG_RULE = /^[a-z0-9][a-z0-9-]{1,62}$/;
const DB_RULE = /^[a-z][a-z0-9_]{2,62}$/;

const derived = ref('');
const slugValid = ref(false);
const dbEdited = ref(false);

// Auto-derive the database name from the slug until the operator overrides it.
const dbValid = ref(false);

watch(slug, (value) => {
  const clean = value.trim().toLowerCase();
  slugValid.value = SLUG_RULE.test(clean);
  if (!dbEdited.value) {
    derived.value = slugValid.value ? `barber_t_${clean.replace(/-/g, '_')}` : '';
    databaseName.value = derived.value;
  }
});

watch(databaseName, (value) => {
  dbEdited.value = value !== derived.value;
  dbValid.value = DB_RULE.test(value.trim());
});

function continueToShop(): void {
  const clean = slug.value.trim().toLowerCase();
  setSlug(clean);
  router.push(`/tenants/${clean}/shop`);
}
</script>

<template>
  <div>
    <UiPageHeader
      title="New Tenant"
      subtitle="Step 1 — create the database and register the tenant in the catalog."
    />

    <div class="grid gap-4 lg:grid-cols-3">
      <div class="lg:col-span-2">
        <UiCard title="Tenant identity" description="Slug and target database.">
          <div class="space-y-5">
            <UiInput
              id="slug"
              v-model="slug"
              label="Slug"
              placeholder="e.g. demo"
              required
              hint="Used as the X-Tenant-Slug header by the shop client."
              :rules="[
                (v) => (!v.trim() ? 'Slug is required.' : ''),
                (v) => (v.trim() && !SLUG_RULE.test(v.trim()) ? 'Lowercase letters, numbers and dashes only.' : ''),
              ]"
            />

            <UiInput
              id="database"
              v-model="databaseName"
              label="Database name"
              placeholder="barber_t_demo"
              required
              hint="Auto-derived from the slug. Edit to override."
              :rules="[
                (v) => (!v.trim() ? 'Database name is required.' : ''),
                (v) => (v.trim() && !DB_RULE.test(v.trim()) ? 'Start with a letter; letters, numbers and underscores only.' : ''),
              ]"
            />

            <div class="flex flex-wrap gap-2">
              <UiButton :disabled="!slugValid || !dbValid" @click="continueToShop">
                <template #icon><Database class="size-4" aria-hidden="true" /></template>
                Create and continue
              </UiButton>
            </div>
          </div>
        </UiCard>
      </div>

      <div class="space-y-4">
        <UiAlert tone="info" title="What happens next">
          Creating the tenant runs <strong>CREATE DATABASE</strong>, applies every tenant
          migration, then inserts the catalog row. If any part fails the database is
          dropped so nothing is left half-created.
        </UiAlert>

        <UiCard title="Connection preview">
          <p class="mb-2 flex items-center gap-2 text-[12px] font-semibold tracking-wider text-muted uppercase">
            <Info class="size-3.5" aria-hidden="true" />
            Resolved
          </p>
          <p class="font-mono text-[12px] break-all text-ink">
            Host=localhost;Port=5432;Database={{ databaseName || '—' }}
          </p>
        </UiCard>
      </div>
    </div>
  </div>
</template>
