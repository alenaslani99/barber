<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { api } from '../../../lib/api';
import { useAuthStore } from '../../../stores/auth';
import { useShopStore } from '../../../stores/shop';
import UiCheckbox from '../../ui/UiCheckbox.vue';

interface ShopHoursRow {
  day: number;
  open: string;
  close: string;
  closed: boolean;
}

const WEEKDAYS_FULL = [
  'NEDELJA',
  'PONEDELJAK',
  'UTORAK',
  'SREDA',
  'ČETVRTAK',
  'PETAK',
  'SUBOTA',
];

const auth = useAuthStore();
const shopStore = useShopStore();

const hoursRows = ref<ShopHoursRow[]>([]);
const hoursError = ref('');
const hoursSuccess = ref('');
const hoursLoading = ref(false);

async function loadHours(): Promise<void> {
  const shopId = shopStore.shop?.id;
  if (!shopId) return;
  try {
    const rows = await api<Array<{ day: number; open: string; close: string; isClosed: boolean }>>(
      `/api/workinghours?barbershopId=${shopId}`,
      { token: auth.accessToken },
    );
    hoursRows.value = [1, 2, 3, 4, 5, 6, 0].map((day) => {
      const found = rows.find((r) => r.day === day);
      if (found) return { day, open: found.open, close: found.close, closed: found.isClosed };
      return { day, open: '09:00', close: '20:00', closed: day === 0 };
    });
  } catch {
    hoursError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

function toggleClosed(r: ShopHoursRow, closed: boolean): void {
  r.closed = closed;
  if (!closed && !(r.open < r.close)) {
    r.open = '09:00';
    r.close = '20:00';
  }
}

async function saveHours(): Promise<void> {
  hoursError.value = '';
  hoursSuccess.value = '';
  const shopId = shopStore.shop?.id;
  if (!shopId) {
    hoursError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
    return;
  }
  hoursLoading.value = true;
  try {
    await api('/api/workinghours', {
      method: 'PUT',
      token: auth.accessToken,
      body: {
        barbershopId: shopId,
        hours: hoursRows.value.map((r) => ({
          day: r.day,
          open: r.closed ? '00:00' : r.open,
          close: r.closed ? '00:00' : r.close,
          isClosed: r.closed,
        })),
      },
    });
    hoursSuccess.value = 'SAČUVANO';
  } catch {
    hoursError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    hoursLoading.value = false;
  }
}

onMounted(() => {
  void loadHours();
});
</script>

<template>
  <h2 class="text-3xl tracking-widest text-bone">RADNO VREME</h2>
  <p
    v-if="hoursError"
    role="alert"
    class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
  >
    {{ hoursError }}
  </p>
  <p
    v-if="hoursSuccess"
    role="status"
    class="mt-4 border border-bone p-3 text-center text-lg tracking-widest text-bone"
  >
    {{ hoursSuccess }}
  </p>
  <ul aria-label="Radno vreme po danima" class="mt-4 flex flex-col gap-3">
    <li
      v-for="r in hoursRows"
      :key="r.day"
      class="grid items-center gap-3 border border-line bg-surface p-4 sm:grid-cols-[1fr_auto_auto_auto]"
    >
      <span class="text-2xl leading-none tracking-widest text-bone">
        {{ WEEKDAYS_FULL[r.day] }}
      </span>
      <UiCheckbox
        :model-value="r.closed"
        label="ZATVORENO"
        @update:model-value="(v: boolean) => toggleClosed(r, v)"
      />
      <label class="flex items-center gap-2 text-lg tracking-widest text-ash">
        OD
        <input
          v-model="r.open"
          type="time"
          :disabled="r.closed"
          class="border border-line bg-ink px-2 py-1 font-form text-xl text-bone outline-none disabled:opacity-40"
        />
      </label>
      <label class="flex items-center gap-2 text-lg tracking-widest text-ash">
        DO
        <input
          v-model="r.close"
          type="time"
          :disabled="r.closed"
          class="border border-line bg-ink px-2 py-1 font-form text-xl text-bone outline-none disabled:opacity-40"
        />
      </label>
    </li>
  </ul>
  <button
    type="button"
    :disabled="hoursLoading"
    class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
    @click="saveHours"
  >
    {{ hoursLoading ? 'UČITAVANJE...' : 'SAČUVAJ' }}
  </button>
</template>
