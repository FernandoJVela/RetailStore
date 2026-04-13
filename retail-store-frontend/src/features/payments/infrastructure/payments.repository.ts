import { paymentsApi } from '@features/payments';
import type { Payment, PaymentDetail, RequestRefundData } from '@features/payments';
import { mapPaymentDto, mapPaymentDetailDto } from '@features/payments/application/mappers/payments.mapper';
 
export const paymentsRepository = {
  async getAll(params?: { status?: string; customerId?: string }): Promise<Payment[]> {
    const { data } = await paymentsApi.getAll(params);
    return data.map(mapPaymentDto);
  },
 
  async getById(id: string): Promise<PaymentDetail> {
    const { data } = await paymentsApi.getById(id);
    return mapPaymentDetailDto(data);
  },
 
  async authorize(id: string, gatewayTransactionId?: string): Promise<void> {
    await paymentsApi.authorize(id, { gatewayTransactionId });
  },
 
  async capture(id: string): Promise<void> {
    await paymentsApi.capture(id);
  },
 
  async fail(id: string, reason: string): Promise<void> {
    await paymentsApi.fail(id, { reason });
  },
 
  async cancel(id: string, reason?: string): Promise<void> {
    await paymentsApi.cancel(id, { reason });
  },
 
  async requestRefund(paymentId: string, input: RequestRefundData): Promise<string> {
    const { data } = await paymentsApi.requestRefund(paymentId, {
      amount: input.amount,
      reason: input.reason,
    });
    return data;
  },
 
  async completeRefund(paymentId: string, refundId: string): Promise<void> {
    await paymentsApi.completeRefund(paymentId, refundId);
  },
 
  async failRefund(paymentId: string, refundId: string, reason: string): Promise<void> {
    await paymentsApi.failRefund(paymentId, refundId, { reason });
  },
};