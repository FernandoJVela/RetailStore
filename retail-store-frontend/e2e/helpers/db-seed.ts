/**
 * Test data seeder for E2E tests.
 *
 * Call from beforeAll / beforeEach in Playwright tests to create predictable
 * test data. Each seeder function returns the IDs of created resources so
 * tests can reference them directly.
 *
 * The seeder uses the backend API directly (no UI) and requires a valid
 * Bearer token with sufficient permissions (Staff or Admin).
 */

import { apiClient, type LoginPayload } from './api-client';

const E2E_EMAIL    = process.env.E2E_EMAIL    ?? 'e2e-test@retailstore.local';
const E2E_PASSWORD = process.env.E2E_PASSWORD ?? 'E2ePassword1!';

// ── Token cache (one login per test run) ──────────────────────────────────────
let cachedToken: string | null = null;

export async function getE2EToken(): Promise<string> {
  if (cachedToken) return cachedToken;
  const session = await apiClient.login({ email: E2E_EMAIL, password: E2E_PASSWORD });
  cachedToken = session.accessToken;
  return cachedToken;
}

export function clearTokenCache() {
  cachedToken = null;
}

// ── Seed helpers ──────────────────────────────────────────────────────────────

export interface SeededProduct {
  productId: string;
  name: string;
  sku: string;
}

/**
 * Creates a product and its inventory entry.
 * Returns IDs for use in test assertions.
 */
export async function seedProductWithInventory(overrides?: {
  name?: string;
  sku?: string;
  price?: number;
  category?: string;
  initialQuantity?: number;
}): Promise<{ productId: string; name: string; sku: string }> {
  const token = await getE2EToken();

  const name     = overrides?.name     ?? `E2E Product ${Date.now()}`;
  const sku      = overrides?.sku      ?? `E2E-${Date.now()}`;
  const price    = overrides?.price    ?? 19.99;
  const category = overrides?.category ?? 'Electronics';

  const productId = await apiClient.createProduct(
    { name, sku, price, category },
    token
  );

  await apiClient.createInventory(
    { productId, initialQuantity: overrides?.initialQuantity ?? 50 },
    token
  );

  return { productId, name, sku };
}

export interface SeededCustomer {
  customerId: string;
  firstName: string;
  lastName: string;
  email: string;
}

/**
 * Creates a customer. Returns IDs for use in test assertions.
 */
export async function seedCustomer(overrides?: {
  firstName?: string;
  lastName?: string;
  email?: string;
}): Promise<SeededCustomer> {
  const token = await getE2EToken();

  const firstName = overrides?.firstName ?? 'E2E';
  const lastName  = overrides?.lastName  ?? 'Customer';
  const email     = overrides?.email     ?? `e2e-customer-${Date.now()}@test.com`;

  const customerId = await apiClient.createCustomer(
    { firstName, lastName, email },
    token
  );

  return { customerId, firstName, lastName, email };
}
