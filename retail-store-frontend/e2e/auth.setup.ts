import { test as setup } from '@playwright/test';
import { apiClient } from './helpers/api-client';

/**
 * Auth setup — runs once per test suite (before any chromium tests).
 * Registers a disposable E2E test user if needed, then logs in and
 * saves the browser storage state so all other tests start authenticated.
 *
 * The saved session is stored at e2e/.auth/session.json (gitignored).
 *
 * Environment variables (optional — defaults work for local dev):
 *   E2E_EMAIL     test user email  (default: e2e-test@retailstore.local)
 *   E2E_PASSWORD  test user password (default: E2ePassword1!)
 */

// Relative to the project root (where playwright.config.ts lives)
const SESSION_FILE = 'e2e/.auth/session.json';

const E2E_EMAIL    = process.env.E2E_EMAIL    ?? 'e2e-test@retailstore.local';
const E2E_PASSWORD = process.env.E2E_PASSWORD ?? 'E2ePassword1!';
const E2E_USERNAME = 'e2e_testuser';

setup('authenticate – register (if needed) then login', async ({ page }) => {
  // ── 1. Ensure the test user exists ─────────────────────────────────────
  // Try registering; a 409 means the user already exists — that's fine.
  const registered = await apiClient.tryRegisterUser({
    username: E2E_USERNAME,
    email: E2E_EMAIL,
    password: E2E_PASSWORD,
  });
  if (!registered && !(await apiClient.userExists(E2E_EMAIL))) {
    throw new Error(`Failed to register or locate E2E test user: ${E2E_EMAIL}`);
  }

  // ── 2. Log in via the UI ────────────────────────────────────────────────
  await page.goto('/login');

  // Use placeholder-based selectors — resilient to i18n label changes
  await page.locator('input[type="email"]').fill(E2E_EMAIL);
  await page.locator('input[type="password"]').fill(E2E_PASSWORD);
  await page.locator('button[type="submit"]').click();

  // Wait until navigation away from /login confirms success
  await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10_000 });

  // ── 3. Persist the authenticated browser state ──────────────────────────
  await page.context().storageState({ path: SESSION_FILE });
});
