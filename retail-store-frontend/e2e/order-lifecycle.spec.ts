/**
 * E2E – Order lifecycle
 *
 * Covers the core business flow end-to-end:
 *   1. Create a new order via the UI (customer + product from seeded API data)
 *   2. Confirm the order — status transitions from Draft → Confirmed
 *   3. Verify inventory reflects the reserved stock after confirmation
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

  /** Return the first row in the orders table that shows status "Draft". */
  function firstDraftRow(page: import('@playwright/test').Page) {
    return page.locator('table tbody tr').filter({ hasText: /draft/i }).first();
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
    await modal.getByLabel('Customer').selectOption({ value: customer.customerId });

    // ── Product ──────────────────────────────────────────────────
    await modal.locator('select').nth(1).selectOption({ value: product.productId });

    // ── Quantity ─────────────────────────────────────────────────
    await modal.locator('input[type="number"]').fill(String(ORDER_QTY));

    // ── Add button ───────────────────────────────────────────────
    await modal
      .locator('xpath=.//label[text()="Add items"]/following-sibling::div//button')
      .click();

    // Line item appears in the list
    await expect(modal.getByText(product.name)).toBeVisible();

    // ── Submit ───────────────────────────────────────────────────
    await modal.getByRole('button', { name: /create order/i }).click();

    // Modal closes
    await expect(modal).not.toBeVisible({ timeout: 8_000 });

    // The new Draft order row is in the table
    await expect(firstDraftRow(page)).toBeVisible({ timeout: 8_000 });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 2. Confirm order
  // ═══════════════════════════════════════════════════════════════════════════

  test('confirms the order – status changes to Confirmed', async ({ page }) => {
    await page.goto('/orders');

    // The order we just created is the most recent Draft row
    const row = firstDraftRow(page);
    await expect(row).toBeVisible({ timeout: 8_000 });

    // Open the action menu (last button in the row)
    await row.getByRole('button').last().click();

    // Click "View Details"
    await page.getByText('View Details').click();

    // Detail slide-panel opens — wait for the Confirm Order button
    const confirmBtn = page.getByRole('button', { name: 'Confirm Order' });
    await expect(confirmBtn).toBeVisible({ timeout: 8_000 });

    // Confirm
    await confirmBtn.click();

    // Button disappears (status is no longer Draft/Pending)
    await expect(confirmBtn).toBeHidden({ timeout: 8_000 });

    // Status badge inside the panel now reads "Confirmed"
    await expect(page.getByText('Confirmed').first()).toBeVisible({ timeout: 8_000 });
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 3. Inventory — stock reserved after confirmation
  // ═══════════════════════════════════════════════════════════════════════════

  test('inventory shows reserved stock after order confirmation', async ({ page }) => {
    await page.goto('/inventory');

    const searchInput = page.getByPlaceholder(/search/i);
    await expect(searchInput).toBeVisible({ timeout: 8_000 });
    await searchInput.fill(product.name);

    const productRow = page
      .locator('table tbody tr')
      .filter({ hasText: product.name })
      .first();
    await expect(productRow).toBeVisible({ timeout: 8_000 });

    // Column layout: Product(0) | OnHand(1) | Reserved(2) | Available(3) | …
    // After order confirmation, Reserved > 0 and Available < INITIAL_STOCK
    const reservedText  = await productRow.locator('td').nth(2).textContent();
    const availableText = await productRow.locator('td').nth(3).textContent();

    const reserved  = parseInt((reservedText  ?? '0').replace(/[^0-9]/g, ''), 10);
    const available = parseInt((availableText ?? '0').replace(/[^0-9]/g, ''), 10);

    expect(reserved).toBeGreaterThan(0);
    expect(available).toBeLessThan(INITIAL_STOCK);
    expect(available).toBe(INITIAL_STOCK - reserved);
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 4. Audit log
  // ═══════════════════════════════════════════════════════════════════════════

  test('audit log has entries for the order actions', async ({ page }) => {
    await page.goto('/audit');

    const moduleFilter = page.locator('select').first();
    await expect(moduleFilter).toBeVisible({ timeout: 8_000 });

    const optionTexts = await moduleFilter.locator('option').allTextContents();
    const orderOption = optionTexts.find((o) => /order/i.test(o));
    if (orderOption) {
      await moduleFilter.selectOption({ label: orderOption });
    }

    await expect(page.locator('table tbody tr').first()).toBeVisible({ timeout: 10_000 });
    const rowCount = await page.locator('table tbody tr').count();
    expect(rowCount).toBeGreaterThan(0);
  });

  // ═══════════════════════════════════════════════════════════════════════════
  // 5. Payments — create payment for confirmed order
  // ═══════════════════════════════════════════════════════════════════════════

  test('can create a payment for the confirmed order from the payments page', async ({ page }) => {
    await page.goto('/payments');

    // Click "Create Payment"
    await page.getByRole('button', { name: /create payment/i }).click();
    const modal = page.getByRole('dialog');
    await expect(modal).toBeVisible({ timeout: 8_000 });

    // Select the confirmed order (our product name is unique — find the right order)
    const orderSelect = modal.locator('select').first();
    await expect(orderSelect).toBeVisible({ timeout: 5_000 });

    // Pick any confirmed order (there's at least one from previous test steps)
    const options = await orderSelect.locator('option').allTextContents();
    const nonEmpty = options.filter((o) => o.trim() && !o.toLowerCase().includes('select'));
    expect(nonEmpty.length).toBeGreaterThan(0);

    await orderSelect.selectOption({ index: 1 }); // first real order

    // Select Credit Card method
    await modal.locator('select').nth(1).selectOption({ value: 'CreditCard' });

    // Submit
    await modal.getByRole('button', { name: /create payment/i }).click();

    // Modal closes on success
    await expect(modal).not.toBeVisible({ timeout: 8_000 });

    // A new Pending payment row appears in the table
    const pendingRow = page.locator('table tbody tr').filter({ hasText: /pending/i }).first();
    await expect(pendingRow).toBeVisible({ timeout: 8_000 });
  });
});
