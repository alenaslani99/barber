<script setup lang="ts">
import { computed } from 'vue';
import { cn } from '../../../utils/cn';
import type { ApiBarber } from '../../../lib/catalog';

const props = defineProps<{ barber: ApiBarber; selected: boolean }>();
defineEmits<{ (e: 'select', id: string): void }>();

const initials = computed(
  () => `${props.barber.firstName.charAt(0)}${props.barber.lastName.charAt(0)}`,
);
const fullName = computed(() => `${props.barber.firstName} ${props.barber.lastName}`);
</script>

<template>
  <button
    type="button"
    :aria-pressed="selected"
    :class="
      cn(
        'flex w-full items-center gap-4 border p-4 text-left',
        selected
          ? 'border-bone bg-bone text-ink'
          : 'border-line bg-surface text-bone hover:border-bone',
      )
    "
    @click="$emit('select', barber.id)"
  >
    <span
      :class="
        cn(
          'flex h-12 w-12 shrink-0 items-center justify-center border text-2xl tracking-widest',
          selected ? 'border-ink bg-ink text-bone' : 'border-line bg-ink text-bone',
        )
      "
      aria-hidden="true"
    >
      {{ initials }}
    </span>
    <span>
      <span class="block text-2xl leading-none tracking-widest">{{ fullName }}</span>
      <span :class="cn('block text-lg tracking-widest', selected ? 'text-ink' : 'text-ash')">
        {{ barber.seniority }}
      </span>
    </span>
  </button>
</template>
