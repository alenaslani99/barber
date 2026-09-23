<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { api, ApiRequestError } from '../../../lib/api';
import { todayIsoDay } from '../../../lib/dates';
import type { ApiBarber } from '../../../lib/catalog';
import { useAuthStore } from '../../../stores/auth';
import UiInput from '../../ui/UiInput.vue';

interface DayOff {
  id: string;
  staffId: string | null;
  staffName: string;
  date: string;
  reason: string | null;
}

defineProps<{ staff: ApiBarber[] }>();

const auth = useAuthStore();
const router = useRouter();

const dayOffs = ref<DayOff[]>([]);
const doStaff = ref('');
const doDate = ref(todayIsoDay());
const doReason = ref('');
const doError = ref('');
const doStaffError = ref('');
const doDateError = ref('');
const doLoading = ref(false);

const todayIso = todayIsoDay();

async function loadDaysOff(): Promise<void> {
  try {
    dayOffs.value = await api<DayOff[]>(`/api/daysoff?from=${todayIso}`, {
      token: auth.accessToken,
    });
  } catch {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { next: '/owner' } });
      return;
    }
    doError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

async function addDayOff(): Promise<void> {
  doError.value = '';
  doStaffError.value = doStaff.value ? '' : 'IZABERI BERBERINA';
  if (!doDate.value) doDateError.value = 'IZABERI DATUM';
  else if (doDate.value < todayIso) doDateError.value = 'DATUM MORA BITI DANAS ILI KASNIJE';
  else doDateError.value = '';
  if (doStaffError.value || doDateError.value) return;
  doLoading.value = true;
  try {
    await api<unknown>('/api/daysoff', {
      method: 'POST',
      token: auth.accessToken,
      body: {
        staffId: doStaff.value,
        date: doDate.value,
        reason: doReason.value || undefined,
      },
    });
    doReason.value = '';
    await loadDaysOff();
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 409) {
      doError.value = 'DAN JE VEĆ ZAUZET';
      return;
    }
    doError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    doLoading.value = false;
  }
}

async function removeDayOff(id: string): Promise<void> {
  doError.value = '';
  try {
    await api<void>(`/api/daysoff/${id}`, { method: 'DELETE', token: auth.accessToken });
    await loadDaysOff();
  } catch {
    doError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}

onMounted(() => {
  void loadDaysOff();
});
</script>

<template>
  <h2 class="text-3xl tracking-widest text-bone">NERADNI DANI</h2>
  <div class="mt-4 grid gap-3 sm:grid-cols-3">
    <div>
      <label for="dayoff-staff" class="block text-lg tracking-widest text-ash">
        BERBERIN
      </label>
      <select
        id="dayoff-staff"
        v-model="doStaff"
        :aria-invalid="!!doStaffError"
        aria-describedby="dayoff-staff-error"
        :class="doStaffError ? 'border-alarm' : 'border-line'"
        class="mt-1 w-full bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
        @change="doStaffError = ''"
      >
        <option value="" disabled>IZABERI BERBERINA</option>
        <option v-for="b in staff.filter((x) => x.isActive)" :key="b.id" :value="b.id">
          {{ b.firstName }} {{ b.lastName }}
        </option>
      </select>
      <p
        id="dayoff-staff-error"
        aria-live="polite"
        class="mt-1 min-h-6 text-lg tracking-widest text-alarm"
      >
        {{ doStaffError }}
      </p>
    </div>
    <div>
      <label for="dayoff-date" class="block text-lg tracking-widest text-ash">
        DATUM
      </label>
      <input
        id="dayoff-date"
        v-model="doDate"
        type="date"
        :min="todayIso"
        :aria-invalid="!!doDateError"
        aria-describedby="dayoff-date-error"
        :class="doDateError ? 'border-alarm' : 'border-line'"
        class="mt-1 w-full bg-ink px-3 py-2 font-form text-xl text-bone outline-none"
        @change="doDateError = ''"
      />
      <p
        id="dayoff-date-error"
        aria-live="polite"
        class="mt-1 min-h-6 text-lg tracking-widest text-alarm"
      >
        {{ doDateError }}
      </p>
    </div>
    <div>
      <UiInput
        id="dayoff-reason"
        v-model="doReason"
        label="RAZLOG (OPCIONO)"
        type="text"
        placeholder="BOLOVANJE"
      />
    </div>
  </div>
  <p
    v-if="doError"
    role="alert"
    class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
  >
    {{ doError }}
  </p>
  <button
    type="button"
    :disabled="doLoading"
    class="mt-4 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
    @click="addDayOff"
  >
    {{ doLoading ? 'UČITAVANJE...' : 'DODAJ NERADAN DAN' }}
  </button>
  <ul v-if="dayOffs.length > 0" aria-label="Neradni dani" class="mt-4 grid gap-2 xl:grid-cols-2">
    <li
      v-for="d in dayOffs"
      :key="d.id"
      class="flex items-center justify-between gap-3 border border-line bg-surface px-3 py-2"
    >
      <span class="text-lg tracking-widest text-bone">
        {{ d.date }} / {{ d.staffName }}{{ d.reason ? ` / ${d.reason}` : '' }}
      </span>
      <button
        type="button"
        class="shrink-0 border border-line px-3 py-1 text-lg tracking-widest text-ash hover:border-alarm hover:text-alarm"
        @click="removeDayOff(d.id)"
      >
        UKLONI
      </button>
    </li>
  </ul>
</template>
