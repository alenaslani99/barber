<script setup lang="ts">
import { computed, reactive, ref, watch, type Ref } from 'vue';
import { Plus, Scissors } from '@lucide/vue';
import UiAlert from '../../ui/UiAlert.vue';
import UiBadge from '../../ui/UiBadge.vue';
import UiButton from '../../ui/UiButton.vue';
import UiCard from '../../ui/UiCard.vue';
import UiInput from '../../ui/UiInput.vue';
import UiSpinner from '../../ui/UiSpinner.vue';
import { ApiError } from '../../../lib/api';
import { createService, getServices, type ShopService } from '../../../lib/tenants';

const props = defineProps<{ slug: string; disabled?: boolean }>();
const emit = defineEmits<{ count: [value: number] }>();

type NumberField = 'price' | 'durationMinutes' | 'slotMinutes';

const form = reactive({ name: '', price: '', durationMinutes: '30', slotMinutes: '30' });
const fieldErrors = reactive<Record<NumberField, string>>({ price: '', durationMinutes: '', slotMinutes: '' });
const nameInput: Ref<InstanceType<typeof UiInput> | null> = ref(null);

const services = ref<ShopService[]>([]);
const loading = ref(false);
const saving = ref(false);
const failure = ref('');

const euro = new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' });

const NUMBER_FIELDS: Array<{ key: NumberField; label: string; suffix: string; min: number; max: number; step: string }> = [
  { key: 'price', label: 'Price', suffix: '€', min: 0.01, max: 99999999, step: '0.01' },
  { key: 'durationMinutes', label: 'Duration', suffix: 'min', min: 5, max: 480, step: '5' },
  { key: 'slotMinutes', label: 'Slot', suffix: 'min', min: 5, max: 120, step: '5' },
];

const canSubmit = computed(() => !props.disabled && !loading.value && form.name.trim().length > 0);

async function load(slug: string): Promise<void> {
  if (!slug) return;
  loading.value = true;
  failure.value = '';
  try {
    services.value = await getServices(slug);
    emit('count', services.value.length);
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    failure.value = error.title;
  } finally {
    loading.value = false;
  }
}

watch(() => props.slug, load, { immediate: true });

function parse(field: NumberField): number | null {
  // Accept German-style decimal commas for the price.
  const value = Number(form[field].replace(',', '.'));
  const spec = NUMBER_FIELDS.find((f) => f.key === field)!;
  if (!form[field].trim() || Number.isNaN(value) || value < spec.min || value > spec.max) {
    fieldErrors[field] = `${spec.label}: ${spec.min}–${spec.max} ${spec.suffix}.`;
    return null;
  }
  if (field !== 'price' && !Number.isInteger(value)) {
    fieldErrors[field] = `${spec.label} must be whole minutes.`;
    return null;
  }
  fieldErrors[field] = '';
  return value;
}

