import { httpClient } from '@shared/api/http-client';
import type {
  PaymentDto, PaymentDetailDto,
  CreatePaymentRequestDto, AuthorizePaymentRequestDto,
  FailPaymentRequestDto, CancelPaymentRequestDto,
  RequestRefundRequestDto, FailRefundRequestDto,
} from './payments.dto';
 
const BASE = '/payments';
 
export const paymentsApi = {
  getAll: (params?: { status?: string; customerId?: string }) =>
    httpClient.get<PaymentDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<PaymentDetailDto>(`${BASE}/${id}`),
 
  getByOrder: (orderId: string) =>
    httpClient.get<PaymentDetailDto>(`${BASE}/by-order/${orderId}`),
 
  create: (data: CreatePaymentRequestDto) =>
    httpClient.post<string>(BASE, data),
 
  authorize: (id: string, data?: AuthorizePaymentRequestDto) =>
    httpClient.put(`${BASE}/${id}/authorize`, data ?? {}),
 
  capture: (id: string) =>
    httpClient.put(`${BASE}/${id}/capture`),
 
  fail: (id: string, data: FailPaymentRequestDto) =>
    httpClient.put(`${BASE}/${id}/fail`, data),
 
  cancel: (id: string, data?: CancelPaymentRequestDto) =>
    httpClient.put(`${BASE}/${id}/cancel`, data ?? {}),
 
  requestRefund: (id: string, data: RequestRefundRequestDto) =>
    httpClient.post(`${BASE}/${id}/refunds`, data),
 
  completeRefund: (id: string, refundId: string) =>
    httpClient.put(`${BASE}/${id}/refunds/${refundId}/complete`),
 
  failRefund: (id: string, refundId: string, data: FailRefundRequestDto) =>
    httpClient.put(`${BASE}/${id}/refunds/${refundId}/fail`, data),
};