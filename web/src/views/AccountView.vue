<script setup lang="ts">
import { computed, onMounted, ref, useTemplateRef } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { api, ApiRequestError } from '../lib/api';
import { formatWhen, statusLabel } from '../lib/bookings';
import { useAuthStore } from '../stores/auth';
import { useShopStore } from '../stores/shop';
import UiInput from '../components/ui/UiInput.vue';

interface Profile {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

interface MyBooking {
  id: string;
  serviceName: string;
  barberName: string;
  startsAt: string;
  endsAt: string;
  status: string;
}

interface BookingsPage {
  items: MyBooking[];
  total: number;
}

type Validator = { validate: () => boolean; setError: (message: string) => void };
type Tab = 'general' | 'history' | 'password';

const PAGE_SIZE = 5;

const auth = useAuthStore();
const router = useRouter();
const shopStore = useShopStore();

const tab = ref<Tab>('general');
const profile = ref<Profile | null>(null);
const bookings = ref<MyBooking[]>([]);
const total = ref(0);
const loading = ref(true);
const loadingMore = ref(false);
const loadError = ref('');
const cancelError = ref('');

const currentPw = ref('');
const newPw = ref('');
const confirmPw = ref('');
const pwLoading = ref(false);
const pwError = ref('');

const currentPwInput = useTemplateRef<Validator>('currentPwInput');
const newPwInput = useTemplateRef<Validator>('newPwInput');
const confirmPwInput = useTemplateRef<Validator>('confirmPwInput');

const canShowMore = computed(() => bookings.value.length < total.value);

async function loadHistory(): Promise<void> {
  loadingMore.value = true;
  try {
    const page = await api<BookingsPage>(
      `/api/booking/mine?skip=${bookings.value.length}&take=${PAGE_SIZE}`,
      { token: auth.accessToken },
    );
    bookings.value = [...bookings.value, ...page.items];
    total.value = page.total;
  } catch {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { next: '/account' } });
      return;
    }
    loadError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    loadingMore.value = false;
  }
}

onMounted(async () => {
  void shopStore.load();
  try {
    const p = await api<Profile>('/api/auth/me', { token: auth.accessToken });
    profile.value = p;
    await loadHistory();
  } catch {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { next: '/account' } });
      return;
    }
    loadError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    loading.value = false;
  }
});

async function changePassword(): Promise<void> {
  pwError.value = '';
    pwError.value = '';
    const currentOk = currentPwInput.value?.validate() ?? false;
  const newOk = newPwInput.value?.validate() ?? false;
  const confirmOk = confirmPwInput.value?.validate() ?? false;
  if (!currentOk || !newOk || !confirmOk) return;
  pwLoading.value = true;
  try {
    await api<void>('/api/auth/change-password', {
      method: 'POST',
      token: auth.accessToken,
      body: { currentPassword: currentPw.value, newPassword: newPw.value },
    });
    await auth.logout();
    await router.push('/login');
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 401) {
      currentPwInput.value?.setError('TRENUTNA LOZINKA NIJE ISPRAVNA');
      return;
    }
    if (error instanceof ApiRequestError && error.status === 400) {
      newPwInput.value?.setError('MIN 8 KARAKTERA');
      return;
    }
    pwError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    pwLoading.value = false;
  }
}

async function logout(): Promise<void> {
  await auth.logout();
  await router.push('/');
}

async function cancelBooking(b: MyBooking): Promise<void> {
  cancelError.value = '';
  try {
    const updated = await api<MyBooking>(`/api/booking/${b.id}/status`, {
      method: 'PATCH',
      token: auth.accessToken,
      body: { status: 'Cancelled' },
    });
    const idx = bookings.value.findIndex((x) => x.id === b.id);
    if (idx >= 0) bookings.value[idx] = updated;
  } catch {
    cancelError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  }
}
</script>

