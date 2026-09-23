<script setup lang="ts">
import { ref, useTemplateRef } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import AuthLayout from '../components/layout/AuthLayout.vue';
import UiInput from '../components/ui/UiInput.vue';
import { ApiRequestError } from '../lib/api';
import { useAuthStore } from '../stores/auth';

type Validator = { validate: () => boolean; setError: (message: string) => void };

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^[+\d][\d\s/().-]{5,}$/;

const router = useRouter();
const route = useRoute();
const auth = useAuthStore();

const firstName = ref('');
const lastName = ref('');
const email = ref('');
const phone = ref('');
const password = ref('');
const confirm = ref('');
const loading = ref(false);
const formError = ref('');

const firstNameInput = useTemplateRef<Validator>('firstNameInput');
const lastNameInput = useTemplateRef<Validator>('lastNameInput');
const emailInput = useTemplateRef<Validator>('emailInput');
const phoneInput = useTemplateRef<Validator>('phoneInput');
const passwordInput = useTemplateRef<Validator>('passwordInput');
const confirmInput = useTemplateRef<Validator>('confirmInput');

const firstNameRules = [(v: string) => (!v ? 'IME JE OBAVEZNO' : '')];
const lastNameRules = [(v: string) => (!v ? 'PREZIME JE OBAVEZNO' : '')];
const emailRules = [
  (v: string) => (!v ? 'EMAIL JE OBAVEZAN' : ''),
  (v: string) => (!EMAIL_RE.test(v) ? 'UNESI ISPRAVAN EMAIL' : ''),
];
const phoneRules = [
  (v: string) => (!v ? 'TELEFON JE OBAVEZAN' : ''),
  (v: string) => (!PHONE_RE.test(v) ? 'UNESI ISPRAVAN TELEFON' : ''),
];
const passwordRules = [
  (v: string) => (!v ? 'LOZINKA JE OBAVEZNA' : ''),
  (v: string) => (v.length < 8 ? 'MIN 8 KARAKTERA' : ''),
];
const confirmRules = [
  (v: string) => (!v ? 'POTVRDI LOZINKU' : ''),
  (v: string) => (v !== password.value ? 'LOZINKE SE NE POKLAPAJU' : ''),
];

function applyFieldErrors(errors: Record<string, string[]>): void {
  const leftovers: string[] = [];
  for (const key of Object.keys(errors)) {
    const message = errors[key]?.[0] ?? 'NEISPRAVAN UNOS';
    if (key === 'Email') emailInput.value?.setError(message);
    else if (key === 'Phone') phoneInput.value?.setError(message);
    else if (key === 'Password') passwordInput.value?.setError(message);
    else if (key === 'FirstName') firstNameInput.value?.setError(message);
    else if (key === 'LastName') lastNameInput.value?.setError(message);
    else leftovers.push(message);
  }
  if (leftovers.length > 0) formError.value = leftovers.join(' / ');
}

async function submit(): Promise<void> {
  formError.value = '';
  const results = [
    firstNameInput.value?.validate() ?? false,
    lastNameInput.value?.validate() ?? false,
    emailInput.value?.validate() ?? false,
    phoneInput.value?.validate() ?? false,
    passwordInput.value?.validate() ?? false,
    confirmInput.value?.validate() ?? false,
  ];
  if (results.includes(false)) return;
  loading.value = true;
  try {
    await auth.register({
      firstName: firstName.value,
      lastName: lastName.value,
      email: email.value,
      phone: phone.value,
      password: password.value,
    });
    const next = typeof route.query.next === 'string' ? route.query.next : '/';
    await router.push(next);
  } catch (error) {
    if (error instanceof ApiRequestError) {
      if (error.status === 409) {
        const title = error.title.toLowerCase();
        if (title.includes('email')) emailInput.value?.setError('EMAIL JE ZAUZET');
        else if (title.includes('phone')) phoneInput.value?.setError('TELEFON JE ZAUZET');
        else formError.value = 'NALOG VEĆ POSTOJI';
        return;
      }
      if (error.status === 400) {
        applyFieldErrors(error.errors);
        return;
      }
      if (error.status === 429) {
        formError.value = 'PREVIŠE POKUŠAJA, POKUŠAJ KASNIJE';
        return;
      }
    }
    formError.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <AuthLayout title="REGISTRACIJA" description="NAPRAVI NALOG DA ZAKAŽEŠ TERMIN">
    <template #fields>
      <form novalidate @submit.prevent="submit">
        <div class="grid grid-cols-2 gap-3">
          <UiInput
            id="register-first-name"
            ref="firstNameInput"
            v-model="firstName"
            label="IME"
            type="text"
            placeholder="NIKOLA"
            autocomplete="given-name"
            :rules="firstNameRules"
          />
          <UiInput
            id="register-last-name"
            ref="lastNameInput"
            v-model="lastName"
            label="PREZIME"
            type="text"
            placeholder="PETROVIĆ"
            autocomplete="family-name"
            :rules="lastNameRules"
          />
        </div>
        <div class="mt-2">
          <UiInput
            id="register-email"
            ref="emailInput"
            v-model="email"
            label="EMAIL"
            type="email"
            placeholder="YOU@MAIL.COM"
            autocomplete="email"
            :rules="emailRules"
          />
        </div>
        <div class="mt-2">
          <UiInput
            id="register-phone"
            ref="phoneInput"
            v-model="phone"
            label="TELEFON"
            type="tel"
            placeholder="+381 60 000 00 00"
            autocomplete="tel"
            :rules="phoneRules"
          />
        </div>
        <div class="mt-2">
          <UiInput
            id="register-password"
            ref="passwordInput"
            v-model="password"
            label="LOZINKA"
            type="password"
            placeholder="MIN 8 KARAKTERA"
            autocomplete="new-password"
            :rules="passwordRules"
          />
        </div>
        <div class="mt-2">
          <UiInput
            id="register-confirm"
            ref="confirmInput"
            v-model="confirm"
            label="POTVRDI LOZINKU"
            type="password"
            placeholder="PONOVI LOZINKU"
            autocomplete="new-password"
            :rules="confirmRules"
          />
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
          {{ loading ? 'UČITAVANJE...' : 'REGISTRUJ SE' }}
        </button>
      </form>
    </template>
    <template #footer>
      <p class="text-lg tracking-widest text-ash">
        IMAŠ NALOG?
        <RouterLink to="/login" class="text-bone hover:text-blaze">PRIJAVI SE</RouterLink>
      </p>
    </template>
  </AuthLayout>
</template>
