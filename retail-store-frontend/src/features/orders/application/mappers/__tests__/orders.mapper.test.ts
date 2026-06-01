import { mapOrderDto, mapOrderDetailDto } from '../orders.mapper';
import type { OrderDto, OrderDetailDto } from '@features/orders';

// ── Fixtures ──────────────────────────────────────────────────────────────────

const baseDto: OrderDto = {
  id: 'ord-1',
  customerId: 'cust-1',
  status: 'Confirmed',
  orderDate: '2024-06-15T10:00:00Z',
  totalAmount: 149.99,
  itemCount: 3,
  completedAt: null,
  cancelledAt: null,
};

const detailDto: OrderDetailDto = {
  ...baseDto,
  items: [
    { id: 'item-1', productId: 'p-1', quantity: 2, unitPrice: 49.99, currency: 'USD', subtotal: 99.98 },
    { id: 'item-2', productId: 'p-2', quantity: 1, unitPrice: 50.01, currency: 'USD', subtotal: 50.01 },
  ],
};

// ── mapOrderDto ───────────────────────────────────────────────────────────────

describe('mapOrderDto', () => {
  it('maps id, customerId, and status verbatim', () => {
    const result = mapOrderDto(baseDto);
    expect(result.id).toBe('ord-1');
    expect(result.customerId).toBe('cust-1');
    expect(result.status).toBe('Confirmed');
  });

  it('converts orderDate string to a Date instance', () => {
    const result = mapOrderDto(baseDto);
    expect(result.orderDate).toBeInstanceOf(Date);
    expect(result.orderDate.getFullYear()).toBe(2024);
  });

  it('maps totalAmount and itemCount correctly', () => {
    const result = mapOrderDto(baseDto);
    expect(result.totalAmount).toBe(149.99);
    expect(result.itemCount).toBe(3);
  });

  it('returns null for completedAt and cancelledAt when absent', () => {
    const result = mapOrderDto(baseDto);
    expect(result.completedAt).toBeNull();
    expect(result.cancelledAt).toBeNull();
  });

  it('converts completedAt to a Date when present', () => {
    const dto = { ...baseDto, completedAt: '2024-06-20T09:00:00Z' };
    const result = mapOrderDto(dto);
    expect(result.completedAt).toBeInstanceOf(Date);
  });

  it('converts cancelledAt to a Date when present', () => {
    const dto = { ...baseDto, cancelledAt: '2024-06-18T15:00:00Z' };
    const result = mapOrderDto(dto);
    expect(result.cancelledAt).toBeInstanceOf(Date);
  });

  it('produces a non-empty formattedTotal string', () => {
    const result = mapOrderDto(baseDto);
    expect(typeof result.formattedTotal).toBe('string');
    expect(result.formattedTotal).toContain('149');
  });

  it('includes the currency symbol in formattedTotal for USD', () => {
    const result = mapOrderDto({ ...baseDto, totalAmount: 29.99 });
    // USD formatting → '$29.99'
    expect(result.formattedTotal).toContain('$');
  });
});

// ── mapOrderDetailDto ─────────────────────────────────────────────────────────

describe('mapOrderDetailDto', () => {
  it('includes all base fields from mapOrderDto', () => {
    const result = mapOrderDetailDto(detailDto);
    expect(result.id).toBe('ord-1');
    expect(result.status).toBe('Confirmed');
    expect(result.totalAmount).toBe(149.99);
  });

  it('maps items array with correct length', () => {
    const result = mapOrderDetailDto(detailDto);
    expect(result.items).toHaveLength(2);
  });

  it('derives itemCount from items.length', () => {
    const result = mapOrderDetailDto(detailDto);
    expect(result.itemCount).toBe(2);
  });

  it('maps each item with productId, quantity, unitPrice, subtotal', () => {
    const result = mapOrderDetailDto(detailDto);
    const item = result.items[0];
    expect(item.productId).toBe('p-1');
    expect(item.quantity).toBe(2);
    expect(item.unitPrice).toBe(49.99);
    expect(item.subtotal).toBe(99.98);
  });

  it('computes formattedPrice for each item', () => {
    const result = mapOrderDetailDto(detailDto);
    expect(result.items[0].formattedPrice).toContain('49');
  });

  it('computes formattedSubtotal for each item', () => {
    const result = mapOrderDetailDto(detailDto);
    expect(result.items[0].formattedSubtotal).toContain('99');
  });

  it('returns empty items array when items is undefined', () => {
    const dto = { ...detailDto, items: undefined as unknown as typeof detailDto.items };
    const result = mapOrderDetailDto(dto);
    expect(result.items).toEqual([]);
    expect(result.itemCount).toBe(0);
  });
});
