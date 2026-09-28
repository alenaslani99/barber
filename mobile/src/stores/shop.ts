import { create } from 'zustand';
import { fetchShop, type ApiShop } from '../lib/catalog';
import { CACHE_KEYS, CACHE_TTL_MIN, cacheGet, cacheSet } from '../lib/cache';

interface ShopState {
  shop: ApiShop | null;
  loading: boolean;
  load: () => Promise<ApiShop | null>;
}

let promise: Promise<ApiShop | null> | null = null;

export const useShopStore = create<ShopState>()((set, get) => ({
  shop: null,
  loading: false,

  // Stale-while-revalidate: cached shop renders instantly, API refreshes behind.
  load: async (): Promise<ApiShop | null> => {
    if (get().shop) return get().shop;
    if (promise) return promise;
    const cached = await cacheGet<ApiShop>(CACHE_KEYS.shop, CACHE_TTL_MIN.shop);
    if (cached) {
      set({ shop: cached });
      // Background revalidate, no spinner.
      void fetchShop()
        .then((data) => {
          set({ shop: data });
          return cacheSet(CACHE_KEYS.shop, data);
        })
        .catch(() => undefined);
      return cached;
    }
    set({ loading: true });
    promise = fetchShop()
      .then((data) => {
        set({ shop: data });
        void cacheSet(CACHE_KEYS.shop, data);
        return data;
      })
      .catch(() => null)
      .finally(() => {
        set({ loading: false });
        promise = null;
      });
    return promise;
  },
}));
