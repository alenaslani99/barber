<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowRight, UserPlus } from '@lucide/vue';
import CredentialsCard from '../components/features/onboarding/CredentialsCard.vue';
import StepShell from '../components/features/onboarding/StepShell.vue';
import UiAlert from '../components/ui/UiAlert.vue';
import UiBadge from '../components/ui/UiBadge.vue';
import UiButton from '../components/ui/UiButton.vue';
import UiCard from '../components/ui/UiCard.vue';
import UiInput from '../components/ui/UiInput.vue';
import UiPageHeader from '../components/ui/UiPageHeader.vue';
import UiSpinner from '../components/ui/UiSpinner.vue';
import { ApiError } from '../lib/api';
import {
  createOwner,
  getOwner,
  getTenantSetup,
  type Credentials,
  type Owner,
} from '../lib/tenants';
import { useTenantStore } from '../stores/tenant';

type Field = 'firstName' | 'lastName' | 'email' | 'phone';
type InputRef = Ref<InstanceType<typeof UiInput> | null>;

const route = useRoute();
const router = useRouter();
const { currentSlug, setSlug } = useTenantStore();

const form = reactive({ firstName: '', lastName: '', email: '', phone: '' });

const firstNameInput: InputRef = ref(null);
const lastNameInput: InputRef = ref(null);
const emailInput: InputRef = ref(null);
const phoneInput: InputRef = ref(null);
// Keys match the API's validation error keys (PascalCase on the wire).
const inputs: Record<Field, InputRef> = {
  firstName: firstNameInput,
  lastName: lastNameInput,
  email: emailInput,
  phone: phoneInput,
};

const loading = ref(false);
const saving = ref(false);
const failure = ref('');
const shopMissing = ref(false);
const existing = ref<Owner | null>(null);
// Lives only in this component: gone as soon as the operator navigates away.
const credentials = ref<Credentials | null>(null);

const hasOwner = computed(() => existing.value !== null || credentials.value !== null);

function syncFromRoute(slug: unknown): void {
  if (typeof slug === 'string' && slug.length > 0) setSlug(slug);
}

onMounted(() => syncFromRoute(route.params.slug));
watch(() => route.params.slug, syncFromRoute);

async function load(slug: string): Promise<void> {
  if (!slug) return;
  loading.value = true;
  failure.value = '';
  existing.value = null;
  credentials.value = null;
  try {
    const setup = await getTenantSetup(slug);
    shopMissing.value = !setup.hasShop;
    if (setup.hasOwner) existing.value = await getOwner(slug);
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    failure.value = error.status === 404 ? `Tenant "${slug}" does not exist.` : error.title;
  } finally {
    loading.value = false;
  }
}

watch(currentSlug, load, { immediate: true });

async function submit(): Promise<void> {
  const results = (Object.values(inputs) as InputRef[]).map((input) => input.value?.validate() ?? false);
  if (results.includes(false)) return;

  saving.value = true;
  failure.value = '';
  try {
    credentials.value = await createOwner(currentSlug.value, {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
    });
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    // 409s for a taken email/phone belong on that field.
    if (error.status === 409 && /email/i.test(error.title)) {
      emailInput.value?.setError(error.title);
      return;
    }
    if (error.status === 409 && /phone/i.test(error.title)) {
      phoneInput.value?.setError(error.title);
      return;
    }
    let placed = false;
    for (const field of Object.keys(inputs) as Field[]) {
      const key = field.charAt(0).toUpperCase() + field.slice(1);
      const message = error.errors[key]?.[0];
      if (message) {
        inputs[field].value?.setError(message);
        placed = true;
      }
    }
    if (!placed) failure.value = error.title;
  } finally {
    saving.value = false;
  }
}

function continueToStaff(): void {
  router.push(`/tenants/${currentSlug.value}/seed`);
}
</script>

