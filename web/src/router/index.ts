import { createRouter, createWebHistory } from 'vue-router';
import BookingPage from '../components/features/booking/BookingPage.vue';
import ShopNotFound from '../components/features/booking/ShopNotFound.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/:slug',
      name: 'booking',
      component: BookingPage,
      props: true,
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: ShopNotFound,
    },
  ],
});

export default router;
