<script setup lang="ts">
import { computed, ref } from 'vue';
import type { BookingStep } from '../../../types/booking';
import { formatDuration, formatPrice } from '../../../utils/format';
import Button from '../../ui/Button.vue';
import Container from '../../layout/Container.vue';
import Header from '../../layout/Header.vue';
import { fixtureServices } from './fixtures';
import ServiceStep from './ServiceStep.vue';
import StepProgress from './StepProgress.vue';
import SummaryRail from './SummaryRail.vue';

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
const selectedServiceId = ref<string | null>(null);

const selectedService = computed(() =>
  fixtureServices.find((service) => service.id === selectedServiceId.value) ?? null,
);

const ctaLabel = computed(() =>
  selectedService.value
    ? `Continue · ${formatPrice(selectedService.value.priceCents)} · ${formatDuration(selectedService.value.durationMinutes)}`
    : 'Select a service',
);
</script>

<template>
  <div class="min-h-screen bg-background text-text">
    <Header :shop-name="props.slug" />

    <Container class="py-6 sm:py-8">
      <h1 class="font-display text-3xl leading-tight sm:text-4xl">Book your chair</h1>
      <p class="mt-2 text-sm text-muted">Pick a service to get started. No account needed until the end.</p>

      <StepProgress :steps="steps" :current-index="currentIndex" class="mt-6" />

      <div class="mt-6 grid gap-6 lg:grid-cols-[1fr_320px]">
        <section
          aria-label="Choose a service"
          class="rounded-lg border border-white/8 bg-surface px-4 py-2 shadow-subtle sm:px-6"
        >
          <ServiceStep :services="fixtureServices" :selected-id="selectedServiceId" @select="selectedServiceId = $event" />
        </section>

        <aside aria-label="Booking summary" class="hidden lg:block">
          <SummaryRail
            :service-name="selectedService?.name ?? null"
            :duration-minutes="selectedService?.durationMinutes ?? null"
            :price-cents="selectedService?.priceCents ?? null"
          />
        </aside>
      </div>
    </Container>

    <div class="sticky bottom-0 border-t border-white/8 bg-surface p-4 lg:hidden">
      <Button class="w-full" :disabled="selectedService === null">{{ ctaLabel }}</Button>
    </div>
  </div>
</template>
