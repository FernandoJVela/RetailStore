import { mapInventoryItemDto, mapInventoryDetailDto } from '../inventory.mapper';
import type { InventoryItemDto, InventoryDetailDto } from '@features/inventory';

// ── Fixtures ──────────────────────────────────────────────────────────────────

function makeDto(overrides: Partial<InventoryItemDto> = {}): InventoryItemDto {
  return {
    id: 'inv-1',
    productId: 'prod-1',
    productName: 'Widget Pro',
    sku: 'WGT-001',
    quantityOnHand: 50,
    reservedQuantity: 5,
    availableQuantity: 45,
    reorderThreshold: 10,
    stockStatus: 'InStock',
    ...overrides,
  };
}

// ── mapInventoryItemDto ───────────────────────────────────────────────────────

describe('mapInventoryItemDto', () => {
  it('maps all base fields verbatim', () => {
    const result = mapInventoryItemDto(makeDto());
    expect(result.id).toBe('inv-1');
    expect(result.productId).toBe('prod-1');
    expect(result.productName).toBe('Widget Pro');
    expect(result.sku).toBe('WGT-001');
    expect(result.quantityOnHand).toBe(50);
    expect(result.reservedQuantity).toBe(5);
    expect(result.availableQuantity).toBe(45);
    expect(result.reorderThreshold).toBe(10);
  });

  // ── Stock status flags ────────────────────────────────────────────────────

  it('sets isLowStock=false and isOutOfStock=false for InStock', () => {
    const result = mapInventoryItemDto(makeDto({ stockStatus: 'InStock' }));
    expect(result.isLowStock).toBe(false);
    expect(result.isOutOfStock).toBe(false);
    expect(result.stockStatus).toBe('InStock');
  });

  it('sets isLowStock=true and isOutOfStock=false for LowStock', () => {
    const result = mapInventoryItemDto(makeDto({ stockStatus: 'LowStock' }));
    expect(result.isLowStock).toBe(true);
    expect(result.isOutOfStock).toBe(false);
    expect(result.stockStatus).toBe('LowStock');
  });

  it('sets isOutOfStock=true and isLowStock=false for OutOfStock', () => {
    const result = mapInventoryItemDto(makeDto({ stockStatus: 'OutOfStock' }));
    expect(result.isOutOfStock).toBe(true);
    expect(result.isLowStock).toBe(false);
    expect(result.stockStatus).toBe('OutOfStock');
  });

  it('defaults unrecognised status to InStock', () => {
    const result = mapInventoryItemDto(makeDto({ stockStatus: 'Unknown' as 'InStock' }));
    expect(result.stockStatus).toBe('InStock');
  });

  // ── stockHealthPercent ────────────────────────────────────────────────────

  it('returns 0% health when availableQuantity is 0', () => {
    const result = mapInventoryItemDto(makeDto({ availableQuantity: 0, reorderThreshold: 10 }));
    expect(result.stockHealthPercent).toBe(0);
  });

  it('returns 100% health when availableQuantity is >= 3× threshold', () => {
    // 3× threshold = 30; available = 30 → 100%
    const result = mapInventoryItemDto(makeDto({ availableQuantity: 30, reorderThreshold: 10 }));
    expect(result.stockHealthPercent).toBe(100);
  });

  it('is capped at 100% even when available is much higher than 3× threshold', () => {
    const result = mapInventoryItemDto(makeDto({ availableQuantity: 1000, reorderThreshold: 10 }));
    expect(result.stockHealthPercent).toBe(100);
  });

  it('returns a proportional percent between 0 and 100 for moderate stock', () => {
    // available = 15, threshold = 10 → ratio = 15/30 = 50%
    const result = mapInventoryItemDto(makeDto({ availableQuantity: 15, reorderThreshold: 10 }));
    expect(result.stockHealthPercent).toBe(50);
  });

  it('returns 100% health when threshold is 0 and items are available', () => {
    const result = mapInventoryItemDto(makeDto({ availableQuantity: 5, reorderThreshold: 0 }));
    expect(result.stockHealthPercent).toBe(100);
  });

  it('returns 0% health when threshold is 0 and no items are available', () => {
    const result = mapInventoryItemDto(makeDto({ availableQuantity: 0, reorderThreshold: 0 }));
    expect(result.stockHealthPercent).toBe(0);
  });
});

// ── mapInventoryDetailDto ─────────────────────────────────────────────────────

describe('mapInventoryDetailDto', () => {
  const detailDto: InventoryDetailDto = {
    ...makeDto(),
    createdAt: '2024-01-10T09:00:00Z',
    updatedAt: '2024-06-15T14:30:00Z',
  };

  it('includes all base InventoryItem fields', () => {
    const result = mapInventoryDetailDto(detailDto);
    expect(result.id).toBe('inv-1');
    expect(result.stockStatus).toBe('InStock');
    expect(result.stockHealthPercent).toBeGreaterThanOrEqual(0);
  });

  it('converts createdAt string to a Date instance', () => {
    const result = mapInventoryDetailDto(detailDto);
    expect(result.createdAt).toBeInstanceOf(Date);
    expect(result.createdAt.getFullYear()).toBe(2024);
  });

  it('converts updatedAt string to a Date when present', () => {
    const result = mapInventoryDetailDto(detailDto);
    expect(result.updatedAt).toBeInstanceOf(Date);
  });

  it('returns null for updatedAt when the DTO has no updatedAt', () => {
    const result = mapInventoryDetailDto({ ...detailDto, updatedAt: null });
    expect(result.updatedAt).toBeNull();
  });
});
