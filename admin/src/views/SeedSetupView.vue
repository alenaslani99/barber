<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch, type Ref } from 'vue';
import { RouterLink, useRoute } from 'vue-router';
import { UserPlus, Users, X } from '@lucide/vue';
import CredentialsCard from '../components/features/onboarding/CredentialsCard.vue';
import ServicesPanel from '../components/features/onboarding/ServicesPanel.vue';
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
  createBarber,
  getSeniorities,
  getStaff,
  getTenantSetup,
  type Credentials,
  type Seniority,
  type StaffAccount,
} from '../lib/tenants';
import { useTenantStore } from '../stores/tenant';

type Field = 'firstName' | 'lastName' | 'email' | 'phone';
type InputRef = Ref<InstanceType<typeof UiInput> | null>;

// Matches SeniorityConfiguration.BarberId on the backend.
const DEFAULT_SENIORITY_ID = '22222222-2222-2222-2222-222222222222';

const route = useRoute();
const { currentSlug, setSlug } = useTenantStore();

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  seniorityId: DEFAULT_SENIORITY_ID,
});

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
const seniorities = ref<Seniority[]>([]);
const staff = ref<StaffAccount[]>([]);
const serviceCount = ref(0);
const bookable = computed(() => staff.value.length > 0 && serviceCount.value > 0);
// Latest created barber only; lives in this component and is never persisted.
const credentials = ref<Credentials | null>(null);

function syncFromRoute(slug: unknown): void {
  if (typeof slug === 'string' && slug.length > 0) setSlug(slug);
}

onMounted(() => syncFromRoute(route.params.slug));
watch(() => route.params.slug, syncFromRoute);

async function load(slug: string): Promise<void> {
  if (!slug) return;
  loading.value = true;
  failure.value = '';
  credentials.value = null;
  try {
    const setup = await getTenantSetup(slug);
    shopMissing.value = !setup.hasShop;
    [seniorities.value, staff.value] = await Promise.all([getSeniorities(slug), getStaff(slug)]);
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    failure.value = error.status === 404 ? `Tenant "${slug}" does not exist.` : error.title;
  } finally {
    loading.value = false;
  }
}

watch(currentSlug, load, { immediate: true });

function resetForm(): void {
  form.firstName = '';
  form.lastName = '';
  form.email = '';
  form.phone = '';
  form.seniorityId = DEFAULT_SENIORITY_ID;
  for (const input of Object.values(inputs)) input.value?.reset();
}

