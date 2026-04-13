import type { PaymentDto, PaymentDetailDto, RefundDto as RefundApiDto } from '@features/payments';
import type { Payment, PaymentDetail, Refund, PaymentStatus, RefundStatus } from '@features/payments';
import { PAYMENT_METHOD_LABELS } from '@features/payments';
 
function formatCurrency(amount: number, currency: string): string {
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency, minimumFractionDigits: 2 }).format(amount);
  } catch {
    return `${amount.toFixed(2)} ${currency}`;
  }
}
 
function buildMethodLabel(method: string, detail: string | null): string {
  const label = PAYMENT_METHOD_LABELS[method] ?? method;
  return detail ? `${label} · ${detail}` : label;
}
 
export function mapPaymentDto(dto: PaymentDto): Payment {
  return {
    id: dto.id,
    orderId: dto.orderId,
    customerId: dto.customerId,
    amount: dto.amount,
    currency: dto.currency,
    formattedAmount: formatCurrency(dto.amount, dto.currency),
    method: dto.method,
    methodLabel: buildMethodLabel(dto.method, dto.methodDetail),
    methodDetail: dto.methodDetail,
    status: dto.status as PaymentStatus,
    gatewayName: dto.gatewayName,
    gatewayTransactionId: dto.gatewayTransactionId,
    totalRefunded: dto.totalRefunded,
    netAmount: dto.netAmount,
    formattedNet: formatCurrency(dto.netAmount, dto.currency),
    hasRefunds: dto.totalRefunded > 0,
    capturedAt: dto.capturedAt ? new Date(dto.capturedAt) : null,
    createdAt: new Date(dto.createdAt),
  };
}
 
function mapRefundDto(dto: RefundApiDto): Refund {
  return {
    id: dto.id,
    amount: dto.amount,
    currency: dto.currency,
    formattedAmount: formatCurrency(dto.amount, dto.currency),
    reason: dto.reason,
    status: dto.status as RefundStatus,
    processedAt: dto.processedAt ? new Date(dto.processedAt) : null,
  };
}
 
export function mapPaymentDetailDto(dto: PaymentDetailDto): PaymentDetail {
  return {
    ...mapPaymentDto(dto),
    authorizedAt: dto.authorizedAt ? new Date(dto.authorizedAt) : null,
    failedAt: dto.failedAt ? new Date(dto.failedAt) : null,
    refundedAt: dto.refundedAt ? new Date(dto.refundedAt) : null,
    cancelledAt: dto.cancelledAt ? new Date(dto.cancelledAt) : null,
    failureReason: dto.failureReason,
    notes: dto.notes,
    refunds: dto.refunds?.map(mapRefundDto) ?? [],
  };
}