import { test as base, expect, type Page } from '@playwright/test';
import { apiClient } from '../helpers/api-client';
import { getE2EToken } from '../helpers/db-seed';

/**
 * Extended Playwright test with convenience fixtures for authenticated flows.
 *
 * Usage:
 *   import { test, expect } from '../fixtures/authenticated-page';
 *
 *   test('navigates to products', async ({ page, token }) => {
 *     await page.goto('/products');
 *     expect(token).toBeTruthy();
 *   });
 */

interface E2EFixtures {
  /** The auth token for the E2E test user — use for direct API calls in tests. */
  token: string;

  /** Navigates to a route and waits for the main layout to be visible. */
  navigateTo: (path: string) => Promise<void>;
}

export const test = base.extend<E2EFixtures>({
  token: async ({}, use) => {
    const t = await getE2EToken();
    await use(t);
  },

  navigateTo: async ({ page }, use) => {
    await use(async (path: string) => {
      await page.goto(path);
      // Wait for the app shell to render (sidebar or main content area)
      await page.waitForSelector('main, [role="main"], nav', { timeout: 8_000 });
    });
  },
});

export { expect } from '@playwright/test';
