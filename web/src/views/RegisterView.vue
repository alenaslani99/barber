<script setup lang="ts">
import { ref, useTemplateRef } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import AuthLayout from '../components/layout/AuthLayout.vue';
import UiInput from '../components/ui/UiInput.vue';

type Validator = { validate: () => boolean };

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^[+\d][\d\s/().-]{5,}$/;

const router = useRouter();
const route = useRoute();

const firstName = ref('');
const lastName = ref('');
const email = ref('');
const phone = ref('');
const password = ref('');
const confirm = ref('');

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

function submit(): void {
  const results = [
    firstNameInput.value?.validate() ?? false,
    lastNameInput.value?.validate() ?? false,
    emailInput.value?.validate() ?? false,
    phoneInput.value?.validate() ?? false,
    passwordInput.value?.validate() ?? false,
    confirmInput.value?.validate() ?? false,
  ];
  if (results.includes(false)) return;
  const next = typeof route.query.next === 'string' ? route.query.next : '/';
  void router.push(next);
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
        <button
          type="submit"
          class="mt-6 w-full bg-blaze py-3 text-2xl tracking-widest text-ink hover:opacity-90"
        >
          REGISTRUJ SE
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
