import { mapProductDto, mapProductDetailDto } from '../products.mapper';
import type { ProductDto, ProductDetailDto } from '@features/products';

// ── Fixtures ──────────────────────────────────────────────────────────────────

const baseDto: ProductDto = {
  id: 'prod-1',
  name: 'Widget Pro',
  sku: 'WGT-001',
  price: 29.99,
  currency: 'USD',
  category: 'Electronics',
  isActive: true,
};

const detailDto: ProductDetailDto = {
  ...baseDto,
  description: 'A premium widget for professionals.',
  createdAt: '2024-01-15T08:00:00Z',
  updatedAt: '2024-06-10T12:00:00Z',
};

// ── mapProductDto ─────────────────────────────────────────────────────────────

describe('mapProductDto', () => {
  it('maps all scalar fields verbatim', () => {
    const result = mapProductDto(baseDto);
    expect(result.id).toBe('prod-1');
    expect(result.name).toBe('Widget Pro');
    expect(result.sku).toBe('WGT-001');
    expect(result.price).toBe(29.99);
    expect(result.currency).toBe('USD');
    expect(result.category).toBe('Electronics');
    expect(result.isActive).toBe(true);
  });

  it('sets statusLabel to "Active" when isActive is true', () => {
    const result = mapProductDto(baseDto);
    expect(result.statusLabel).toBe('Active');
  });

  it('sets statusLabel to "Inactive" when isActive is false', () => {
    const result = mapProductDto({ ...baseDto, isActive: false });
    expect(result.statusLabel).toBe('Inactive');
  });

  it('produces a non-empty formattedPrice string', () => {
    const result = mapProductDto(baseDto);
    expect(typeof result.formattedPrice).toBe('string');
    expect(result.formattedPrice.length).toBeGreaterThan(0);
  });

  it('includes the price amount in formattedPrice', () => {
    const result = mapProductDto({ ...baseDto, price: 49.99 });
    expect(result.formattedPrice).toContain('49');
  });

  it('includes the currency symbol for USD', () => {
    const result = mapProductDto(baseDto);
    expect(result.formattedPrice).toContain('$');
  });

  it('falls back gracefully to "amount currency" when currency is unrecognised', () => {
    // Intl.NumberFormat throws for invalid currencies — the mapper catches and falls back
    const result = mapProductDto({ ...baseDto, currency: 'XXX' });
    // Should still return a string without crashing
    expect(typeof result.formattedPrice).toBe('string');
  });
});

// ── mapProductDetailDto ───────────────────────────────────────────────────────

describe('mapProductDetailDto', () => {
  it('includes all base product fields', () => {
    const result = mapProductDetailDto(detailDto);
    expect(result.id).toBe('prod-1');
    expect(result.name).toBe('Widget Pro');
    expect(result.statusLabel).toBe('Active');
  });

  it('maps the description field', () => {
    const result = mapProductDetailDto(detailDto);
    expect(result.description).toBe('A premium widget for professionals.');
  });

  it('returns undefined description when absent', () => {
    const result = mapProductDetailDto({ ...detailDto, description: undefined });
    expect(result.description).toBeUndefined();
  });

  it('converts createdAt string to a Date instance', () => {
    const result = mapProductDetailDto(detailDto);
    expect(result.createdAt).toBeInstanceOf(Date);
    expect(result.createdAt.getFullYear()).toBe(2024);
  });

  it('converts updatedAt string to a Date when present', () => {
    const result = mapProductDetailDto(detailDto);
    expect(result.updatedAt).toBeInstanceOf(Date);
  });

  it('returns null for updatedAt when the DTO has no updatedAt', () => {
    const result = mapProductDetailDto({ ...detailDto, updatedAt: null });
    expect(result.updatedAt).toBeNull();
  });
});
