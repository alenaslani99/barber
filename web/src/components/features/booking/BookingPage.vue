<script setup lang="ts">
import { ref } from 'vue';
import type { BookingStep } from '../../../types/booking';
import Button from '../../ui/Button.vue';
import Container from '../../layout/Container.vue';
import Header from '../../layout/Header.vue';
import StepProgress from './StepProgress.vue';

interface Props {
  slug: string;
}

const props = defineProps<Props>();

const steps: BookingStep[] = [
  { key: 'service', label: 'Service' },
  { key: 'barber', label: 'Barber' },
  { key: 'slots', label: 'Time' },
  { key: 'details', label: 'Details' },
  { key: 'ticket', label: 'Done' },
];

const currentIndex = ref(0);
</script>

<template>
  <div class="min-h-screen bg-background text-text">
    <Header :shop-name="props.slug" />

    <Container class="py-6">
      <StepProgress :steps="steps" :current-index="currentIndex" />

      <div class="mt-6 grid gap-6 lg:grid-cols-[1fr_320px]">
        <section aria-label="Booking step" class="rounded-lg border border-black/8 bg-white p-6 shadow-subtle">
          <p class="text-body">Step component lands here: {{ steps[currentIndex].label }}.</p>
        </section>

        <aside aria-label="Booking summary" class="hidden lg:block">
          <div class="sticky top-6 rounded-lg border border-black/8 bg-white p-6 shadow-subtle">
            <p class="text-body font-semibold">Summary</p>
          </div>
        </aside>
      </div>
    </Container>

    <div class="sticky bottom-0 border-t border-black/8 bg-white p-4 lg:hidden">
      <Button class="w-full">Continue</Button>
    </div>
  </div>
</template>
