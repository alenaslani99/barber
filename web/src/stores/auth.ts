import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { api, ApiRequestError } from '../lib/api';
import { clearBookingDrafts } from '../lib/draft';

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

interface TokenPayload {
  sub?: string;
  email?: string;
  given_name?: string;
  family_name?: string;
  role?: string;
  tenant?: string;
  [claim: string]: unknown;
}

const ROLE_CLAIMS = [
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role',
  'role',
];

function decodePayload(token: string): TokenPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const json = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
    const parsed: unknown = JSON.parse(json);
    if (parsed === null || typeof parsed !== 'object') return null;
    return parsed as TokenPayload;
  } catch {
    return null;
  }
}

let refreshTimer: ReturnType<typeof setTimeout> | null = null;

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null);
  const accessExpiresAt = ref<string | null>(null);
  let refreshPromise: Promise<string | null> | null = null;

  const isAuthenticated = computed(() => accessToken.value !== null);
  const payload = computed(() =>
    accessToken.value ? decodePayload(accessToken.value) : null,
  );
  const firstName = computed(() => payload.value?.given_name ?? '');
  const lastName = computed(() => payload.value?.family_name ?? '');
  const email = computed(() => payload.value?.email ?? '');
  const role = computed(() => {
    if (!payload.value) return '';
    for (const key of ROLE_CLAIMS) {
      const value = payload.value[key];
      if (typeof value === 'string' && value) return value;
    }
    return '';
  });
  const canManage = computed(() => role.value === 'Owner' || role.value === 'Barber');
  const displayName = computed(() => {
    const full = `${firstName.value} ${lastName.value}`.trim();
    if (full) return full.toUpperCase();
    if (email.value) {
      const fallback = email.value
        .split('@')[0]
        .replace(/[^a-z0-9]+/gi, ' ')
        .trim()
        .toUpperCase();
      if (fallback) return fallback;
    }
    return 'NALOG';
  });

  function clearTimer(): void {
    if (refreshTimer !== null) {
      clearTimeout(refreshTimer);
      refreshTimer = null;
    }
  }

  function scheduleRefresh(): void {
    clearTimer();
    if (!accessToken.value || !accessExpiresAt.value) return;
    const delay = new Date(accessExpiresAt.value).getTime() - Date.now() - 60_000;
    if (delay <= 0) return;
    refreshTimer = setTimeout(() => {
      void refreshTokens();
    }, delay);
  }

  function applySession(session: SessionResponse): void {
    accessToken.value = session.accessToken;
    accessExpiresAt.value = session.accessTokenExpiresAt;
    scheduleRefresh();
  }

  function clearSession(): void {
    accessToken.value = null;
    accessExpiresAt.value = null;
    refreshPromise = null;
    clearTimer();
  }

  async function login(userEmail: string, userPassword: string): Promise<void> {
    const session = await api<SessionResponse>('/api/auth/login', {
      method: 'POST',
      auth: false,
      body: { email: userEmail, password: userPassword },
    });
    applySession(session);
  }

  async function register(payload: RegisterPayload): Promise<void> {
    const session = await api<SessionResponse>('/api/auth/register', {
      method: 'POST',
      auth: false,
      body: payload,
    });
    applySession(session);
  }

  async function refreshTokens(): Promise<string | null> {
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
  }

  async function logout(): Promise<void> {
    try {
      await api<void>('/api/auth/logout', { method: 'POST', auth: false });
    } catch {
      // Clear locally regardless of server result.
    } finally {
      clearSession();
      clearBookingDrafts();
    }
  }

  async function boot(): Promise<void> {
    try {
      await refreshTokens();
    } catch {
      clearSession();
    }
  }

  return {
    accessToken,
    accessExpiresAt,
    isAuthenticated,
    firstName,
    lastName,
    email,
    role,
    canManage,
    displayName,
    login,
    register,
    refreshTokens,
    logout,
    boot,
  };
});
