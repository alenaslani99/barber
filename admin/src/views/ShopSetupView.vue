<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Clock, Save } from '@lucide/vue';
import StepShell from '../components/features/onboarding/StepShell.vue';
import UiAlert from '../components/ui/UiAlert.vue';
import UiBadge from '../components/ui/UiBadge.vue';
import UiButton from '../components/ui/UiButton.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiInput from '../components/ui/UiInput.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';
import UiSpinner from '../components/ui/UiSpinner.vue';
import { ApiError } from '../lib/api';
import { getShop, getTenantSetup, saveShop } from '../lib/tenants';
import { useTenantStore } from '../stores/tenant';

type Field = 'name' | 'tagline' | 'address' | 'phone' | 'timeZone';

const route = useRoute();
const router = useRouter();
const { currentSlug, setSlug } = useTenantStore();

const form = reactive({
  name: '',
  tagline: '',
  address: '',
  phone: '',
  timeZone: 'Europe/Berlin',
  description: '',
});
const descriptionError = ref('');

type InputRef = Ref<InstanceType<typeof UiInput> | null>;
const nameInput: InputRef = ref(null);
const taglineInput: InputRef = ref(null);
const addressInput: InputRef = ref(null);
const phoneInput: InputRef = ref(null);
const timeZoneInput: InputRef = ref(null);
// Keys match the API's validation error keys (PascalCase on the wire).
const inputs: Record<Field, InputRef> = {
  name: nameInput,
  tagline: taglineInput,
  address: addressInput,
  phone: phoneInput,
  timeZone: timeZoneInput,
};

const loading = ref(false);
const saving = ref(false);
const configured = ref(false);
const failure = ref('');

const canSave = computed(() => form.name.trim().length > 0 && !loading.value);

function syncFromRoute(slug: unknown): void {
  if (typeof slug === 'string' && slug.length > 0) setSlug(slug);
}

onMounted(() => syncFromRoute(route.params.slug));
watch(() => route.params.slug, syncFromRoute);

async function load(slug: string): Promise<void> {
  if (!slug) return;
  loading.value = true;
  failure.value = '';
  try {
    const setup = await getTenantSetup(slug);
    configured.value = setup.hasShop;
    if (setup.hasShop) {
      const shop = await getShop(slug);
      form.name = shop.name;
      form.tagline = shop.tagline;
      form.address = shop.address ?? '';
      form.phone = shop.phone ?? '';
      form.timeZone = shop.timeZone;
      form.description = shop.description;
    }
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    failure.value = error.status === 404 ? `Tenant "${slug}" does not exist.` : error.title;
  } finally {
    loading.value = false;
  }
}

watch(currentSlug, load, { immediate: true });

