import { computed, ref } from 'vue';

/**
 * Holds the tenant slug the operator is currently working on. Steps 2-4 are
 * keyed by this value and the sidebar uses it to unlock the onboarding links.
 *
 * Deliberately a plain module ref, not Pinia: this is session-only UI state
 * that must never be persisted.
 */
const currentSlug = ref<string>('');

export function useTenantStore() {
  const hasTenant = computed(() => currentSlug.value.length > 0);

  function setSlug(slug: string): void {
    currentSlug.value = slug.trim().toLowerCase();
  }

  function clear(): void {
    currentSlug.value = '';
  }

  return { currentSlug, hasTenant, setSlug, clear };
}
