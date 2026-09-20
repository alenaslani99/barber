<script setup lang="ts">
import { computed } from 'vue';
import { cn } from '../../../utils/cn';
import type { Barber } from '../../../data/mock';

const props = defineProps<{ barber: Barber; selected: boolean }>();
defineEmits<{ (e: 'select', id: string): void }>();

const initials = computed(() =>
  props.barber.name
    .split(' ')
    .map((w) => w.charAt(0))
    .slice(0, 2)
    .join(''),
);
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
    <span class="flex w-full items-center justify-between gap-3">
      <span>
        <span class="block text-2xl leading-none tracking-widest">{{ barber.name }}</span>
        <span :class="cn('block text-lg tracking-widest', selected ? 'text-ink' : 'text-ash')">
          {{ barber.role }}
        </span>
      </span>
      <span
        :class="cn('shrink-0 text-lg tracking-widest', selected ? 'text-ink' : 'text-ash')"
      >
        {{ barber.nextAvailable }}
      </span>
    </span>
  </button>
</template>