<template>
  <main class="bg-ink text-bone">
    <div class="mx-auto w-full px-4 py-10 lg:w-1/2">
      <p v-if="shopStore.shop" class="text-center text-lg tracking-widest">
        <RouterLink to="/" class="text-ash hover:text-bone" aria-label="Nazad na početnu">
          {{ shopStore.shop.name }}
        </RouterLink>
      </p>
      <h1 class="mt-2 text-center font-display text-6xl leading-none tracking-wide">NALOG</h1>

      <nav aria-label="Sekcije naloga" class="mt-8 grid grid-cols-3 border border-line">
        <button
          type="button"
          :aria-current="tab === 'general' ? 'page' : undefined"
          :class="
            tab === 'general'
              ? 'bg-bone text-ink'
              : 'text-ash hover:text-bone'
          "
          class="border-r border-line py-2 text-xl tracking-widest last:border-r-0"
          @click="tab = 'general'"
        >
          OPŠTE
        </button>
        <button
          type="button"
          :aria-current="tab === 'history' ? 'page' : undefined"
          :class="
            tab === 'history'
              ? 'bg-bone text-ink'
              : 'text-ash hover:text-bone'
          "
          class="border-r border-line py-2 text-xl tracking-widest last:border-r-0"
          @click="tab = 'history'"
        >
          ISTORIJA
        </button>
        <button
          type="button"
          :aria-current="tab === 'password' ? 'page' : undefined"
          :class="
            tab === 'password'
              ? 'bg-bone text-ink'
              : 'text-ash hover:text-bone'
          "
          class="border-r border-line py-2 text-xl tracking-widest last:border-r-0"
          @click="tab = 'password'"
        >
          LOZINKA
        </button>
      </nav>

      <p v-if="loading" class="mt-8 text-xl tracking-widest text-ash">UČITAVANJE...</p>
      <p
        v-else-if="loadError"
        role="alert"
        class="mt-8 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
      >
        {{ loadError }}
      </p>

      <div v-else-if="profile && tab === 'general'">
        <ul aria-label="Podaci o nalogu" class="mt-8 border-y border-line">
          <li class="flex items-center justify-between py-2">
            <span class="text-lg tracking-widest text-ash">IME</span>
            <span class="text-xl tracking-widest text-bone">{{ profile.firstName }}</span>
          </li>
          <li class="flex items-center justify-between border-t border-line py-2">
            <span class="text-lg tracking-widest text-ash">PREZIME</span>
            <span class="text-xl tracking-widest text-bone">{{ profile.lastName }}</span>
          </li>
          <li class="flex items-center justify-between border-t border-line py-2">
            <span class="text-lg tracking-widest text-ash">EMAIL</span>
            <span class="text-xl tracking-widest text-bone">{{ profile.email }}</span>
          </li>
          <li class="flex items-center justify-between border-t border-line py-2">
            <span class="text-lg tracking-widest text-ash">TELEFON</span>
            <span class="text-xl tracking-widest text-bone">{{ profile.phone }}</span>
          </li>
        </ul>
        <button
          type="button"
          class="mt-10 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone"
          @click="logout"
        >
          ODJAVI SE
        </button>
      </div>

      <div v-else-if="profile && tab === 'history'">
        <h2 class="mt-8 text-3xl tracking-widest text-bone">
          MOJE REZERVACIJE
          <span class="text-blaze">({{ total }})</span>
        </h2>
        <p
          v-if="cancelError"
          role="alert"
          class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
        >
          {{ cancelError }}
        </p>
        <p v-if="bookings.length === 0" class="mt-4 text-xl tracking-widest text-ash">
          NEMAŠ REZERVACIJA
        </p>
        <ul v-else aria-label="Rezervacije" class="mt-4 flex flex-col gap-3">
          <li
            v-for="b in bookings"
            :key="b.id"
            class="border border-line bg-surface p-4"
          >
            <div class="flex items-center justify-between gap-3">
              <span class="text-2xl leading-none tracking-widest text-bone">
                {{ b.serviceName }}
              </span>
              <span class="shrink-0 text-lg tracking-widest text-blaze">
                {{ statusLabel(b.status) }}
              </span>
            </div>
            <p class="mt-1 text-lg tracking-widest text-ash">
              {{ b.barberName }} / {{ formatWhen(b.startsAt) }}
            </p>
            <button
              v-if="b.status === 'Pending' || b.status === 'Confirmed'"
              type="button"
              class="mt-3 w-full border border-line py-2 text-xl tracking-widest text-bone hover:border-alarm hover:text-alarm"
              @click="cancelBooking(b)"
            >
              OTKAŽI
            </button>
          </li>
        </ul>
        <button
          v-if="canShowMore"
          type="button"
          :disabled="loadingMore"
          class="mt-4 w-full border border-line py-3 text-2xl tracking-widest text-bone hover:border-bone disabled:opacity-40"
          @click="loadHistory"
        >
          {{ loadingMore ? 'UČITAVANJE...' : 'PRIKAŽI JOŠ' }}
        </button>
      </div>

      <div v-else-if="profile && tab === 'password'">
        <form novalidate class="mt-8" @submit.prevent="changePassword">
          <UiInput
            id="account-current-password"
            ref="currentPwInput"
            v-model="currentPw"
            label="TRENUTNA LOZINKA"
            type="password"
            placeholder="TRENUTNA LOZINKA"
            autocomplete="current-password"
            :rules="[(v: string) => (!v ? 'LOZINKA JE OBAVEZNA' : '')]"
          />
          <div class="mt-2">
            <UiInput
              id="account-new-password"
              ref="newPwInput"
              v-model="newPw"
              label="NOVA LOZINKA"
              type="password"
              placeholder="MIN 8 KARAKTERA"
              autocomplete="new-password"
              :rules="[
                (v: string) => (!v ? 'LOZINKA JE OBAVEZNA' : ''),
                (v: string) => (v.length < 8 ? 'MIN 8 KARAKTERA' : ''),
              ]"
            />
          </div>
          <div class="mt-2">
            <UiInput
              id="account-confirm-password"
              ref="confirmPwInput"
              v-model="confirmPw"
              label="POTVRDI NOVU LOZINKU"
              type="password"
              placeholder="PONOVI NOVU LOZINKU"
              autocomplete="new-password"
              :rules="[
                (v: string) => (!v ? 'POTVRDI LOZINKU' : ''),
                (v: string) => (v !== newPw ? 'LOZINKE SE NE POKLAPAJU' : ''),
              ]"
            />
          </div>
          <p
            v-if="pwError"
            role="alert"
            class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
          >
            {{ pwError }}
          </p>
          <button
            type="submit"
            :disabled="pwLoading"
            class="mt-6 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
          >
            {{ pwLoading ? 'UČITAVANJE...' : 'SAČUVAJ' }}
          </button>
        </form>
      </div>
    </div>
  </main>
</template>
