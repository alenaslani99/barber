<script setup lang="ts">
import { KeyRound } from '@lucide/vue';
import UiAlert from '../../ui/UiAlert.vue';
import UiCard from '../../ui/UiCard.vue';
import CredentialField from './CredentialField.vue';
import type { Credentials } from '../../../lib/tenants';

defineProps<{ title: string; credentials: Credentials }>();
</script>

<template>
  <UiCard :title="title" :description="`${credentials.firstName} ${credentials.lastName}`">
    <template #actions>
      <KeyRound class="size-5 text-brand" aria-hidden="true" />
    </template>
    <div class="space-y-4">
      <UiAlert tone="warning" title="Shown only once">
        Copy the password now. It is not stored anywhere and cannot be shown again.
      </UiAlert>
      <CredentialField label="Email" :value="credentials.email" />
      <CredentialField label="Password" :value="credentials.password" secret />
      <slot />
    </div>
  </UiCard>
</template>
