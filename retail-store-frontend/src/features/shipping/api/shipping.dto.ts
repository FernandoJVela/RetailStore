/** API DTOs — match backend JSON responses exactly. */
 
export interface ShipmentDto {
  id: string;
  orderId: string;
  customerId: string;
  status: string;
  carrier: string | null;
  trackingNumber: string | null;
  shippingCost: number;
  costCurrency: string;
  itemCount: number;
  shippedAt: string | null;
  deliveredAt: string | null;
  createdAt: string;
}
 
export interface ShipmentDetailDto {
  id: string;
  orderId: string;
  customerId: string;
  status: string;
  carrier: string | null;
  trackingNumber: string | null;
  estimatedDelivery: string | null;
  street: string;
  city: string;
  state: string | null;
  zipCode: string | null;
  country: string;
  shippingCost: number;
  costCurrency: string;
  totalWeightKg: number | null;
  shippedAt: string | null;
  deliveredAt: string | null;
  notes: string | null;
  items: ShipmentItemDto[];
  createdAt: string;
  updatedAt: string | null;
}
 
export interface ShipmentItemDto {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  weightKg: number | null;
}
 
export interface AssignCarrierRequestDto {
  carrier: string;
  trackingNumber: string;
  estimatedDelivery?: string | null;
}
 
export interface SetShippingCostRequestDto {
  cost: number;
  currency: string;
}
 
export interface FailReasonRequestDto {
  reason: string;
}