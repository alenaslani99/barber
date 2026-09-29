import { createRouter, createWebHistory } from 'vue-router';
import AppShell from '../components/layout/AppShell.vue';
import DashboardView from '../views/DashboardView.vue';
import HealthView from '../views/HealthView.vue';
import NewTenantView from '../views/NewTenantView.vue';
import NotFoundView from '../views/NotFoundView.vue';
import OwnerSetupView from '../views/OwnerSetupView.vue';
import SeedSetupView from '../views/SeedSetupView.vue';
import SettingsView from '../views/SettingsView.vue';
import ShopSetupView from '../views/ShopSetupView.vue';
import TenantsView from '../views/TenantsView.vue';

/**
 * The auth guard below is intentionally inert today. It already reads
 * `meta.requiresAuth` / `meta.roles` so adding authentication later is a
 * policy change, not a refactor. Never persist tokens here — in-memory only.
 */
const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: AppShell,
      children: [
        {
          path: '',
          name: 'dashboard',
          component: DashboardView,
          meta: { title: 'Dashboard' },
        },
        {
          path: 'tenants',
          name: 'tenants',
          component: TenantsView,
          meta: { title: 'All Clients' },
        },
        {
          path: 'tenants/new',
          name: 'tenant-new',
          component: NewTenantView,
          meta: { title: 'New Tenant', requiresAuth: true },
        },
        {
          path: 'tenants/:slug/shop',
          name: 'tenant-shop',
          component: ShopSetupView,
          meta: { title: 'Shop Setup', requiresAuth: true },
        },
        {
          path: 'tenants/:slug/owner',
          name: 'tenant-owner',
          component: OwnerSetupView,
          meta: { title: 'Owner Account', requiresAuth: true },
        },
        {
          path: 'tenants/:slug/seed',
          name: 'tenant-seed',
          component: SeedSetupView,
          meta: { title: 'Staff & Services', requiresAuth: true },
        },
        {
          path: 'system/health',
          name: 'health',
          component: HealthView,
          meta: { title: 'Health' },
        },
        {
          path: 'system/settings',
          name: 'settings',
          component: SettingsView,
          meta: { title: 'Settings', requiresAuth: true },
        },
      ],
    },
    { path: '/:pathMatch(.*)*', name: 'notfound', component: NotFoundView },
  ],
});

router.beforeEach((to) => {
  if (to.name === 'notfound') return undefined;
  // Placeholder: no session exists yet, so nothing is rejected.
  return undefined;
});

export default router;