<template>
  <div>
    <UiPageHeader title="Owner Account" subtitle="Step 3 — create the shop owner login.">
      <template #meta>
        <div class="mt-2">
          <UiBadge v-if="hasOwner" tone="success" dot>Configured</UiBadge>
          <UiBadge v-else tone="warning">Not configured</UiBadge>
        </div>
      </template>
    </UiPageHeader>

    <StepShell :step="3">
      <template #default="{ slug }">
        <div v-if="slug" class="grid gap-4 lg:grid-cols-3">
          <div class="space-y-4 lg:col-span-2">
            <UiCard v-if="loading">
              <div class="grid place-items-center py-10 text-muted">
                <UiSpinner class="size-5" />
              </div>
            </UiCard>

            <template v-else-if="credentials">
              <CredentialsCard title="Owner account created" :credentials="credentials">
                <div class="flex flex-wrap gap-2 pt-1">
                  <UiButton @click="continueToStaff">
                    <template #icon><ArrowRight class="size-4" aria-hidden="true" /></template>
                    Continue to staff
                  </UiButton>
                </div>
              </CredentialsCard>
            </template>

            <UiCard
              v-else-if="existing"
              title="Owner already set"
              :description="`${existing.firstName} ${existing.lastName}`"
            >
              <dl class="space-y-2 text-[13px]">
                <div class="flex justify-between gap-4">
                  <dt class="text-muted">Email</dt>
                  <dd class="font-mono text-ink">{{ existing.email }}</dd>
                </div>
                <div class="flex justify-between gap-4">
                  <dt class="text-muted">Phone</dt>
                  <dd class="font-mono text-ink">{{ existing.phone }}</dd>
                </div>
              </dl>
              <p class="mt-4 text-[12px] text-muted">
                The password was shown once when the account was created.
              </p>
              <div class="mt-4">
                <UiButton @click="continueToStaff">
                  <template #icon><ArrowRight class="size-4" aria-hidden="true" /></template>
                  Continue to staff
                </UiButton>
              </div>
            </UiCard>

            <form v-else novalidate @submit.prevent="submit">
              <UiCard title="Owner details" description="The password is generated for you.">
                <div class="space-y-5">
                  <UiAlert v-if="failure" tone="danger" title="Owner not created">
                    {{ failure }}
                  </UiAlert>

                  <div class="grid gap-5 sm:grid-cols-2">
                    <UiInput
                      id="owner-first-name"
                      ref="firstNameInput"
                      v-model="form.firstName"
                      label="First name"
                      required
                      :rules="[(v) => (!v.trim() ? 'First name is required.' : '')]"
                    />
                    <UiInput
                      id="owner-last-name"
                      ref="lastNameInput"
                      v-model="form.lastName"
                      label="Last name"
                      required
                      :rules="[(v) => (!v.trim() ? 'Last name is required.' : '')]"
                    />
                  </div>
                  <UiInput
                    id="owner-email"
                    ref="emailInput"
                    v-model="form.email"
                    type="email"
                    label="Email"
                    placeholder="owner@shop.de"
                    required
                    hint="The owner logs in with this."
                    :rules="[
                      (v) => (!v.trim() ? 'Email is required.' : ''),
                      (v) => (v.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.trim()) ? 'Enter a valid email.' : ''),
                    ]"
                  />
                  <UiInput
                    id="owner-phone"
                    ref="phoneInput"
                    v-model="form.phone"
                    type="tel"
                    label="Phone"
                    placeholder="+49 170 1234567"
                    required
                    :rules="[(v) => (!v.trim() ? 'Phone is required.' : '')]"
                  />

                  <div class="flex flex-wrap gap-2">
                    <UiButton type="submit" :loading="saving">
                      <template #icon>
                        <UiSpinner v-if="saving" class="size-4" />
                        <UserPlus v-else class="size-4" aria-hidden="true" />
                      </template>
                      Create owner
                    </UiButton>
                  </div>
                </div>
              </UiCard>
            </form>
          </div>

          <div class="space-y-4">
            <UiAlert v-if="shopMissing && !loading" tone="warning" title="No shop yet">
              Step 2 is not done. The owner can still be created, but the shop page stays
              empty until the shop exists.
            </UiAlert>
            <UiAlert tone="info" title="How the password works">
              A strong password is generated on the server and shown here once. Use the eye
              icon to reveal it, or copy it without looking.
            </UiAlert>
          </div>
        </div>
      </template>
    </StepShell>
  </div>
</template>
