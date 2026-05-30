import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { shippingRepository } from '@features/shipping/infrastructure/shipping.repository';
import type { AssignCarrierData } from '@features/shipping';
 
const KEYS = {
  all: ['shipments'] as const,
  detail: (id: string) => ['shipments', id] as const,
  list: (params?: { status?: string }) => ['shipments', 'list', params] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function useShipments(params?: { status?: string }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => shippingRepository.getAll(params),
    staleTime: 15_000,
  });
}
 
export function useShipment(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => shippingRepository.getById(id),
    enabled: !!id,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS
// ═══════════════════════════════════════════════════════════
 
export function useAssignCarrier() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AssignCarrierData }) =>
      shippingRepository.assignCarrier(id, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useSetShippingCost() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cost, currency }: { id: string; cost: number; currency: string }) =>
      shippingRepository.setCost(id, cost, currency),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useMarkShipped() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => shippingRepository.markShipped(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useMarkInTransit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => shippingRepository.markInTransit(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useMarkDelivered() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => shippingRepository.markDelivered(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useMarkFailed() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      shippingRepository.markFailed(id, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useCancelShipment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      shippingRepository.cancel(id, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}