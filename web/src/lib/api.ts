export interface ApiErrorBody {
  status: number;
  title: string;
  errors: Record<string, string[]>;
}

export class ApiRequestError extends Error {
  readonly status: number;
  readonly title: string;
  readonly errors: Record<string, string[]>;

  constructor(status: number, title: string, errors: Record<string, string[]> = {}) {
    super(title);
    this.name = 'ApiRequestError';
    this.status = status;
    this.title = title;
    this.errors = errors;
  }
}

interface ApiOptions {
  method?: string;
  body?: unknown;
  auth?: boolean;
  token?: string | null;
  retry?: boolean;
}

const BASE_URL = import.meta.env.VITE_API_URL as string;
const TENANT_SLUG = import.meta.env.VITE_TENANT_SLUG as string;

function parseError(status: number, data: unknown): ApiRequestError {
  if (data !== null && typeof data === 'object') {
    const body = data as { title?: unknown; errors?: unknown };
    const title = typeof body.title === 'string' && body.title ? body.title : 'GREŠKA';
    const errors: Record<string, string[]> = {};
    if (body.errors !== null && typeof body.errors === 'object') {
      const raw = body.errors as Record<string, unknown>;
      for (const key of Object.keys(raw)) {
        const value = raw[key];
        if (Array.isArray(value)) {
          errors[key] = value.filter((v): v is string => typeof v === 'string');
        }
      }
    }
    return new ApiRequestError(status, title, errors);
  }
  return new ApiRequestError(status, 'GREŠKA');
}

export async function api<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'X-Tenant-Slug': TENANT_SLUG,
  };
  if (options.auth !== false && options.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }
  let response: Response;
  try {
    response = await fetch(`${BASE_URL}${path}`, {
      method: options.method ?? 'GET',
      headers,
      credentials: 'include',
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
    });
  } catch {
    throw new ApiRequestError(0, 'GREŠKA U VEZI, POKUŠAJ PONOVO');
  }
  if (response.status === 401 && options.auth !== false && options.retry !== false) {
    const { useAuthStore } = await import('../stores/auth');
    const refreshed = await useAuthStore().refreshTokens();
    if (refreshed) {
      return api<T>(path, { ...options, token: refreshed, retry: false });
    }
  }
  if (response.status === 204) return undefined as T;
  const data: unknown = await response.json().catch(() => null);
  if (!response.ok) throw parseError(response.status, data);
  return data as T;
}
