import { beforeAll, afterAll, afterEach, describe, it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '../server';
import { productsApi } from '@features/products/api/products.api';
import { ordersApi } from '@features/orders/api/orders.api';
import { inventoryApi } from '@features/inventory/api/inventory.api';
import { usersApi } from '@features/users/api/users.api';
import { mockProducts, mockOrders, mockInventoryItems, mockLoginResponse } from '../data/fixtures';

// ── Server lifecycle ──────────────────────────────────────────────────────────
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

// ── Smoke tests: happy-path handlers return fixture data ──────────────────────

describe('MSW server – handler smoke tests', () => {
  it('GET /api/v1/products returns fixture products', async () => {
    const response = await productsApi.getAll();
    expect(response.data).toHaveLength(mockProducts.length);
    expect(response.data[0].id).toBe(mockProducts[0].id);
    expect(response.data[0].name).toBe('Widget Pro');
  });

  it('GET /api/v1/orders returns fixture orders', async () => {
    const response = await ordersApi.getAll();
    expect(response.data).toHaveLength(mockOrders.length);
    expect(response.data[0].status).toBe('Draft');
  });

  it('GET /api/v1/inventory returns all inventory items', async () => {
    const response = await inventoryApi.getAll();
    expect(response.data).toHaveLength(mockInventoryItems.length);
  });

  it('POST /api/v1/users/login returns a JWT for valid credentials', async () => {
    const response = await usersApi.login({
      email: 'testuser@example.com',
      password: 'Password123!',
    });
    expect(response.data.accessToken).toBe(mockLoginResponse.accessToken);
    expect(response.data.userId).toBe(mockLoginResponse.userId);
  });

  it('POST /api/v1/users/login returns 401 for wrong credentials', async () => {
    await expect(
      usersApi.login({ email: 'testuser@example.com', password: 'wrong' })
    ).rejects.toMatchObject({ response: { status: 401 } });
  });

  // ── server.use() override for a single test ───────────────────────────────

  it('server.use() overrides the default handler for one test', async () => {
    server.use(
      http.get('/api/v1/products', () => HttpResponse.json([]))
    );

    const response = await productsApi.getAll();
    expect(response.data).toHaveLength(0); // overridden → empty list
  });

  it('default handler is restored after the override test', async () => {
    // server.resetHandlers() was called by afterEach — fixture data is back
    const response = await productsApi.getAll();
    expect(response.data).toHaveLength(mockProducts.length);
  });

  // ── 404 handlers ──────────────────────────────────────────────────────────

  it('GET /api/v1/products/:id returns 404 for unknown id', async () => {
    await expect(
      productsApi.getById('non-existent-id')
    ).rejects.toMatchObject({ response: { status: 404 } });
  });

  it('GET /api/v1/orders/:id returns 404 for unknown id', async () => {
    await expect(
      ordersApi.getById('non-existent-id')
    ).rejects.toMatchObject({ response: { status: 404 } });
  });

  // ── Filter / query-param forwarding ──────────────────────────────────────

  it('GET /api/v1/inventory?stockStatus=LowStock filters by status', async () => {
    const response = await inventoryApi.getAll({ stockStatus: 'LowStock' });
    expect(response.data.every((i) => i.stockStatus === 'LowStock')).toBe(true);
  });

  it('POST /api/v1/products returns 201', async () => {
    const response = await productsApi.create({
      name: 'New Widget',
      sku: 'NW-001',
      price: 9.99,
      currency: 'USD',
      category: 'General',
    });
    expect(response.status).toBe(201);
    expect(typeof response.data).toBe('string'); // new ID
  });
});
