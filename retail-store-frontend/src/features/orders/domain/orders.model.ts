/** Domain models — what the UI actually needs. */
 
export type OrderStatus = 'Draft' | 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Completed' | 'Cancelled';
 
export interface Order {
  id: string;
  customerId: string;
  status: OrderStatus;
  orderDate: Date;
  totalAmount: number;
  formattedTotal: string;
  itemCount: number;
  completedAt: Date | null;
  cancelledAt: Date | null;
}
 
export interface OrderDetail extends Order {
  items: OrderItem[];
}
 
export interface OrderItem {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  currency: string;
  subtotal: number;
  formattedPrice: string;
  formattedSubtotal: string;
}
 
export interface CreateOrderData {
  customerId: string;
  items: { productId: string; quantity: number }[];
}
 
/** Badge variant per order status */
export function orderStatusVariant(status: OrderStatus): 'success' | 'warning' | 'danger' | 'info' | 'default' {
  switch (status) {
    case 'Completed': case 'Delivered': return 'success';
    case 'Confirmed': case 'Shipped': return 'info';
    case 'Pending': case 'Draft': return 'warning';
    case 'Cancelled': return 'danger';
  }
}
 
/** Status progression for the order lifecycle */
export const ORDER_STATUS_STEPS: OrderStatus[] = ['Draft', 'Pending', 'Confirmed', 'Shipped', 'Delivered', 'Completed'];