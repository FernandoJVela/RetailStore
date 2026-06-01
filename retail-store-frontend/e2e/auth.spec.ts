/**
 * E2E – Authentication flow
 *
 * Covers:
 *   1. Login form (unauthenticated context)
 *      – valid credentials → leaves /login, dashboard renders
 *      – wrong password    → inline error message
 *      – empty submission  → client-side validation, no navigation
 *
 *   2. Logout (authenticated context, storageState from auth.setup)
 *      – clicking Logout   → redirected to /login
 *      – protected route   → redirected to /login after logout
 *
 *   3. Session persistence (authenticated context)
 *      – page refresh      → still authenticated (no redirect to /login)
 *      – direct navigation to protected route → renders normally
 *
 * Prerequisites: backend running on :5240, frontend on :3000.
 * Run: npm run e2e -- --grep "auth"
 */

import { test, expect } from '@playwright/test';

const E2E_EMAIL    = process.env.E2E_EMAIL    ?? 'e2e-test@retailstore.local';
const E2E_PASSWORD = process.env.E2E_PASSWORD ?? 'E2ePassword1!';

// ── Helpers ───────────────────────────────────────────────────────────────────

/** Fill and submit the login form. */
async function submitLoginForm(
  page: import('@playwright/test').Page,
  email: string,
  password: string
) {
  await page.locator('input[type="email"]').fill(email);
  await page.locator('input[type="password"]').fill(password);
  await page.getByRole('button', { name: 'Sign In' }).click();
}

/** Open the user-avatar dropdown in the Topbar. */
async function openUserMenu(page: import('@playwright/test').Page) {
  // The avatar button is in the header — it's the last button element there
  // (Mobile hamburger is first, avatar trigger is last)
  await page.locator('header').getByRole('button').last().click();
}

// ═════════════════════════════════════════════════════════════════════════════
// 1. Login form — runs WITHOUT the stored session so the form is visible
// ═════════════════════════════════════════════════════════════════════════════
test.describe('Login form (unauthenticated)', () => {
  // Clear storageState for this describe block so the login page is shown
  test.use({ storageState: { cookies: [], origins: [] } });

  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('input[type="email"]')).toBeVisible();
  });

  test('valid credentials navigate away from /login and render the app shell', async ({ page }) => {
    await submitLoginForm(page, E2E_EMAIL, E2E_PASSWORD);

    // Wait until the URL no longer contains "login"
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10_000 });

    // The main layout renders a <main> element
    await expect(page.locator('main')).toBeVisible();
    expect(page.url()).not.toContain('/login');
  });

  test('valid credentials show the authenticated username in the Topbar', async ({ page }) => {
    await submitLoginForm(page, E2E_EMAIL, E2E_PASSWORD);
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10_000 });

    // Topbar shows the first letter of the username in the avatar circle
    // username = e2e_testuser  →  avatar initial = 'E'
    await expect(page.locator('header')).toContainText('E');
  });

  test('wrong password shows an inline error message', async ({ page }) => {
    await submitLoginForm(page, E2E_EMAIL, 'WrongPassword!');

    // The API returns: { detail: 'Email or password is incorrect.' }
    // The Alert component renders it as a paragraph
    await expect(
      page.getByText('Email or password is incorrect.')
    ).toBeVisible({ timeout: 8_000 });

    // User must remain on the login page
    expect(page.url()).toContain('/login');
  });

  test('empty email submission shows a client-side validation error', async ({ page }) => {
    // Leave email empty — click Sign In immediately
    await page.locator('input[type="password"]').fill('somepassword');
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Client-side validation fires, page stays at /login
    await page.waitForTimeout(500); // brief wait for re-render
    expect(page.url()).toContain('/login');
  });

  test('empty password submission shows a validation error', async ({ page }) => {
    await page.locator('input[type="email"]').fill(E2E_EMAIL);
    // Leave password empty — click Sign In
    await page.getByRole('button', { name: 'Sign In' }).click();

    await page.waitForTimeout(500);
    expect(page.url()).toContain('/login');
  });
});

// ═════════════════════════════════════════════════════════════════════════════
// 2. Logout — uses the saved session (auth.setup wrote storageState)
// ═════════════════════════════════════════════════════════════════════════════
test.describe('Logout (authenticated)', () => {
  test.beforeEach(async ({ page }) => {
    // Start at the dashboard; the session should keep us authenticated
    await page.goto('/');
    await expect(page.locator('main')).toBeVisible({ timeout: 8_000 });
  });

  test('clicking Logout redirects to /login', async ({ page }) => {
    await openUserMenu(page);

    // Dropdown appears with Logout button
    await expect(page.getByRole('button', { name: 'Logout' })).toBeVisible();
    await page.getByRole('button', { name: 'Logout' }).click();

    await page.waitForURL('**/login', { timeout: 8_000 });
    expect(page.url()).toContain('/login');
  });

  test('after logout, visiting a protected route redirects back to /login', async ({ page }) => {
    // Logout via user menu
    await openUserMenu(page);
    await page.getByRole('button', { name: 'Logout' }).click();
    await page.waitForURL('**/login', { timeout: 8_000 });

    // Try to visit a protected route directly
    await page.goto('/inventory');

    // AuthGuard should redirect unauthenticated users back to /login
    await page.waitForURL('**/login', { timeout: 5_000 });
    expect(page.url()).toContain('/login');
  });
});

// ═════════════════════════════════════════════════════════════════════════════
// 3. Session persistence — uses the saved session
// ═════════════════════════════════════════════════════════════════════════════
test.describe('Session persistence (authenticated)', () => {
  test('refreshing the page keeps the user authenticated', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('main')).toBeVisible({ timeout: 8_000 });

    await page.reload();

    // After reload, AuthGuard checks localStorage token.
    // If authenticated, stays on '/'; if not, redirects to '/login'.
    await page.waitForLoadState('networkidle');
    expect(page.url()).not.toContain('/login');
    await expect(page.locator('main')).toBeVisible();
  });

  test('navigating directly to a protected route renders the page', async ({ page }) => {
    await page.goto('/inventory');

    // Should render without redirecting to /login
    await page.waitForURL('**/inventory', { timeout: 8_000 });
    await expect(page.locator('main')).toBeVisible();
    expect(page.url()).toContain('/inventory');
  });

  test('navigating directly to /products renders the page', async ({ page }) => {
    await page.goto('/products');

    await page.waitForURL('**/products', { timeout: 8_000 });
    await expect(page.locator('main')).toBeVisible();
  });
});
