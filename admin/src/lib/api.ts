/**
 * Fetch wrapper for the local-only admin API. Every call carries the admin key;
 * ProblemDetails responses surface as a typed ApiError.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly title: string;
  readonly errors: Record<string, string[]>;

  constructor(status: number, title: string, errors: Record<string, string[]> = {}) {
    super(title);
    this.name = 'ApiError';
    this.status = status;
    this.title = title;
    this.errors = errors;
  }
}

interface ApiOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
}

export const API_URL = import.meta.env.VITE_API_URL;
const ADMIN_KEY = import.meta.env.VITE_ADMIN_KEY;

function fallbackTitle(status: number): string {
  if (status === 401) return 'Admin key rejected. Check VITE_ADMIN_KEY.';
  if (status === 404) return 'Not found.';
  return `Request failed (${status}).`;
}

function parseError(status: number, data: unknown): ApiError {
  if (data === null || typeof data !== 'object') return new ApiError(status, fallbackTitle(status));

  const body = data as { title?: unknown; errors?: unknown };
  const title = typeof body.title === 'string' && body.title ? body.title : fallbackTitle(status);
  const errors: Record<string, string[]> = {};
  if (body.errors !== null && typeof body.errors === 'object') {
    for (const [key, value] of Object.entries(body.errors as Record<string, unknown>)) {
      if (Array.isArray(value)) errors[key] = value.filter((v): v is string => typeof v === 'string');
    }
  }
  return new ApiError(status, title, errors);
}

export async function api<T>(path: string, options: ApiOptions = {}): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method: options.method ?? 'GET',
      headers: {
        'Content-Type': 'application/json',
        'X-Admin-Key': ADMIN_KEY,
      },
      // Provisioning creates a database and runs every migration; give it room.
      signal: AbortSignal.timeout(60000),
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
    });
  } catch {
    throw new ApiError(0, `API not reachable at ${API_URL}. Is the backend running?`);
  }

  if (response.status === 204) return undefined as T;
  const data: unknown = await response.json().catch(() => null);
  if (!response.ok) throw parseError(response.status, data);
  return data as T;
}
