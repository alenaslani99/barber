<script setup lang="ts">
import { useRoute } from 'vue-router';
import {
  Building2,
  ClipboardList,
  LayoutDashboard,
  Lock,
  Plus,
  Scissors,
  Server,
  Settings,
  UserCog,
  X,
} from '@lucide/vue';
import { RouterLink } from 'vue-router';
import type { NavSection } from '../../types/nav';
import { useTenantStore } from '../../stores/tenant';
import { cn } from '../../utils/cn';

/** Mobile drawer state, owned by AppShell. */
const open = defineModel<boolean>('open', { default: false });

const route = useRoute();
const { hasTenant, currentSlug } = useTenantStore();

const sections: NavSection[] = [
  {
    label: 'Overview',
    items: [{ label: 'Dashboard', icon: LayoutDashboard, to: '/', match: 'dashboard' }],
  },
  {
    label: 'Tenants',
    items: [
      { label: 'All Clients', icon: Building2, to: '/tenants', match: 'tenants' },
      { label: 'New Tenant', icon: Plus, to: '/tenants/new', match: 'tenant-new' },
    ],
  },
  {
    label: 'Onboarding',
    items: [
      { label: 'Shop Setup', icon: ClipboardList, to: '/tenants/:slug/shop', match: 'tenant-shop', requiresTenant: true },
      { label: 'Owner Account', icon: UserCog, to: '/tenants/:slug/owner', match: 'tenant-owner', requiresTenant: true },
      { label: 'Staff & Services', icon: Scissors, to: '/tenants/:slug/seed', match: 'tenant-seed', requiresTenant: true },
    ],
  },
  {
    label: 'System',
    items: [
      { label: 'Health', icon: Server, to: '/system/health', match: 'health' },
      { label: 'Settings', icon: Settings, to: '/system/settings', match: 'settings' },
    ],
  },
];

function isActive(item: { match?: string }): boolean {
  return item.match !== undefined && route.name === item.match;
}

function isLocked(item: { requiresTenant?: boolean }): boolean {
  return item.requiresTenant === true && !hasTenant.value;
}

/** Locked items keep their URL shape so the lock is purely visual + a11y. */
function target(to: string): string {
  return to.replace(':slug', currentSlug.value || '_');
}
</script>

<template>
  <!-- Mobile overlay -->
  <div
    v-if="open"
    class="fixed inset-0 z-30 bg-ink/50 lg:hidden"
    @click="open = false"
  />

  <aside
    :class="
      cn(
        'fixed inset-y-0 left-0 z-40 flex w-64 flex-col bg-sidebar transition-transform lg:translate-x-0',
        open ? 'translate-x-0' : '-translate-x-full',
      )
    "
  >
    <!-- Brand -->
    <div class="flex items-center border-b border-white/10 px-5 py-4">
      <p class="min-w-0 flex-1 truncate text-[14px] font-semibold tracking-wide text-white">
        Control Panel
      </p>
      <button
        type="button"
        class="rounded-field p-1.5 text-sidebar-text hover:bg-white/10 hover:text-white lg:hidden"
        aria-label="Close navigation"
        @click="open = false"
      >
        <X class="size-4" aria-hidden="true" />
      </button>
    </div>

    <!-- Nav -->
    <nav class="flex-1 overflow-y-auto px-3 py-4" aria-label="Main">
      <div v-for="section in sections" :key="section.label" class="mb-5 last:mb-0">
        <p
          class="mb-1.5 px-3 text-[10px] font-semibold tracking-[0.12em] text-sidebar-text/70 uppercase"
        >
          {{ section.label }}
        </p>
        <ul class="space-y-0.5">
          <li v-for="item in section.items" :key="item.label">
            <RouterLink
              v-if="!isLocked(item)"
              :to="target(item.to)"
              :aria-current="isActive(item) ? 'page' : undefined"
              :class="
                cn(
                  'flex items-center gap-3 rounded-field px-3 py-2 text-[13px] font-medium transition-colors',
                  isActive(item)
                    ? 'bg-sidebar-active text-sidebar-text-active'
                    : 'text-sidebar-text hover:bg-sidebar-hover hover:text-white',
                )
              "
              @click="open = false"
            >
              <component
                :is="item.icon"
                :class="cn('size-4 shrink-0', isActive(item) ? 'text-white' : 'text-sidebar-text')"
                aria-hidden="true"
              />
              <span class="truncate">{{ item.label }}</span>
            </RouterLink>

            <span
              v-else
              class="flex cursor-not-allowed items-center gap-3 rounded-field px-3 py-2 text-[13px] font-medium text-sidebar-text/40"
              :aria-disabled="true"
              :title="'Create a tenant first'"
            >
              <component :is="item.icon" class="size-4 shrink-0" aria-hidden="true" />
              <span class="truncate">{{ item.label }}</span>
              <Lock class="ml-auto size-3.5 shrink-0" aria-hidden="true" />
            </span>
          </li>
        </ul>
      </div>
    </nav>

    <!-- Footer -->
    <div class="border-t border-white/10 px-5 py-3.5">
      <p class="truncate text-[11px] text-sidebar-text/60">
        admin@local · v0.0.0
      </p>
    </div>
  </aside>
</template>
