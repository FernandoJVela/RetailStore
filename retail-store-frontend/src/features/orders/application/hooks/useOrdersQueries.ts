import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ordersRepository } from '@features/orders/infrastructure/orders.repository';
import type { CreateOrderData } from '@features/orders';
 
const KEYS = {
  all: ['orders'] as const,
  detail: (id: string) => ['orders', id] as const,
  list: (params?: { status?: string }) => ['orders', 'list', params] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function useOrders(params?: { status?: string }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => ordersRepository.getAll(params),
    staleTime: 15_000,
  });
}
 
export function useOrder(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => ordersRepository.getById(id),
    enabled: !!id,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS
// ═══════════════════════════════════════════════════════════
 
export function useCreateOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateOrderData) => ordersRepository.create(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useAddOrderItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ orderId, productId, quantity }: { orderId: string; productId: string; quantity: number }) =>
      ordersRepository.addItem(orderId, productId, quantity),
    onSuccess: (_d, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.orderId) });
    },
  });
}
 
export function useRemoveOrderItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ orderId, productId }: { orderId: string; productId: string }) =>
      ordersRepository.removeItem(orderId, productId),
    onSuccess: (_d, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.orderId) });
    },
  });
}
 
export function useConfirmOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => ordersRepository.confirm(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useCompleteOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => ordersRepository.complete(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useCancelOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      ordersRepository.cancel(id, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}