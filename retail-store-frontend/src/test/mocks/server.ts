import { setupServer } from 'msw/node';
import { handlers } from './handlers';

/**
 * MSW Node.js server — used in Vitest (server-side) integration tests.
 *
 * Lifecycle helpers for test files:
 *
 *   import { server } from '@/test/mocks/server';
 *
 *   beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }));
 *   afterEach(() => server.resetHandlers());   // undo per-test overrides
 *   afterAll(() => server.close());
 *
 * To override a handler for one test:
 *
 *   import { http, HttpResponse } from 'msw';
 *   server.use(
 *     http.get('/api/v1/products', () => HttpResponse.json([], { status: 200 }))
 *   );
 */
export const server = setupServer(...handlers);
