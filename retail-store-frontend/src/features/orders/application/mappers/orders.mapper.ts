import type { OrderDto, OrderDetailDto, OrderItemDto } from '@features/orders';
import type { Order, OrderDetail, OrderItem, OrderStatus } from '@features/orders';
 
function fmt(amount: number, currency = 'USD'): string {
  try { return new Intl.NumberFormat('en-US', { style: 'currency', currency, minimumFractionDigits: 2 }).format(amount); }
  catch { return `${amount.toFixed(2)} ${currency}`; }
}
 
export function mapOrderDto(dto: OrderDto): Order {
  return {
    id: dto.id,
    customerId: dto.customerId,
    status: dto.status as OrderStatus,
    orderDate: new Date(dto.orderDate),
    totalAmount: dto.totalAmount,
    formattedTotal: fmt(dto.totalAmount),
    itemCount: dto.itemCount,
    completedAt: dto.completedAt ? new Date(dto.completedAt) : null,
    cancelledAt: dto.cancelledAt ? new Date(dto.cancelledAt) : null,
  };
}
 
function mapItemDto(dto: OrderItemDto): OrderItem {
  return {
    id: dto.id,
    productId: dto.productId,
    quantity: dto.quantity,
    unitPrice: dto.unitPrice,
    currency: dto.currency,
    subtotal: dto.subtotal,
    formattedPrice: fmt(dto.unitPrice, dto.currency),
    formattedSubtotal: fmt(dto.subtotal, dto.currency),
  };
}
 
export function mapOrderDetailDto(dto: OrderDetailDto): OrderDetail {
  return {
    id: dto.id,
    customerId: dto.customerId,
    status: dto.status as OrderStatus,
    orderDate: new Date(dto.orderDate),
    totalAmount: dto.totalAmount,
    formattedTotal: fmt(dto.totalAmount),
    itemCount: dto.items?.length ?? 0,
    completedAt: dto.completedAt ? new Date(dto.completedAt) : null,
    cancelledAt: dto.cancelledAt ? new Date(dto.cancelledAt) : null,
    items: dto.items?.map(mapItemDto) ?? [],
  };
}