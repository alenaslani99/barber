<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { fetchBarbers, type ApiBarber } from '../lib/catalog';
import { useAuthStore } from '../stores/auth';
import { useShopStore } from '../stores/shop';
import BookingsSection from '../components/features/owner/BookingsSection.vue';
import DaysOffSection from '../components/features/owner/DaysOffSection.vue';
import HoursSection from '../components/features/owner/HoursSection.vue';
import ServicesSection from '../components/features/owner/ServicesSection.vue';
import StaffSection from '../components/features/owner/StaffSection.vue';

type Section = 'bookings' | 'daysoff' | 'services' | 'staff' | 'hours';
const section = ref<Section>('bookings');

const SECTIONS: ReadonlyArray<{ id: Section; label: string }> = [
  { id: 'bookings', label: 'REZERVACIJE' },
  { id: 'daysoff', label: 'NERADNI DANI' },
  { id: 'services', label: 'USLUGE' },
  { id: 'staff', label: 'OSOBLJE' },
  { id: 'hours', label: 'RADNO VREME' },
];

const auth = useAuthStore();
const router = useRouter();
const shopStore = useShopStore();

const isOwner = computed(() => auth.role === 'Owner');
const staff = ref<ApiBarber[]>([]);

async function loadStaff(): Promise<void> {
  try {
    staff.value = await fetchBarbers(true, auth.accessToken);
  } catch {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { next: '/owner' } });
    }
  }
}

onMounted(() => {
  void shopStore.load();
  if (isOwner.value) void loadStaff();
});
</script>

<template>
  <main class="bg-ink text-bone">
    <div class="mx-auto w-full max-w-6xl px-4 py-10">
      <p v-if="shopStore.shop" class="text-center text-lg tracking-widest">
        <RouterLink to="/" class="text-ash hover:text-bone" aria-label="Nazad na početnu">
          {{ shopStore.shop.name }}
        </RouterLink>
      </p>
      <h1 class="mt-2 text-center font-display text-6xl leading-none tracking-wide">PANEL</h1>

      <nav
        v-if="isOwner"
        aria-label="Sekcije panela"
        class="mt-8 flex flex-col border border-line lg:hidden"
      >
        <button
          v-for="s in SECTIONS"
          :key="s.id"
          type="button"
          :aria-current="section === s.id ? 'page' : undefined"
          :class="section === s.id ? 'bg-bone text-ink' : 'text-ash'"
          class="border-b border-line py-2 text-xl tracking-widest last:border-b-0"
          @click="section = s.id"
        >
          {{ s.label }}
        </button>
      </nav>

      <div class="mt-8 lg:mt-10 lg:grid lg:grid-cols-[240px_1fr] lg:gap-8">
        <aside v-if="isOwner" class="hidden lg:block" aria-label="Sekcije panela">
          <nav class="flex flex-col border border-line">
            <button
              v-for="s in SECTIONS"
              :key="s.id"
              type="button"
              :aria-current="section === s.id ? 'page' : undefined"
              :class="section === s.id ? 'bg-bone text-ink' : 'text-ash hover:text-bone'"
              class="border-b border-line px-4 py-3 text-left text-xl tracking-widest last:border-b-0"
              @click="section = s.id"
            >
              {{ s.label }}
            </button>
          </nav>
        </aside>

        <div>
          <BookingsSection
            v-if="section === 'bookings'"
            :staff="staff"
            :is-owner="isOwner"
          />
          <DaysOffSection
            v-if="isOwner && section === 'daysoff'"
            :staff="staff"
          />
          <ServicesSection v-if="isOwner && section === 'services'" />
          <StaffSection
            v-if="isOwner && section === 'staff'"
            :staff="staff"
            @staff-changed="loadStaff"
          />
          <HoursSection v-if="isOwner && section === 'hours'" />
        </div>
      </div>
    </div>
  </main>
</template>
