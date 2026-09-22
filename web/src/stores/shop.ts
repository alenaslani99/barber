import { ref } from 'vue';
import { defineStore } from 'pinia';
import { fetchShop, type ApiShop } from '../lib/catalog';

export const useShopStore = defineStore('shop', () => {
  const shop = ref<ApiShop | null>(null);
  const loading = ref(false);
  let promise: Promise<ApiShop | null> | null = null;

  async function load(): Promise<ApiShop | null> {
    if (shop.value) return shop.value;
    if (promise) return promise;
    loading.value = true;
    promise = fetchShop()
      .then((data) => {
        shop.value = data;
        return data;
      })
      .catch(() => null)
      .finally(() => {
        loading.value = false;
        promise = null;
      });
    return promise;
  }

  return { shop, loading, load };
});
