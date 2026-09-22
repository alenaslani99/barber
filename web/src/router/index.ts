import { createRouter, createWebHistory } from 'vue-router';
import AccountView from '../views/AccountView.vue';
import BookingView from '../views/BookingView.vue';
import LoginView from '../views/LoginView.vue';
import NotFoundView from '../views/NotFoundView.vue';
import OwnerView from '../views/OwnerView.vue';
import RegisterView from '../views/RegisterView.vue';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'booking', component: BookingView, meta: { requiresAuth: true } },
    { path: '/login', name: 'login', component: LoginView },
    { path: '/register', name: 'register', component: RegisterView },
    {
      path: '/account',
      name: 'account',
      component: AccountView,
      meta: { requiresAuth: true },
    },
    {
      path: '/owner',
      name: 'owner',
      component: OwnerView,
      meta: { requiresAuth: true, roles: ['Owner', 'Barber'], hidden: true },
    },
    { path: '/:pathMatch(.*)*', name: 'notfound', component: NotFoundView },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();
  const roles = to.meta.roles;
  const roleOk = !Array.isArray(roles) || roles.includes(auth.role);
  if (to.meta.hidden === true && (!auth.isAuthenticated || !roleOk)) {
    return { name: 'notfound' };
  }
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { path: '/login', query: { next: to.fullPath } };
  }
  if (!roleOk) {
    return { path: '/' };
  }
  return undefined;
});

export default router;
