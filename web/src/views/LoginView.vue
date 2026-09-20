<script setup lang="ts">
import { onMounted, ref, useTemplateRef } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import AuthLayout from '../components/layout/AuthLayout.vue';
import UiCheckbox from '../components/ui/UiCheckbox.vue';
import UiInput from '../components/ui/UiInput.vue';
import { ApiRequestError } from '../lib/api';
import { useAuthStore } from '../stores/auth';

type Validator = { validate: () => boolean; setError: (message: string) => void };

const REMEMBER_KEY = 'barber.remember.email';
const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const router = useRouter();
const route = useRoute();
const auth = useAuthStore();

const email = ref('');
const password = ref('');
const remember = ref(false);
const loading = ref(false);
const formError = ref('');

const emailInput = useTemplateRef<Validator>('emailInput');
const passwordInput = useTemplateRef<Validator>('passwordInput');

const emailRules = [
  (v: string) => (!v ? 'EMAIL JE OBAVEZAN' : ''),
  (v: string) => (!EMAIL_RE.test(v) ? 'UNESI ISPRAVAN EMAIL' : ''),
];
const passwordRules = [
  (v: string) => (!v ? 'LOZINKA JE OBAVEZNA' : ''),
  (v: string) => (v.length < 8 ? 'MIN 8 KARAKTERA' : ''),
];

onMounted(() => {
  const saved = localStorage.getItem(REMEMBER_KEY);
  if (saved) {
    email.value = saved;
    remember.value = true;
  }
});

async function submit(): Promise<void> {
  formError.value = '';
  const emailOk = emailInput.value?.validate() ?? false;
  const passOk = passwordInput.value?.validate() ?? false;
  if (!emailOk || !passOk) return;
  loading.value = true;
  try {
    await auth.login(email.value, password.value);
    if (remember.value) localStorage.setItem(REMEMBER_KEY, email.value);
    else localStorage.removeItem(REMEMBER_KEY);
    const next = typeof route.query.next === 'string' ? route.query.next : '/';
    await router.push(next);
  } catch (error) {
    if (error instanceof ApiRequestError && error.status === 401) {
      formError.value = 'POGREŠAN EMAIL ILI LOZINKA';
    } else {
      formError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
    }
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <AuthLayout title="PRIJAVA" description="PRIJAVI SE DA ZAKAŽEŠ TERMIN">
    <template #fields>
      <form novalidate @submit.prevent="submit">
        <UiInput
          id="login-email"
          ref="emailInput"
          v-model="email"
          label="EMAIL"
          type="email"
          placeholder="YOU@MAIL.COM"
          autocomplete="email"
          :rules="emailRules"
        />
        <div class="mt-2">
          <UiInput
            id="login-password"
            ref="passwordInput"
            v-model="password"
            label="LOZINKA"
            type="password"
            placeholder="MIN 8 KARAKTERA"
            autocomplete="current-password"
            :rules="passwordRules"
          />
        </div>
        <div class="mt-4">
          <UiCheckbox v-model="remember" label="ZAPAMTI ME" />
        </div>
        <p
          v-if="formError"
          role="alert"
          class="mt-4 border border-alarm p-3 text-center text-lg tracking-widest text-alarm"
        >
          {{ formError }}
        </p>
        <button
          type="submit"
          :disabled="loading"
          class="mt-6 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90 disabled:opacity-40"
        >
          {{ loading ? 'UČITAVANJE...' : 'PRIJAVI SE' }}
        </button>
      </form>
    </template>
    <template #footer>
      <p class="text-lg tracking-widest text-ash">
        NEMAŠ NALOG?
        <RouterLink to="/register" class="text-bone hover:text-blaze">REGISTRUJ SE</RouterLink>
      </p>
    </template>
  </AuthLayout>
</template>
