import type { ShipmentDto, ShipmentDetailDto, ShipmentItemDto } from '@features/shipping';
import type { Shipment, ShipmentDetail, ShipmentItem, ShippingAddress, ShipmentStatus } from '@features/shipping';
 
function formatCurrency(amount: number, currency: string): string {
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency, minimumFractionDigits: 2 }).format(amount);
  } catch { return `${amount.toFixed(2)} ${currency}`; }
}
 
export function mapShipmentDto(dto: ShipmentDto): Shipment {
  return {
    id: dto.id,
    orderId: dto.orderId,
    customerId: dto.customerId,
    status: dto.status as ShipmentStatus,
    carrier: dto.carrier,
    trackingNumber: dto.trackingNumber,
    shippingCost: dto.shippingCost,
    costCurrency: dto.costCurrency,
    formattedCost: formatCurrency(dto.shippingCost, dto.costCurrency),
    itemCount: dto.itemCount,
    shippedAt: dto.shippedAt ? new Date(dto.shippedAt) : null,
    deliveredAt: dto.deliveredAt ? new Date(dto.deliveredAt) : null,
    createdAt: new Date(dto.createdAt),
    hasCarrier: !!dto.carrier,
  };
}
 
function mapAddress(dto: ShipmentDetailDto): ShippingAddress {
  return {
    street: dto.street,
    city: dto.city,
    state: dto.state,
    zipCode: dto.zipCode,
    country: dto.country,
    fullAddress: [dto.street, dto.city, dto.state, dto.zipCode, dto.country].filter(Boolean).join(', '),
  };
}
 
function mapItem(dto: ShipmentItemDto): ShipmentItem {
  return { id: dto.id, productId: dto.productId, productName: dto.productName, quantity: dto.quantity, weightKg: dto.weightKg };
}
 
export function mapShipmentDetailDto(dto: ShipmentDetailDto): ShipmentDetail {
  return {
    ...mapShipmentDto({ ...dto, itemCount: dto.items?.length ?? 0 }),
    estimatedDelivery: dto.estimatedDelivery ? new Date(dto.estimatedDelivery) : null,
    address: mapAddress(dto),
    totalWeightKg: dto.totalWeightKg,
    notes: dto.notes,
    items: dto.items?.map(mapItem) ?? [],
    updatedAt: dto.updatedAt ? new Date(dto.updatedAt) : null,
  };
}