async function submit(): Promise<void> {
  const results = Object.values(inputs).map((input) => input.value?.validate() ?? false);
  if (results.includes(false)) return;

  saving.value = true;
  failure.value = '';
  try {
    credentials.value = await createBarber(currentSlug.value, {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      seniorityId: form.seniorityId,
    });
    resetForm();
    staff.value = await getStaff(currentSlug.value);
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
</script>

<template>
  <div>
    <UiPageHeader
      title="Staff & Services"
      subtitle="Step 4 — barbers and services so the shop is bookable."
    >
      <template #meta>
        <div class="mt-2">
          <UiBadge v-if="staff.length > 0" tone="success" dot>
            {{ staff.length }} {{ staff.length === 1 ? 'barber' : 'barbers' }}
          </UiBadge>
          <UiBadge v-else tone="warning">No staff</UiBadge>
          <UiBadge v-if="serviceCount > 0" tone="success" dot class="ml-2">
            {{ serviceCount }} {{ serviceCount === 1 ? 'service' : 'services' }}
          </UiBadge>
          <UiBadge v-else tone="warning" class="ml-2">No services</UiBadge>
        </div>
      </template>
    </UiPageHeader>

    <StepShell :step="4">
      <template #default="{ slug }">
        <div v-if="slug" class="grid gap-4 lg:grid-cols-3">
          <div class="space-y-4 lg:col-span-2">
            <UiAlert v-if="shopMissing && !loading" tone="warning" title="Create the shop first">
              Staff belong to the barbershop.
              <RouterLink :to="`/tenants/${slug}/shop`" class="font-semibold text-brand underline">
                Go to step 2
              </RouterLink>
            </UiAlert>

            <CredentialsCard
              v-if="credentials"
              title="Barber account created"
              :credentials="credentials"
            >
              <div class="flex flex-wrap gap-2 pt-1">
                <UiButton variant="secondary" @click="credentials = null">
                  <template #icon><X class="size-4" aria-hidden="true" /></template>
                  Done, hide credentials
                </UiButton>
              </div>
            </CredentialsCard>

            <form novalidate @submit.prevent="submit">
              <UiCard title="Add barber" description="Creates their login. The password is generated for you.">
                <div v-if="loading" class="grid place-items-center py-10 text-muted">
                  <UiSpinner class="size-5" />
                </div>

                <div v-else class="space-y-5">
                  <UiAlert v-if="failure" tone="danger" title="Barber not created">
                    {{ failure }}
                  </UiAlert>

                  <div class="grid gap-5 sm:grid-cols-2">
                    <UiInput
                      id="barber-first-name"
                      ref="firstNameInput"
                      v-model="form.firstName"
                      label="First name"
                      required
                      :disabled="shopMissing"
                      :rules="[(v) => (!v.trim() ? 'First name is required.' : '')]"
                    />
                    <UiInput
                      id="barber-last-name"
                      ref="lastNameInput"
                      v-model="form.lastName"
                      label="Last name"
                      required
                      :disabled="shopMissing"
                      :rules="[(v) => (!v.trim() ? 'Last name is required.' : '')]"
                    />
                  </div>

                  <div class="grid gap-5 sm:grid-cols-2">
                    <UiInput
                      id="barber-email"
                      ref="emailInput"
                      v-model="form.email"
                      type="email"
                      label="Email"
                      placeholder="From the owner"
                      required
                      hint="The barber logs in with this."
                      :disabled="shopMissing"
                      :rules="[
                        (v) => (!v.trim() ? 'Email is required.' : ''),
                        (v) => (v.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.trim()) ? 'Enter a valid email.' : ''),
                      ]"
                    />
                    <UiInput
                      id="barber-phone"
                      ref="phoneInput"
                      v-model="form.phone"
                      type="tel"
                      label="Phone"
                      placeholder="+49 170 1234567"
                      required
                      :disabled="shopMissing"
                      :rules="[(v) => (!v.trim() ? 'Phone is required.' : '')]"
                    />
                  </div>

                  <div>
                    <label
                      for="barber-seniority"
                      class="mb-1.5 block text-[12px] font-semibold tracking-wider text-muted uppercase"
                    >
                      Seniority
                    </label>
                    <select
                      id="barber-seniority"
                      v-model="form.seniorityId"
                      :disabled="shopMissing"
                      class="w-full rounded-field border border-line-strong bg-surface px-3 py-2 text-[14px] text-ink hover:border-muted focus:border-brand focus:ring-2 focus:ring-brand/20 disabled:bg-canvas disabled:text-muted sm:w-1/2"
                    >
                      <option v-for="s in seniorities" :key="s.id" :value="s.id">{{ s.name }}</option>
                    </select>
                  </div>

                  <div class="flex flex-wrap gap-2">
                    <UiButton type="submit" :disabled="shopMissing" :loading="saving">
                      <template #icon>
                        <UiSpinner v-if="saving" class="size-4" />
                        <UserPlus v-else class="size-4" aria-hidden="true" />
                      </template>
                      Add barber
                    </UiButton>
                  </div>
                </div>
              </UiCard>
            </form>

            <UiCard title="Barbers" :description="`${staff.length} on ${slug}`">
              <template #actions>
                <Users class="size-4 text-muted" aria-hidden="true" />
              </template>
              <p v-if="staff.length === 0" class="py-6 text-center text-[13px] text-muted">
                No barbers yet. Add the first one above.
              </p>
              <div v-else class="-mx-5 -my-5 overflow-x-auto">
                <table class="w-full text-left text-[13px]">
                  <thead class="border-b border-line text-[11px] tracking-wider text-muted uppercase">
                    <tr>
                      <th class="px-5 py-2.5 font-semibold">Name</th>
                      <th class="px-5 py-2.5 font-semibold">Email</th>
                      <th class="px-5 py-2.5 font-semibold">Phone</th>
                      <th class="px-5 py-2.5 font-semibold">Seniority</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="member in staff" :key="member.id" class="border-b border-line last:border-0">
                      <td class="px-5 py-3 font-medium text-ink">
                        {{ member.firstName }} {{ member.lastName }}
                        <UiBadge v-if="!member.isActive" tone="neutral" class="ml-2">Inactive</UiBadge>
                      </td>
                      <td class="px-5 py-3 font-mono text-ink">{{ member.email }}</td>
                      <td class="px-5 py-3 font-mono text-muted">{{ member.phone }}</td>
                      <td class="px-5 py-3"><UiBadge tone="brand">{{ member.seniority }}</UiBadge></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </UiCard>

            <ServicesPanel :slug="slug" :disabled="shopMissing" @count="serviceCount = $event" />
          </div>

          <div class="space-y-4">
            <UiAlert v-if="bookable" tone="success" title="Shop is bookable">
              At least one barber and one service are set up.
              <RouterLink to="/tenants" class="font-semibold text-brand underline">
                Back to all clients
              </RouterLink>
            </UiAlert>
            <UiAlert v-else-if="!loading" tone="warning" title="Not bookable yet">
              Clients can book once there is at least one barber and one service.
            </UiAlert>
            <UiAlert tone="info" title="Passwords">
              Each barber gets a generated password, shown once. Hand it over together with
              the email the owner gave you.
            </UiAlert>
          </div>
        </div>
      </template>
    </StepShell>
  </div>
</template>
