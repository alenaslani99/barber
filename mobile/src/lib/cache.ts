import AsyncStorage from '@react-native-async-storage/async-storage';

interface CacheEnvelope<T> {
  data: T;
  savedAt: number;
}

export const CACHE_KEYS = {
  shop: 'barber.cache.shop',
  profile: 'barber.cache.profile',
  history: 'barber.cache.history',
} as const;

// TTLs in minutes: display data can go stale, bookings logic stays realtime.
export const CACHE_TTL_MIN = {
  shop: 60,
  profile: 60,
  history: 15,
} as const;

export async function cacheGet<T>(key: string, ttlMin: number): Promise<T | null> {
  try {
    const raw = await AsyncStorage.getItem(key);
    if (!raw) return null;
    const envelope = JSON.parse(raw) as CacheEnvelope<T>;
    if (Date.now() - envelope.savedAt > ttlMin * 60_000) return null;
    return envelope.data;
  } catch {
    return null;
  }
}

export async function cacheSet<T>(key: string, data: T): Promise<void> {
  try {
    const envelope: CacheEnvelope<T> = { data, savedAt: Date.now() };
    await AsyncStorage.setItem(key, JSON.stringify(envelope));
  } catch {
    // Cache is best-effort.
  }
}

export async function cacheInvalidate(key: string): Promise<void> {
  try {
    await AsyncStorage.removeItem(key);
  } catch {
    // ignore
  }
}

// Per-user data must not leak across accounts.
export async function clearUserCache(): Promise<void> {
  await Promise.all([
    cacheInvalidate(CACHE_KEYS.profile),
    cacheInvalidate(CACHE_KEYS.history),
  ]);
}
