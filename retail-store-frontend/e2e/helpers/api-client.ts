/**
 * Lightweight HTTP client for the RetailStore backend API.
 * Used by E2E tests to seed and clean up test data without going through the UI.
 *
 * All requests target the backend directly (port 5240), bypassing the Vite proxy,
 * so they work even before the frontend dev server is up.
 */

const API_BASE = process.env.API_URL ?? 'http://localhost:5240/api/v1';

// ── Auth helpers ──────────────────────────────────────────────────────────────

export interface RegisterPayload {
  username: string;
  email: string;
  password: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface AuthToken {
  accessToken: string;
  refreshToken: string;
  userId: string;
  username: string;
}

async function post<T>(path: string, body: unknown, token?: string): Promise<{ ok: boolean; status: number; data: T | null }> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(body),
  });

  let data: T | null = null;
  try {
    data = await res.json() as T;
  } catch { /* no body */ }

  return { ok: res.ok, status: res.status, data };
}

async function get<T>(path: string, token?: string): Promise<T | null> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) return null;
  return res.json() as Promise<T>;
}

// ── Public helpers used by auth.setup.ts and db-seed.ts ──────────────────────

export const apiClient = {
  /**
   * Attempts to register a user. Returns true on success (201),
   * false on conflict (409 — user already exists), throws on other errors.
   */
  async tryRegisterUser(payload: RegisterPayload): Promise<boolean> {
    const result = await post<string>('/users/register', payload);
    if (result.status === 201) return true;
    if (result.status === 409) return false; // already exists
    throw new Error(`Registration failed with status ${result.status}`);
  },

  /** Logs in and returns the auth token pair. */
  async login(payload: LoginPayload): Promise<AuthToken> {
    const result = await post<AuthToken>('/users/login', payload);
    if (!result.ok || !result.data)
      throw new Error(`Login failed with status ${result.status}`);
    return result.data;
  },

  /** Returns true if a user with the given email exists (login succeeds). */
  async userExists(email: string): Promise<boolean> {
    const result = await post<AuthToken>('/users/login', { email, password: 'any' });
    return result.status !== 404; // 401 = wrong password but user exists
  },

  /** Creates a product and returns the new product ID. Requires a Bearer token. */
  async createProduct(payload: {
    name: string; sku: string; price: number;
    currency?: string; category: string; description?: string;
  }, token: string): Promise<string> {
    const result = await post<string>('/products', { currency: 'USD', ...payload }, token);
    if (!result.ok || !result.data)
      throw new Error(`Product creation failed with status ${result.status}`);
    return result.data;
  },

  /** Creates an inventory record for a product. Requires a Bearer token. */
  async createInventory(payload: {
    productId: string; initialQuantity: number; reorderThreshold?: number;
  }, token: string): Promise<string> {
    const result = await post<string>('/inventory', { reorderThreshold: 10, ...payload }, token);
    if (!result.ok || !result.data)
      throw new Error(`Inventory creation failed with status ${result.status}`);
    return result.data;
  },

  /** Registers a customer and returns the new customer ID. Requires a Bearer token. */
  async createCustomer(payload: {
    firstName: string; lastName: string; email: string; phone?: string;
  }, token: string): Promise<string> {
    const result = await post<string>('/customers', payload, token);
    if (!result.ok || !result.data)
      throw new Error(`Customer creation failed with status ${result.status}`);
    return result.data;
  },

  /** Returns users list — useful for verifying state in tests. */
  async getUsers(token: string) {
    return get<unknown[]>('/users', token);
  },
};
