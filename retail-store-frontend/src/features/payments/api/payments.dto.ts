/** API DTOs — match backend JSON responses exactly. */
 
export interface PaymentDto {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  method: string;
  methodDetail: string | null;
  status: string;
  gatewayName: string | null;
  gatewayTransactionId: string | null;
  totalRefunded: number;
  netAmount: number;
  capturedAt: string | null;
  createdAt: string;
}
 
export interface PaymentDetailDto {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  method: string;
  methodDetail: string | null;
  status: string;
  gatewayName: string | null;
  gatewayTransactionId: string | null;
  authorizedAt: string | null;
  capturedAt: string | null;
  failedAt: string | null;
  refundedAt: string | null;
  cancelledAt: string | null;
  failureReason: string | null;
  notes: string | null;
  totalRefunded: number;
  netAmount: number;
  refunds: RefundDto[];
  createdAt: string;
}
 
export interface RefundDto {
  id: string;
  amount: number;
  currency: string;
  reason: string;
  status: string;
  processedAt: string | null;
}
 
export interface CreatePaymentRequestDto {
  orderId: string;
  method: string;
  methodDetail?: string | null;
  gatewayName?: string | null;
}
 
export interface AuthorizePaymentRequestDto {
  gatewayTransactionId?: string | null;
  gatewayResponse?: string | null;
}
 
export interface FailPaymentRequestDto {
  reason: string;
}
 
export interface CancelPaymentRequestDto {
  reason?: string | null;
}
 
export interface RequestRefundRequestDto {
  amount: number;
  reason: string;
}
 
export interface FailRefundRequestDto {
  reason: string;
}