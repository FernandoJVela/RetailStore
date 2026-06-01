import { createOrderSchema, addItemSchema, cancelReasonSchema } from '../orders.validation';

// Helper: run schema validation and return the first error message (or null if valid)
async function firstError(schema: { validate: (v: unknown, o?: object) => Promise<unknown> }, value: unknown): Promise<string | null> {
  try {
    await schema.validate(value, { abortEarly: true });
    return null;
  } catch (err: unknown) {
    return (err as { message: string }).message;
  }
}

// ── createOrderSchema ─────────────────────────────────────────────────────────

describe('createOrderSchema', () => {
  it('passes when customerId is a non-empty string', async () => {
    await expect(createOrderSchema.validate({ customerId: 'cust-1' })).resolves.toBeDefined();
  });

  it('fails when customerId is missing', async () => {
    const error = await firstError(createOrderSchema, { customerId: undefined });
    expect(error).toBeTruthy();
  });

  it('fails when customerId is an empty string', async () => {
    const error = await firstError(createOrderSchema, { customerId: '' });
    expect(error).toMatch(/customer is required/i);
  });
});

// ── addItemSchema ─────────────────────────────────────────────────────────────

describe('addItemSchema', () => {
  it('passes with valid productId and quantity >= 1', async () => {
    await expect(addItemSchema.validate({ productId: 'p-1', quantity: 1 })).resolves.toBeDefined();
    await expect(addItemSchema.validate({ productId: 'p-1', quantity: 100 })).resolves.toBeDefined();
  });

  it('fails when productId is missing', async () => {
    const error = await firstError(addItemSchema, { quantity: 1 });
    expect(error).toMatch(/product is required/i);
  });

  it('fails when quantity is 0', async () => {
    const error = await firstError(addItemSchema, { productId: 'p-1', quantity: 0 });
    expect(error).toMatch(/at least 1/i);
  });

  it('fails when quantity is negative', async () => {
    const error = await firstError(addItemSchema, { productId: 'p-1', quantity: -5 });
    expect(error).toMatch(/at least 1/i);
  });

  it('fails when quantity is missing', async () => {
    const error = await firstError(addItemSchema, { productId: 'p-1', quantity: undefined });
    expect(error).toMatch(/quantity is required/i);
  });

  it('fails when quantity is a decimal (non-integer)', async () => {
    const error = await firstError(addItemSchema, { productId: 'p-1', quantity: 1.5 });
    expect(error).toBeTruthy();
  });
});

// ── cancelReasonSchema ────────────────────────────────────────────────────────

describe('cancelReasonSchema', () => {
  it('passes when reason has 3 or more characters', async () => {
    await expect(cancelReasonSchema.validate({ reason: 'abc' })).resolves.toBeDefined();
    await expect(cancelReasonSchema.validate({ reason: 'customer no longer needs the order' })).resolves.toBeDefined();
  });

  it('fails when reason is too short (< 3 chars)', async () => {
    const error = await firstError(cancelReasonSchema, { reason: 'ab' });
    expect(error).toMatch(/at least 3 characters/i);
  });

  it('fails when reason is empty', async () => {
    const error = await firstError(cancelReasonSchema, { reason: '' });
    expect(error).toBeTruthy();
  });

  it('fails when reason is missing', async () => {
    const error = await firstError(cancelReasonSchema, {});
    expect(error).toMatch(/reason is required/i);
  });

  it('fails when reason exceeds 500 characters', async () => {
    const error = await firstError(cancelReasonSchema, { reason: 'a'.repeat(501) });
    expect(error).toBeTruthy();
  });

  it('passes at exactly 500 characters', async () => {
    await expect(cancelReasonSchema.validate({ reason: 'a'.repeat(500) })).resolves.toBeDefined();
  });
});
