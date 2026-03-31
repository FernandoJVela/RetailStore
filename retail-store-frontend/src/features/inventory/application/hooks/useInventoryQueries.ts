import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { inventoryRepository } from '@features/inventory/infrastructure/inventory.repository';
import type { CreateInventoryData, AdjustStockData } from '@features/inventory';
 
const KEYS = {
  all: ['inventory'] as const,
  detail: (productId: string) => ['inventory', productId] as const,
  list: (params?: { stockStatus?: string }) => ['inventory', 'list', params] as const,
  lowStock: ['inventory', 'low-stock'] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function useInventory(params?: { stockStatus?: string }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => inventoryRepository.getAll(params),
    staleTime: 15_000,
  });
}
 
export function useInventoryDetail(productId: string) {
  return useQuery({
    queryKey: KEYS.detail(productId),
    queryFn: () => inventoryRepository.getByProduct(productId),
    enabled: !!productId,
  });
}
 
export function useLowStock() {
  return useQuery({
    queryKey: KEYS.lowStock,
    queryFn: () => inventoryRepository.getLowStock(),
    staleTime: 15_000,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS
// ═══════════════════════════════════════════════════════════
 
export function useCreateInventoryItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateInventoryData) => inventoryRepository.create(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useAddStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) =>
      inventoryRepository.addStock(productId, quantity),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.all });
    },
  });
}
 
export function useRemoveStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) =>
      inventoryRepository.removeStock(productId, quantity),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.all });
    },
  });
}
 
export function useAdjustStock() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, data }: { productId: string; data: AdjustStockData }) =>
      inventoryRepository.adjustStock(productId, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.all });
    },
  });
}
 
export function useUpdateThreshold() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, newThreshold }: { productId: string; newThreshold: number }) =>
      inventoryRepository.updateThreshold(productId, newThreshold),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.all });
    },
  });
}