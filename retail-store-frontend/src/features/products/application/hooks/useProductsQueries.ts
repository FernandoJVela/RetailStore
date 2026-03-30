import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productsRepository } from '@features/products/infrastructure/products.repository';
import type { CreateProductData, UpdateProductDetailsData, UpdateProductPriceData } from '@features/products';
 
const KEYS = {
  all: ['products'] as const,
  detail: (id: string) => ['products', id] as const,
  list: (params?: { category?: string; isActive?: boolean; search?: string }) =>
    ['products', 'list', params] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function useProducts(params?: { category?: string; isActive?: boolean; search?: string }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => productsRepository.getAll(params),
    staleTime: 30_000,
  });
}
 
export function useProduct(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => productsRepository.getById(id),
    enabled: !!id,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS
// ═══════════════════════════════════════════════════════════
 
export function useCreateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateProductData) => productsRepository.create(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useUpdateProductDetails() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductDetailsData }) =>
      productsRepository.updateDetails(id, data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.id) });
    },
  });
}
 
export function useUpdateProductPrice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductPriceData }) =>
      productsRepository.updatePrice(id, data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.id) });
    },
  });
}
 
export function useDeactivateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => productsRepository.deactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useReactivateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => productsRepository.reactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}