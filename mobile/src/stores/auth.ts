import { Platform } from 'react-native';
import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { api, ApiRequestError } from '../lib/api';
import { clearUserCache } from '../lib/cache';
import { decodePayload, payloadDisplayName, payloadRole } from '../lib/token';

export interface SessionResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
}

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
}

const TOKEN_KEY = 'barber.auth.token';
const EXPIRES_KEY = 'barber.auth.expiresAt';

// SecureStore is native-only; web falls back to AsyncStorage.
async function persistSession(token: string | null, expiresAt: string | null): Promise<void> {
  try {
    if (Platform.OS === 'web') {
      if (token && expiresAt) {
        await AsyncStorage.multiSet([
          [TOKEN_KEY, token],
          [EXPIRES_KEY, expiresAt],
        ]);
      } else {
        await AsyncStorage.multiRemove([TOKEN_KEY, EXPIRES_KEY]);
      }
      return;
    }
    if (token && expiresAt) {
      await SecureStore.setItemAsync(TOKEN_KEY, token);
      await SecureStore.setItemAsync(EXPIRES_KEY, expiresAt);
    } else {
      await SecureStore.deleteItemAsync(TOKEN_KEY);
      await SecureStore.deleteItemAsync(EXPIRES_KEY);
    }
  } catch {
    // Session persistence is best-effort.
  }
}

async function readStoredSession(): Promise<SessionResponse | null> {
  try {
    let token: string | null;
    let expiresAt: string | null;
    if (Platform.OS === 'web') {
      const pairs = await AsyncStorage.multiGet([TOKEN_KEY, EXPIRES_KEY]);
      token = pairs[0]?.[1] ?? null;
      expiresAt = pairs[1]?.[1] ?? null;
    } else {
      token = await SecureStore.getItemAsync(TOKEN_KEY);
      expiresAt = await SecureStore.getItemAsync(EXPIRES_KEY);
    }
    if (token && expiresAt) return { accessToken: token, accessTokenExpiresAt: expiresAt };
    return null;
  } catch {
    return null;
  }
}

function isExpired(expiresAt: string | null, skewMs = 30_000): boolean {
  if (!expiresAt) return true;
  return new Date(expiresAt).getTime() - Date.now() <= skewMs;
}

let refreshTimer: ReturnType<typeof setTimeout> | null = null;
let refreshPromise: Promise<string | null> | null = null;

interface AuthState {
  accessToken: string | null;
  accessTokenExpiresAt: string | null;
  hydrated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  refreshTokens: () => Promise<string | null>;
  logout: () => Promise<void>;
  boot: () => Promise<void>;
}

function clearTimer(): void {
  if (refreshTimer !== null) {
    clearTimeout(refreshTimer);
    refreshTimer = null;
  }
}

export const useAuthStore = create<AuthState>()((set, get) => {
  function scheduleRefresh(): void {
    clearTimer();
    const { accessToken, accessTokenExpiresAt } = get();
    if (!accessToken || !accessTokenExpiresAt) return;
    const delay = new Date(accessTokenExpiresAt).getTime() - Date.now() - 60_000;
    if (delay <= 0) return;
    refreshTimer = setTimeout(() => {
      void get().refreshTokens();
    }, delay);
  }

  function applySession(session: SessionResponse): void {
    set({ accessToken: session.accessToken, accessTokenExpiresAt: session.accessTokenExpiresAt });
    scheduleRefresh();
    void persistSession(session.accessToken, session.accessTokenExpiresAt);
  }

  function clearSession(): void {
    set({ accessToken: null, accessTokenExpiresAt: null });
    refreshPromise = null;
    clearTimer();
    void persistSession(null, null);
  }

  return {
    accessToken: null,
    accessTokenExpiresAt: null,
    hydrated: false,

    login: async (userEmail: string, userPassword: string): Promise<void> => {
      // Fresh account context — never reuse the previous user's cached data.
      await clearUserCache();
      const session = await api<SessionResponse>('/api/auth/login', {
        method: 'POST',
        auth: false,
        body: { email: userEmail, password: userPassword },
      });
      applySession(session);
    },

    register: async (payload: RegisterPayload): Promise<void> => {
      await clearUserCache();
      const session = await api<SessionResponse>('/api/auth/register', {
        method: 'POST',
        auth: false,
        body: payload,
      });
      applySession(session);
    },

    refreshTokens: async (): Promise<string | null> => {
      if (refreshPromise) return refreshPromise;
      refreshPromise = (async () => {
        try {
          const session = await api<SessionResponse>('/api/auth/refresh', {
            method: 'POST',
            auth: false,
            retry: false,
          });
          applySession(session);
          return session.accessToken;
        } catch (error) {
          if (!(error instanceof ApiRequestError) || error.status !== 0) clearSession();
          return null;
        } finally {
          refreshPromise = null;
        }
      })();
      return refreshPromise;
    },

    logout: async (): Promise<void> => {
      try {
        await api<void>('/api/auth/logout', { method: 'POST', auth: false });
      } catch {
        // Clear locally regardless of server result.
      } finally {
        clearSession();
        await clearUserCache();
      }
    },

    boot: async (): Promise<void> => {
      const stored = await readStoredSession();
      if (stored && !isExpired(stored.accessTokenExpiresAt)) {
        set({
          accessToken: stored.accessToken,
          accessTokenExpiresAt: stored.accessTokenExpiresAt,
        });
        scheduleRefresh();
      }
      set({ hydrated: true });
      const before = get().accessToken;
      const refreshed = await get().refreshTokens();
      if (!refreshed && before && get().accessToken === null && stored) {
        // Refresh cookie missing/expired but the stored access token is still
        // bearer-valid — keep the session instead of logging out.
        if (!isExpired(stored.accessTokenExpiresAt)) {
          set({
            accessToken: stored.accessToken,
            accessTokenExpiresAt: stored.accessTokenExpiresAt,
          });
          scheduleRefresh();
        }
      }
    },
  };
});

export function useIsAuthenticated(): boolean {
  return useAuthStore((s) => s.accessToken !== null);
}

export function useDisplayName(): string {
  const token = useAuthStore((s) => s.accessToken);
  return payloadDisplayName(token ? decodePayload(token) : null);
}

export function useRole(): string {
  const token = useAuthStore((s) => s.accessToken);
  return payloadRole(token ? decodePayload(token) : null);
}
