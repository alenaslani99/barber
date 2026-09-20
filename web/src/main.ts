import { createApp } from 'vue';
import { createPinia } from 'pinia';
import './style.css';
import App from './App.vue';
import router from './router';
import { useAuthStore } from './stores/auth';

async function bootstrap(): Promise<void> {
  const app = createApp(App);
  app.use(createPinia());
  app.use(router);
  try {
    await useAuthStore().boot();
  } catch {
    // Stay logged out when the silent refresh fails.
  }
  app.mount('#app');
}

void bootstrap();
