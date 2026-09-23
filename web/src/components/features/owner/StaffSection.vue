<script setup lang="ts">
import { ref, useTemplateRef } from 'vue';
import { api, ApiRequestError } from '../../../lib/api';
import type { ApiBarber } from '../../../lib/catalog';
import { useAuthStore } from '../../../stores/auth';
import { useShopStore } from '../../../stores/shop';
import UiInput from '../../ui/UiInput.vue';

type Validator = { validate: () => boolean; setError: (message: string) => void };

const SENIORITIES = [
  { id: '11111111-1111-1111-1111-111111111111', name: 'Junior' },
  { id: '22222222-2222-2222-2222-222222222222', name: 'Barber' },
  { id: '33333333-3333-3333-3333-333333333333', name: 'Senior' },
  { id: '44444444-4444-4444-4444-444444444444', name: 'Master' },
];

const emit = defineEmits<{ (e: 'staff-changed'): void }>();

defineProps<{ staff: ApiBarber[] }>();

const auth = useAuthStore();
const shopStore = useShopStore();

const staffEmail = ref('');
const staffSeniority = ref('22222222-2222-2222-2222-222222222222');
const staffFormError = ref('');
const staffFormLoading = ref(false);

const staffEmailInput = useTemplateRef<Validator>('staffEmailInput');

const staffEmailRules = [
  (v: string) => (!v ? 'EMAIL JE OBAVEZAN' : ''),
  (v: string) => (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v) ? 'UNESI ISPRAVAN EMAIL' : ''),
];

async function addStaffMember(): Promise<void> {
  staffFormError.value = '';
  if (!(staffEmailInput.value?.validate() ?? false)) return;
  staffFormLoading.value = true;
  try {
    const shopId = shopStore.shop?.id;
    if (!shopId) {
      staffFormError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
      return;
    }
    await api('/api/staff', {
      method: 'POST',
      token: auth.accessToken,
      body: { email: staffEmail.value, barbershopId: shopId, seniorityId: staffSeniority.value },
    });
    staffEmail.value = '';
    emit('staff-changed');
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 404) {
      staffFormError.value = 'KORISNIK NE POSTOJI, MORA SE PRVO REGISTROVATI';
      return;
    }
    staffFormError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    staffFormLoading.value = false;
  }
}

async function toggleStaff(b: ApiBarber): Promise<void> {
  staffFormError.value = '';
  try {
    await api(`/api/staff/${b.id}`, {
      method: 'PATCH',
      token: auth.accessToken,
      body: { isActive: !b.isActive },
    });
    emit('staff-changed');
  } catch {
    staffFormError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}
</script>

<template>
  <h2 class="text-3xl tracking-widest text-bone">OSOBLJE</h2>
  <p
    v-if="staffFormError"
    role="alert"
    class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
  >
    {{ staffFormError }}
  </p>
  <ul aria-label="Osoblje" class="mt-4 grid gap-3 xl:grid-cols-2">
    <li
      v-for="b in staff"
      :key="b.id"
      :class="
        b.isActive
          ? 'border border-line bg-surface p-4'
          : 'border border-line bg-surface p-4 opacity-50'
      "
    >
      <div class="flex items-center justify-between gap-3">
        <span class="text-2xl leading-none tracking-widest text-bone">
          {{ b.firstName }} {{ b.lastName }}
        </span>
        <span class="shrink-0 text-lg tracking-widest text-ash">
          {{ b.seniority.toUpperCase() }}
        </span>
      </div>
      <button
        type="button"
        class="mt-3 w-full border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
        @click="toggleStaff(b)"
      >
        {{ b.isActive ? 'DEAKTIVIRAJ' : 'AKTIVIRAJ' }}
      </button>
    </li>
  </ul>

  <h3 class="mt-8 text-2xl tracking-widest text-bone">DODAJ BERBERINA</h3>
  <p class="mt-1 text-lg tracking-widest text-ash">KORISNIK MORA BITI REGISTROVAN</p>
  <div class="mt-4 grid gap-3 sm:grid-cols-2">
    <UiInput
      id="staff-email"
      ref="staffEmailInput"
      v-model="staffEmail"
      label="EMAIL"
      type="email"
      placeholder="BERBERIN@MAIL.COM"
      autocomplete="email"
      :rules="staffEmailRules"
    />
    <div>
      <label for="staff-seniority" class="block text-lg tracking-widest text-ash">NIVO</label>
      <select
        id="staff-seniority"
        v-model="staffSeniority"
        class="mt-1 w-full border border-line bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
      >
        <option v-for="s in SENIORITIES" :key="s.id" :value="s.id">
          {{ s.name.toUpperCase() }}
        </option>
      </select>
      <p aria-hidden="true" class="mt-1 min-h-6 text-lg tracking-widest"> </p>
    </div>
  </div>
  <button
    type="button"
    :disabled="staffFormLoading"
    class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
    @click="addStaffMember"
  >
    {{ staffFormLoading ? 'UČITAVANJE...' : 'DODAJ' }}
  </button>
</template>
