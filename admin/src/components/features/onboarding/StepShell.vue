<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowRight, Search } from '@lucide/vue';
import UiButton from '../../ui/UiButton.vue';
import UiInput from '../../ui/UiInput.vue';
import { useTenantStore } from '../../../stores/tenant';

const props = defineProps<{
  step: number;
  /** Where to go once this step's form is submitted. */
  nextTo?: string;
}>();

const emit = defineEmits<{ submitted: [] }>();

const router = useRouter();
const { currentSlug, setSlug } = useTenantStore();

const slug = ref<string>(currentSlug.value);
const error = ref('');

const SLUG_RULE = /^[a-z0-9][a-z0-9-]{1,62}$/;

const steps = computed(() => [
  { n: 1, label: 'Tenant' },
  { n: 2, label: 'Shop' },
  { n: 3, label: 'Owner' },
  { n: 4, label: 'Staff' },
]);

function submit(): void {
  const value = slug.value.trim().toLowerCase();
  if (!SLUG_RULE.test(value)) {
    error.value = 'Use lowercase letters, numbers and dashes.';
    return;
  }
  setSlug(value);
  emit('submitted');
  if (props.nextTo) {
    router.push(props.nextTo.replace(':slug', value));
  }
}
</script>

<template>
  <div>
    <!-- Step indicator -->
    <ol class="mb-6 flex flex-wrap items-center gap-x-2 gap-y-2">
      <li v-for="s in steps" :key="s.n" class="flex items-center gap-2">
        <span
          :class="[
            'grid size-6 place-items-center rounded-full text-[11px] font-semibold',
            s.n < step ? 'bg-success text-white' : '',
            s.n === step ? 'bg-brand text-white' : '',
            s.n > step ? 'bg-line text-muted' : '',
          ]"
        >
          {{ s.n }}
        </span>
        <span
          :class="[
            'text-[12px] font-semibold tracking-wider uppercase',
            s.n === step ? 'text-ink' : 'text-muted',
          ]"
        >
          {{ s.label }}
        </span>
        <ArrowRight v-if="s.n < 4" class="size-3.5 text-line-strong" aria-hidden="true" />
      </li>
    </ol>

    <!-- Tenant picker -->
    <form
      v-if="!currentSlug"
      class="mb-6 flex flex-wrap items-end gap-3 rounded-card border border-line bg-surface px-5 py-4"
      @submit.prevent="submit"
    >
      <div class="min-w-[260px] flex-1">
        <UiInput
          id="tenant-slug"
          v-model="slug"
          label="Tenant slug"
          placeholder="e.g. demo"
          hint="Lowercase letters, numbers and dashes. This is the X-Tenant-Slug value."
          :rules="[(v: string) => (!SLUG_RULE.test(v.trim()) ? 'Use lowercase letters, numbers and dashes.' : '')]"
        />
      </div>
      <UiButton type="submit" :disabled="!slug">
        <template #icon><Search class="size-4" aria-hidden="true" /></template>
        Load tenant
      </UiButton>
    </form>

    <slot :slug="currentSlug" />
  </div>
</template>
