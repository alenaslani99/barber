<script setup lang="ts">
import { onMounted, ref, useTemplateRef } from 'vue';
import { api } from '../../../lib/api';
import { useAuthStore } from '../../../stores/auth';
import { useShopStore } from '../../../stores/shop';
import UiCheckbox from '../../ui/UiCheckbox.vue';
import UiInput from '../../ui/UiInput.vue';

interface ManagedService {
  id: string;
  name: string;
  price: number;
  durationMinutes: number;
  slotMinutes: number;
  isActive: boolean;
}

type Validator = { validate: () => boolean; setError: (message: string) => void };

const DURATIONS = [10, 15, 20, 30, 40, 45, 55, 60, 90];
const SLOT_STEPS = [5, 10, 15, 20, 30, 60];

const auth = useAuthStore();
const shopStore = useShopStore();

const shopServices = ref<ManagedService[]>([]);
const svcError = ref('');
const svcLoading = ref(false);
const svcEditingId = ref<string | null>(null);
const svcName = ref('');
const svcPrice = ref('');
const svcDuration = ref('30');
const svcSlot = ref('30');
const svcActive = ref(true);

const svcNameInput = useTemplateRef<Validator>('svcNameInput');
const svcPriceInput = useTemplateRef<Validator>('svcPriceInput');

const svcNameRules = [(v: string) => (!v ? 'NAZIV JE OBAVEZAN' : '')];
const svcPriceRules = [
  (v: string) => (!v ? 'CENA JE OBAVEZNA' : ''),
  (v: string) => (!/^\d+(\.\d{1,2})?$/.test(v) ? 'UNESI ISPRAVAN IZNOS' : ''),
];

async function loadServices(): Promise<void> {
  try {
    const list = await api<ManagedService[]>('/api/service', { auth: false });
    shopServices.value = list;
  } catch {
    svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

function resetServiceForm(): void {
  svcEditingId.value = null;
  svcName.value = '';
  svcPrice.value = '';
  svcDuration.value = '30';
  svcSlot.value = '30';
  svcActive.value = true;
}

async function saveService(): Promise<void> {
  svcError.value = '';
  const nameOk = svcNameInput.value?.validate() ?? false;
  const priceOk = svcPriceInput.value?.validate() ?? false;
  if (!nameOk || !priceOk) return;
  svcLoading.value = true;
  try {
    const body = {
      name: svcName.value,
      price: Number(svcPrice.value),
      durationMinutes: Number(svcDuration.value),
      slotMinutes: Number(svcSlot.value),
      isActive: svcActive.value,
    };
    if (svcEditingId.value) {
      await api(`/api/service/${svcEditingId.value}`, {
        method: 'PATCH',
        token: auth.accessToken,
        body,
      });
    } else {
      const shopId = shopStore.shop?.id;
      if (!shopId) {
        svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
        return;
      }
      await api('/api/service', {
        method: 'POST',
        token: auth.accessToken,
        body: { barbershopId: shopId, ...body },
      });
    }
    resetServiceForm();
    await loadServices();
  } catch {
    svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    svcLoading.value = false;
  }
}

function editService(s: ManagedService): void {
  svcError.value = '';
  svcEditingId.value = s.id;
  svcName.value = s.name;
  svcPrice.value = String(s.price);
  svcDuration.value = String(s.durationMinutes);
  svcSlot.value = String(s.slotMinutes);
  svcActive.value = s.isActive;
}

async function toggleService(s: ManagedService): Promise<void> {
  svcError.value = '';
  try {
    await api(`/api/service/${s.id}`, {
      method: 'PATCH',
      token: auth.accessToken,
      body: { isActive: !s.isActive },
    });
    await loadServices();
  } catch {
    svcError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

onMounted(() => {
  void loadServices();
});
</script>

<template>
  <h2 class="text-3xl tracking-widest text-bone">USLUGE</h2>
  <p
    v-if="svcError"
    role="alert"
    class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
  >
    {{ svcError }}
  </p>
  <ul aria-label="Usluge" class="mt-4 grid gap-3 xl:grid-cols-2">
    <li
      v-for="s in shopServices"
      :key="s.id"
      :class="
        s.isActive
          ? 'border border-line bg-surface p-4'
          : 'border border-line bg-surface p-4 opacity-50'
      "
    >
      <div class="flex items-center justify-between gap-3">
        <span class="text-2xl leading-none tracking-widest text-bone">
          {{ s.name }}
        </span>
        <span class="shrink-0 text-2xl tracking-widest text-bone">{{ s.price }}</span>
      </div>
      <p class="mt-1 text-lg tracking-widest text-ash">
        {{ s.durationMinutes }} MIN / KORAK {{ s.slotMinutes }} MIN
      </p>
      <div class="mt-3 flex gap-2">
        <button
          type="button"
          class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
          @click="editService(s)"
        >
          UREDI
        </button>
        <button
          type="button"
          class="flex-1 border border-line py-2 text-xl tracking-widest text-bone hover:border-bone"
          @click="toggleService(s)"
        >
          {{ s.isActive ? 'DEAKTIVIRAJ' : 'AKTIVIRAJ' }}
        </button>
      </div>
    </li>
  </ul>

  <h3 class="mt-8 text-2xl tracking-widest text-bone">
    {{ svcEditingId ? 'IZMENI USLUGU' : 'NOVA USLUGA' }}
  </h3>
  <div class="mt-4 grid gap-3 sm:grid-cols-2">
    <UiInput
      id="svc-name"
      ref="svcNameInput"
      v-model="svcName"
      label="NAZIV"
      type="text"
      placeholder="KLASIČNO ŠIŠANJE"
      :rules="svcNameRules"
    />
    <UiInput
      id="svc-price"
      ref="svcPriceInput"
      v-model="svcPrice"
      label="CENA (RSD)"
      type="text"
      placeholder="2500"
      autocomplete="off"
      :rules="svcPriceRules"
    />
    <div>
      <label for="svc-duration" class="block text-lg tracking-widest text-ash">
        TRAJANJE (MIN)
      </label>
      <select
        id="svc-duration"
        v-model="svcDuration"
        class="mt-1 w-full border border-line bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
      >
        <option v-for="d in DURATIONS" :key="d" :value="String(d)">{{ d }} MIN</option>
      </select>
      <p aria-hidden="true" class="mt-1 min-h-6 text-lg tracking-widest"> </p>
    </div>
    <div>
      <label for="svc-slot" class="block text-lg tracking-widest text-ash">
        KORAK (MIN)
      </label>
      <select
        id="svc-slot"
        v-model="svcSlot"
        class="mt-1 w-full border border-line bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
      >
        <option v-for="m in SLOT_STEPS" :key="m" :value="String(m)">{{ m }} MIN</option>
      </select>
      <p aria-hidden="true" class="mt-1 min-h-6 text-lg tracking-widest"> </p>
    </div>
  </div>
  <div class="mt-2">
    <UiCheckbox v-model="svcActive" label="AKTIVNA" />
  </div>
  <div class="mt-4 flex gap-2">
    <button
      type="button"
      :disabled="svcLoading"
      class="flex-1 bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
      @click="saveService"
    >
      {{ svcLoading ? 'UČITAVANJE...' : svcEditingId ? 'SAČUVAJ' : 'DODAJ' }}
    </button>
    <button
      v-if="svcEditingId"
      type="button"
      class="flex-1 border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
      @click="resetServiceForm"
    >
      OTKAŽI
    </button>
  </div>
</template>