async function save(): Promise<void> {
  if (!(nameInput.value?.validate() ?? false)) return;

  saving.value = true;
  failure.value = '';
  descriptionError.value = '';
  try {
    await saveShop(currentSlug.value, {
      name: form.name.trim(),
      tagline: form.tagline.trim(),
      address: form.address.trim() || null,
      phone: form.phone.trim() || null,
      timeZone: form.timeZone.trim(),
      description: form.description.trim(),
    });
    await router.push(`/tenants/${currentSlug.value}/owner`);
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    let placed = false;
    for (const field of Object.keys(inputs) as Field[]) {
      const key = field.charAt(0).toUpperCase() + field.slice(1);
      const message = error.errors[key]?.[0];
      if (message) {
        inputs[field].value?.setError(message);
        placed = true;
      }
    }
    const descriptionMessage = error.errors.Description?.[0];
    if (descriptionMessage) {
      descriptionError.value = descriptionMessage;
      placed = true;
    }
    if (!placed) failure.value = error.title;
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <div>
    <UiPageHeader title="Shop Setup" subtitle="Step 2 — barbershop details and opening hours.">
      <template #meta>
        <div class="mt-2">
          <UiBadge v-if="configured" tone="success" dot>Configured</UiBadge>
          <UiBadge v-else tone="warning">Not configured</UiBadge>
        </div>
      </template>
    </UiPageHeader>

    <StepShell :step="2">
      <template #default="{ slug }">
        <div v-if="slug" class="grid gap-4 lg:grid-cols-3">
          <form class="lg:col-span-2" novalidate @submit.prevent="save">
            <UiCard
              title="Barbershop details"
              :description="`Shown on the booking page for ${slug}.`"
            >
              <div v-if="loading" class="grid place-items-center py-10 text-muted">
                <UiSpinner class="size-5" />
              </div>

              <div v-else class="space-y-5">
                <UiAlert v-if="failure" tone="danger" title="Shop not saved">
                  {{ failure }}
                </UiAlert>

                <UiInput
                  id="shop-name"
                  ref="nameInput"
                  v-model="form.name"
                  label="Name"
                  placeholder="e.g. Acme Barbers"
                  required
                  :rules="[(v) => (!v.trim() ? 'Name is required.' : '')]"
                />
                <UiInput
                  id="shop-tagline"
                  ref="taglineInput"
                  v-model="form.tagline"
                  label="Tagline"
                  placeholder="e.g. Fresh cuts since 1998"
                  hint="Short line under the shop name. Max 100 characters."
                />

                <div class="grid gap-5 sm:grid-cols-2">
                  <UiInput
                    id="shop-address"
                    ref="addressInput"
                    v-model="form.address"
                    label="Address"
                    placeholder="Zeil 1, 60313 Frankfurt"
                  />
                  <UiInput
                    id="shop-phone"
                    ref="phoneInput"
                    v-model="form.phone"
                    type="tel"
                    label="Phone"
                    placeholder="+49 69 1234567"
                  />
                </div>

                <UiInput
                  id="shop-timezone"
                  ref="timeZoneInput"
                  v-model="form.timeZone"
                  label="Time zone"
                  placeholder="Europe/Berlin"
                  hint="IANA id. Booking slots are calculated in this zone."
                />

                <div>
                  <label
                    for="shop-description"
                    class="mb-1.5 block text-[12px] font-semibold tracking-wider text-muted uppercase"
                  >
                    Description
                  </label>
                  <textarea
                    id="shop-description"
                    v-model="form.description"
                    rows="4"
                    maxlength="500"
                    :aria-invalid="!!descriptionError"
                    class="w-full rounded-field border border-line-strong bg-surface px-3 py-2 text-[14px] text-ink placeholder:text-muted/60 hover:border-muted focus:border-brand focus:ring-2 focus:ring-brand/20"
                    placeholder="A few sentences about the shop."
                    @input="descriptionError = ''"
                  />
                  <p v-if="descriptionError" class="mt-1.5 text-[12px] font-medium text-danger">
                    {{ descriptionError }}
                  </p>
                  <p v-else class="mt-1.5 text-[12px] text-muted">
                    {{ form.description.length }}/500
                  </p>
                </div>

                <div class="flex flex-wrap gap-2">
                  <UiButton type="submit" :disabled="!canSave" :loading="saving">
                    <template #icon>
                      <UiSpinner v-if="saving" class="size-4" />
                      <Save v-else class="size-4" aria-hidden="true" />
                    </template>
                    {{ configured ? 'Save and continue' : 'Create shop and continue' }}
                  </UiButton>
                </div>
              </div>
            </UiCard>
          </form>

          <div class="space-y-4">
            <UiCard title="Opening hours">
              <template #actions>
                <Clock class="size-4 text-muted" aria-hidden="true" />
              </template>
              <dl class="space-y-2 text-[13px]">
                <div class="flex justify-between">
                  <dt class="text-muted">Mon – Sat</dt>
                  <dd class="font-mono text-ink">09:00 – 19:00</dd>
                </div>
                <div class="flex justify-between">
                  <dt class="text-muted">Sunday</dt>
                  <dd class="font-mono text-ink">Closed</dd>
                </div>
              </dl>
              <p class="mt-4 text-[12px] text-muted">
                Set automatically when the shop is created. The owner changes them from the
                shop app.
              </p>
            </UiCard>
          </div>
        </div>
      </template>
    </StepShell>
  </div>
</template>
