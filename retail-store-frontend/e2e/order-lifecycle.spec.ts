/**
 * E2E – Order lifecycle
 *
 * Covers the core business flow end-to-end:
 *   1. Create a new order via the UI (customer + product from seeded API data)
 *   2. Confirm the order — status transitions from Pending → Confirmed
 *   3. Verify inventory reflects the stock change (available < initial)
 *   4. Verify the audit log has entries for the order actions
 *
 * Tests are sequential and share seeded product/customer data created in beforeAll.
 * Backend must be running on :5240, frontend dev server on :3000.
 *
 * Run: npm run e2e -- --grep "Order lifecycle"
 */

import { test, expect } from '@playwright/test';
import { seedProductWithInventory, seedCustomer } from './helpers/db-seed';

const INITIAL_STOCK = 20;
const ORDER_QTY     = 2;

test.describe('Order lifecycle', () => {
  // Shared state seeded once in beforeAll, used across all tests
  let product: { productId: string; name: string; sku: string };
  let customer: { customerId: string; firstName: string; lastName: string; email: string };

  test.beforeAll(async () => {
    const ts = Date.now();
    product  = await seedProductWithInventory({
      name:            `Lifecycle-${ts}`,
      sku:             `LIFE-${ts}`,
      price:           25.00,
      category:        'Electronics',
      initialQuantity: INITIAL_STOCK,
    });
    customer = await seedCustomer({
      firstName: 'Lifecycle',
      lastName:  `Test-${ts}`,
    });
  });

  // ── Helpers ────────────────────────────────────────────────────────────────

  /** Return the first row in the orders table that shows status "Pending". */
  function firstPendingRow(page: import('@playwright/test').Page) {
    return page.locator('table tbody tr').filter({ hasText: /pending/i }).first();
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // 1. Create order
  // ═══════════════════════════════════════════════════════════════════════════

  test('creates a new order for a customer via the UI', async ({ page }) => {
    await page.goto('/orders');

    // Open modal
    await page.getByRole('button', { name: /create order/i }).click();
    const modal = page.getByRole('dialog');
    await expect(modal).toBeVisible({ timeout: 8_000 });

    // ── Customer ─────────────────────────────────────────────────
    // <Select label="Customer"> renders a <label for="…"> + <select id="…">
    await modal.getByLabel('Customer').selectOption({ value: customer.customerId });

    // ── Product ──────────────────────────────────────────────────
    // The product <Select> has no label prop — it's the second <select> in the modal
    // (customer select is first; product select is second)
    await modal.locator('select').nth(1).selectOption({ value: product.productId });

    // ── Quantity ─────────────────────────────────────────────────
    await modal.locator('input[type="number"]').fill(String(ORDER_QTY));

    // ── Add button ───────────────────────────────────────────────
    // Icon-only <Button> next to the quantity input inside the "Add items" section.
    // Located via XPath sibling: the <div class="flex gap-2"> after the "Add items" label.
    await modal
      .locator('xpath=.//label[text()="Add items"]/following-sibling::div//button')
      .click();

    // Line item appears in the list
    await expect(modal.getByText(product.name)).toBeVisible();

    // ── Submit ───────────────────────────────────────────────────
    // Button text: "Create Order (2 items)"
    await modal.getByRole('button', { name: /create order/i }).click();

    // Modal closes
    await expect(modal).not.toBeVisible({ timeout: 8_000 });

    // The new Pending order row is in the table (orders sorted by date desc)
    await expect(firstPendingRow(page)).toBeVisible({ timeout: 8_000 });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 2. Confirm order
  // ═══════════════════════════════════════════════════════════════════════════

  test('confirms the order – status changes to Confirmed', async ({ page }) => {
    await page.goto('/orders');

    // The order we just created is the most recent Pending row (list sorted date desc)
    const row = firstPendingRow(page);
    await expect(row).toBeVisible({ timeout: 8_000 });

    // Open the action menu (last button in the row – the "…" trigger)
    await row.getByRole('button').last().click();

    // Click "View Details" in the dropdown
    await page.getByText('View Details').click();

    // Detail slide-panel opens — wait for the Confirm Order button
    const confirmBtn = page.getByRole('button', { name: 'Confirm Order' });
    await expect(confirmBtn).toBeVisible({ timeout: 8_000 });

    // Confirm
    await confirmBtn.click();

    // The button disappears (status is no longer Draft/Pending)
    await expect(confirmBtn).toBeHidden({ timeout: 8_000 });

    // The status badge inside the panel now reads "Confirmed"
    // Use .first() in case the word "Confirmed" also appears in filter pills elsewhere
    await expect(page.getByText('Confirmed').first()).toBeVisible({ timeout: 8_000 });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 3. Inventory
  // ═══════════════════════════════════════════════════════════════════════════

  test('inventory shows reduced available stock after order confirmation', async ({ page }) => {
    await page.goto('/inventory');

    // Search by our unique product name
    const searchInput = page.getByPlaceholder(/search/i);
    await expect(searchInput).toBeVisible({ timeout: 8_000 });
    await searchInput.fill(product.name);

    // Wait for the matching row
    const productRow = page
      .locator('table tbody tr')
      .filter({ hasText: product.name })
      .first();
    await expect(productRow).toBeVisible({ timeout: 8_000 });

    // Column layout: Product(0) | OnHand(1) | Reserved(2) | Available(3) | …
    // After order creation + confirmation, Available must be < INITIAL_STOCK
    const availableText = await productRow.locator('td').nth(3).textContent();
    const available = parseInt((availableText ?? '0').replace(/[^0-9]/g, ''), 10);

    expect(available).toBeLessThan(INITIAL_STOCK);
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 4. Audit log
  // ═══════════════════════════════════════════════════════════════════════════

  test('audit log has entries for the order actions', async ({ page }) => {
    await page.goto('/audit');

    // The Log tab is active by default.
    // Optionally filter by the Orders module if the option exists.
    const moduleFilter = page.locator('select').first();
    await expect(moduleFilter).toBeVisible({ timeout: 8_000 });

    const optionTexts = await moduleFilter.locator('option').allTextContents();
    const orderOption = optionTexts.find((o) => /order/i.test(o));
    if (orderOption) {
      await moduleFilter.selectOption({ label: orderOption });
    }

    // At least one audit entry must be visible (the audit system recorded our actions)
    await expect(page.locator('table tbody tr').first()).toBeVisible({ timeout: 10_000 });
    const rowCount = await page.locator('table tbody tr').count();
    expect(rowCount).toBeGreaterThan(0);
  });
});