async function submit(): Promise<void> {
  const nameOk = nameInput.value?.validate() ?? false;
  const price = parse('price');
  const durationMinutes = parse('durationMinutes');
  const slotMinutes = parse('slotMinutes');
  if (!nameOk || price === null || durationMinutes === null || slotMinutes === null) return;

  saving.value = true;
  failure.value = '';
  try {
    await createService(props.slug, { name: form.name.trim(), price, durationMinutes, slotMinutes });
    form.name = '';
    form.price = '';
    nameInput.value?.reset();
    await load(props.slug);
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    if (error.status === 409 && /already exists/i.test(error.title)) {
      nameInput.value?.setError(error.title);
      return;
    }
    const nameError = error.errors.Name?.[0];
    if (nameError) nameInput.value?.setError(nameError);
    fieldErrors.price = error.errors.Price?.[0] ?? '';
    fieldErrors.durationMinutes = error.errors.DurationMinutes?.[0] ?? '';
    fieldErrors.slotMinutes = error.errors.SlotMinutes?.[0] ?? '';
    const placed = nameError || fieldErrors.price || fieldErrors.durationMinutes || fieldErrors.slotMinutes;
    if (!placed) failure.value = error.title;
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiCard title="Services" :description="`${services.length} on ${slug}`">
    <template #actions>
      <Scissors class="size-4 text-muted" aria-hidden="true" />
    </template>

    <div v-if="loading" class="grid place-items-center py-10 text-muted">
      <UiSpinner class="size-5" />
    </div>

    <div v-else class="space-y-5">
      <UiAlert v-if="failure" tone="danger" title="Service not saved">{{ failure }}</UiAlert>

      <form class="space-y-4" novalidate @submit.prevent="submit">
        <UiInput
          id="service-name"
          ref="nameInput"
          v-model="form.name"
          label="Name"
          placeholder="e.g. Haircut"
          required
          :disabled="disabled"
          :rules="[(v) => (!v.trim() ? 'Name is required.' : '')]"
        />

        <div class="grid gap-4 sm:grid-cols-3">
          <div v-for="field in NUMBER_FIELDS" :key="field.key">
            <label
              :for="`service-${field.key}`"
              class="mb-1.5 block text-[12px] font-semibold tracking-wider text-muted uppercase"
            >
              {{ field.label }} <span class="text-danger" aria-hidden="true">*</span>
            </label>
            <div class="relative">
              <input
                :id="`service-${field.key}`"
                v-model="form[field.key]"
                type="text"
                :inputmode="field.key === 'price' ? 'decimal' : 'numeric'"
                :disabled="disabled"
                :aria-invalid="!!fieldErrors[field.key]"
                :aria-describedby="fieldErrors[field.key] ? `service-${field.key}-error` : undefined"
                :class="[
                  'w-full rounded-field border bg-surface py-2 pr-12 pl-3 font-mono text-[14px] text-ink disabled:bg-canvas disabled:text-muted',
                  fieldErrors[field.key]
                    ? 'border-danger focus:border-danger focus:ring-2 focus:ring-danger/25'
                    : 'border-line-strong hover:border-muted focus:border-brand focus:ring-2 focus:ring-brand/20',
                ]"
                @input="fieldErrors[field.key] = ''"
              />
              <span class="pointer-events-none absolute inset-y-0 right-3 grid place-items-center text-[12px] text-muted">
                {{ field.suffix }}
              </span>
            </div>
            <p
              v-if="fieldErrors[field.key]"
              :id="`service-${field.key}-error`"
              class="mt-1.5 text-[12px] font-medium text-danger"
            >
              {{ fieldErrors[field.key] }}
            </p>
          </div>
        </div>
        <p class="text-[12px] text-muted">
          Slot is the booking grid step: a 45 min service on a 15 min slot can start at 09:00,
          09:15, 09:30…
        </p>

        <UiButton type="submit" :disabled="!canSubmit" :loading="saving">
          <template #icon>
            <UiSpinner v-if="saving" class="size-4" />
            <Plus v-else class="size-4" aria-hidden="true" />
          </template>
          Add service
        </UiButton>
      </form>

      <p v-if="services.length === 0" class="border-t border-line pt-5 text-center text-[13px] text-muted">
        No services yet. Without at least one, clients cannot book.
      </p>
      <div v-else class="-mx-5 -mb-5 overflow-x-auto border-t border-line">
        <table class="w-full text-left text-[13px]">
          <thead class="border-b border-line text-[11px] tracking-wider text-muted uppercase">
            <tr>
              <th class="px-5 py-2.5 font-semibold">Service</th>
              <th class="px-5 py-2.5 text-right font-semibold">Price</th>
              <th class="px-5 py-2.5 text-right font-semibold">Duration</th>
              <th class="px-5 py-2.5 text-right font-semibold">Slot</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="service in services" :key="service.id" class="border-b border-line last:border-0">
              <td class="px-5 py-3 font-medium text-ink">
                {{ service.name }}
                <UiBadge v-if="!service.isActive" tone="neutral" class="ml-2">Inactive</UiBadge>
              </td>
              <td class="px-5 py-3 text-right font-mono text-ink">{{ euro.format(service.price) }}</td>
              <td class="px-5 py-3 text-right font-mono text-muted">{{ service.durationMinutes }} min</td>
              <td class="px-5 py-3 text-right font-mono text-muted">{{ service.slotMinutes }} min</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </UiCard>
</template>
