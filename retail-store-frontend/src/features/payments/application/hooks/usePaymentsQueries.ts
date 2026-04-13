import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { paymentsRepository } from '@features/payments';
import type { RequestRefundData } from '@features/payments';
 
const KEYS = {
  all: ['payments'] as const,
  detail: (id: string) => ['payments', id] as const,
  list: (params?: { status?: string; customerId?: string }) =>
    ['payments', 'list', params] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function usePayments(params?: { status?: string; customerId?: string }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => paymentsRepository.getAll(params),
    staleTime: 15_000,
  });
}
 
export function usePayment(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => paymentsRepository.getById(id),
    enabled: !!id,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS — Lifecycle
// ═══════════════════════════════════════════════════════════
 
export function useAuthorizePayment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, gatewayTransactionId }: { id: string; gatewayTransactionId?: string }) =>
      paymentsRepository.authorize(id, gatewayTransactionId),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useCapturePayment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => paymentsRepository.capture(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useFailPayment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      paymentsRepository.fail(id, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useCancelPayment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) =>
      paymentsRepository.cancel(id, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS — Refunds
// ═══════════════════════════════════════════════════════════
 
export function useRequestRefund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ paymentId, data }: { paymentId: string; data: RequestRefundData }) =>
      paymentsRepository.requestRefund(paymentId, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useCompleteRefund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ paymentId, refundId }: { paymentId: string; refundId: string }) =>
      paymentsRepository.completeRefund(paymentId, refundId),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}