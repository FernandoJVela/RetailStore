/** Domain models — what the UI actually needs. */
 
export type ShipmentStatus = 'Pending' | 'Processing' | 'Shipped' | 'InTransit' | 'Delivered' | 'Failed' | 'Returned' | 'Cancelled';
 
export interface Shipment {
  id: string;
  orderId: string;
  customerId: string;
  status: ShipmentStatus;
  carrier: string | null;
  trackingNumber: string | null;
  shippingCost: number;
  costCurrency: string;
  formattedCost: string;
  itemCount: number;
  shippedAt: Date | null;
  deliveredAt: Date | null;
  createdAt: Date;
  hasCarrier: boolean;
}
 
export interface ShipmentDetail extends Shipment {
  estimatedDelivery: Date | null;
  address: ShippingAddress;
  totalWeightKg: number | null;
  notes: string | null;
  items: ShipmentItem[];
  updatedAt: Date | null;
}
 
export interface ShippingAddress {
  street: string;
  city: string;
  state: string | null;
  zipCode: string | null;
  country: string;
  fullAddress: string;
}
 
export interface ShipmentItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  weightKg: number | null;
}
 
export interface AssignCarrierData {
  carrier: string;
  trackingNumber: string;
  estimatedDelivery?: string;
}
 
/** Status badge variant */
export function shipmentStatusVariant(status: ShipmentStatus): 'success' | 'warning' | 'danger' | 'info' | 'default' {
  switch (status) {
    case 'Delivered': return 'success';
    case 'Shipped': case 'InTransit': return 'info';
    case 'Pending': case 'Processing': return 'warning';
    case 'Failed': case 'Cancelled': return 'danger';
    case 'Returned': return 'default';
  }
}
 
/** Status progression steps for the tracker */
export const STATUS_STEPS: ShipmentStatus[] = ['Pending', 'Processing', 'Shipped', 'InTransit', 'Delivered'];