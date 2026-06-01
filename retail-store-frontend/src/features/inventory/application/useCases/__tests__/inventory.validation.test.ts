import {
  addStockSchema,
  adjustStockSchema,
  updateThresholdSchema,
  createInventorySchema,
} from '../inventory.validation';

// Helper: run schema validation, return first error message or null if valid
async function firstError(schema: { validate: (v: unknown, o?: object) => Promise<unknown> }, value: unknown): Promise<string | null> {
  try {
    await schema.validate(value, { abortEarly: true });
    return null;
  } catch (err: unknown) {
    return (err as { message: string }).message;
  }
}

// ── addStockSchema ────────────────────────────────────────────────────────────

describe('addStockSchema', () => {
  it('passes for a positive integer quantity', async () => {
    await expect(addStockSchema.validate({ quantity: 5 })).resolves.toBeDefined();
    await expect(addStockSchema.validate({ quantity: 100 })).resolves.toBeDefined();
  });

  it('fails when quantity is 0', async () => {
    const error = await firstError(addStockSchema, { quantity: 0 });
    expect(error).toMatch(/must be positive/i);
  });

  it('fails when quantity is negative', async () => {
    const error = await firstError(addStockSchema, { quantity: -1 });
    expect(error).toMatch(/must be positive/i);
  });

  it('fails when quantity is a decimal (non-integer)', async () => {
    const error = await firstError(addStockSchema, { quantity: 2.5 });
    expect(error).toMatch(/must be a whole number/i);
  });

  it('fails when quantity is missing', async () => {
    const error = await firstError(addStockSchema, {});
    expect(error).toMatch(/quantity is required/i);
  });
});

// ── adjustStockSchema ─────────────────────────────────────────────────────────

describe('adjustStockSchema', () => {
  it('passes for newQuantity = 0 with a valid reason', async () => {
    await expect(adjustStockSchema.validate({ newQuantity: 0, reason: 'write-off' })).resolves.toBeDefined();
  });

  it('passes for positive newQuantity with a valid reason', async () => {
    await expect(adjustStockSchema.validate({ newQuantity: 30, reason: 'physical count' })).resolves.toBeDefined();
  });

  it('fails when newQuantity is negative', async () => {
    const error = await firstError(adjustStockSchema, { newQuantity: -1, reason: 'test' });
    expect(error).toMatch(/cannot be negative/i);
  });

  it('fails when newQuantity is a decimal', async () => {
    const error = await firstError(adjustStockSchema, { newQuantity: 10.5, reason: 'test reason' });
    expect(error).toMatch(/must be a whole number/i);
  });

  it('fails when reason is too short (< 3 characters)', async () => {
    const error = await firstError(adjustStockSchema, { newQuantity: 10, reason: 'ab' });
    expect(error).toMatch(/at least 3 characters/i);
  });

  it('fails when reason is missing', async () => {
    const error = await firstError(adjustStockSchema, { newQuantity: 10 });
    expect(error).toMatch(/reason is required/i);
  });

  it('fails when newQuantity is missing', async () => {
    const error = await firstError(adjustStockSchema, { reason: 'valid reason' });
    expect(error).toMatch(/quantity is required/i);
  });
});

// ── updateThresholdSchema ─────────────────────────────────────────────────────

describe('updateThresholdSchema', () => {
  it('passes when newThreshold is 0 (disabled alerts)', async () => {
    await expect(updateThresholdSchema.validate({ newThreshold: 0 })).resolves.toBeDefined();
  });

  it('passes for positive integer threshold', async () => {
    await expect(updateThresholdSchema.validate({ newThreshold: 10 })).resolves.toBeDefined();
  });

  it('fails when newThreshold is negative', async () => {
    const error = await firstError(updateThresholdSchema, { newThreshold: -1 });
    expect(error).toMatch(/cannot be negative/i);
  });

  it('fails when newThreshold is a decimal', async () => {
    const error = await firstError(updateThresholdSchema, { newThreshold: 5.5 });
    expect(error).toMatch(/must be a whole number/i);
  });

  it('fails when newThreshold is missing', async () => {
    const error = await firstError(updateThresholdSchema, {});
    expect(error).toMatch(/threshold is required/i);
  });
});

// ── createInventorySchema ─────────────────────────────────────────────────────

describe('createInventorySchema', () => {
  it('passes with valid productId, initialQuantity 0, and default threshold', async () => {
    await expect(createInventorySchema.validate({
      productId: 'prod-1',
      initialQuantity: 0,
      reorderThreshold: 10,
    })).resolves.toBeDefined();
  });

  it('fails when productId is missing', async () => {
    const error = await firstError(createInventorySchema, { initialQuantity: 10, reorderThreshold: 5 });
    expect(error).toMatch(/product is required/i);
  });

  it('fails when initialQuantity is negative', async () => {
    const error = await firstError(createInventorySchema, {
      productId: 'prod-1', initialQuantity: -1, reorderThreshold: 5,
    });
    expect(error).toBeTruthy();
  });

  it('fails when initialQuantity is missing', async () => {
    const error = await firstError(createInventorySchema, { productId: 'prod-1', reorderThreshold: 5 });
    expect(error).toMatch(/initial quantity is required/i);
  });
});
