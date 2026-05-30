/** API DTOs — match backend JSON responses exactly. */
 
export interface OrderDto {
  id: string;
  customerId: string;
  status: string;
  orderDate: string;
  totalAmount: number;
  itemCount: number;
  completedAt: string | null;
  cancelledAt: string | null;
}
 
export interface OrderDetailDto {
  id: string;
  customerId: string;
  status: string;
  orderDate: string;
  totalAmount: number;
  completedAt: string | null;
  cancelledAt: string | null;
  items: OrderItemDto[];
}
 
export interface OrderItemDto {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  currency: string;
  subtotal: number;
}
 
export interface CreateOrderRequestDto {
  customerId: string;
  orderDate?: string | null;
  items: CreateOrderItemRequestDto[];
}
 
export interface CreateOrderItemRequestDto {
  productId: string;
  quantity: number;
}
 
export interface AddOrderItemRequestDto {
  productId: string;
  quantity: number;
}
 
export interface CancelOrderRequestDto {
  reason: string;
}