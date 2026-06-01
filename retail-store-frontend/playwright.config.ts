import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright E2E configuration.
 *
 * Prerequisites before running:
 *   1. Backend (RetailStore.Api) must be running on http://localhost:5240
 *   2. Set E2E_EMAIL / E2E_PASSWORD env vars, or rely on defaults (see auth.setup.ts)
 *
 * Run with:
 *   npm run e2e           – headless, single run
 *   npm run e2e:ui        – interactive Playwright UI mode
 *   npm run e2e:debug     – headed, pause on first failure
 */

const BASE_URL = process.env.BASE_URL ?? 'http://localhost:3000';

export default defineConfig({
  testDir: './e2e',

  // Fail fast in CI; retry once locally on flaky network
  retries: process.env.CI ? 2 : 0,
  fullyParallel: false,   // sequential: E2E tests share DB state
  workers: 1,

  reporter: [
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ['line'],
  ],

  use: {
    baseURL: BASE_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    // ── 1. Auth setup: logs in once and saves storage state ────────────────
    {
      name: 'setup',
      testMatch: /auth\.setup\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },

    // ── 2. Chromium: all tests run with the authenticated session ──────────
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        // Reuse the auth state created by the setup project
        storageState: 'e2e/.auth/session.json',
      },
      dependencies: ['setup'],
      testIgnore: /auth\.setup\.ts/,
    },
  ],

  // Start the Vite dev server automatically before tests.
  // The backend must be running separately on port 5240.
  webServer: {
    command: 'npm run dev',
    url: BASE_URL,
    reuseExistingServer: true,   // reuse if already running (dev workflow)
    timeout: 30_000,
  },
});
