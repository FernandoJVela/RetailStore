import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { providersRepository } from '@features/providers';
import type { RegisterProviderData, UpdateProviderData } from '@features/providers';
 
const KEYS = {
  all: ['providers'] as const,
  detail: (id: string) => ['providers', id] as const,
  list: (params?: { search?: string; isActive?: boolean }) =>
    ['providers', 'list', params] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function useProviders(params?: { search?: string; isActive?: boolean }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => providersRepository.getAll(params),
    staleTime: 30_000,
  });
}
 
export function useProvider(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => providersRepository.getById(id),
    enabled: !!id,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS
// ═══════════════════════════════════════════════════════════
 
export function useRegisterProvider() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterProviderData) => providersRepository.register(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useUpdateProvider() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProviderData }) =>
      providersRepository.update(id, data),
    onSuccess: (_d, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.id) });
    },
  });
}
 
export function useChangeProviderEmail() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, newEmail }: { id: string; newEmail: string }) =>
      providersRepository.changeEmail(id, newEmail),
    onSuccess: (_d, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.id) });
    },
  });
}
 
export function useAssociateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ providerId, productId }: { providerId: string; productId: string }) =>
      providersRepository.associateProduct(providerId, productId),
    onSuccess: (_d, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.providerId) });
    },
  });
}
 
export function useDissociateProduct() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ providerId, productId }: { providerId: string; productId: string }) =>
      providersRepository.dissociateProduct(providerId, productId),
    onSuccess: (_d, vars) => {
      qc.invalidateQueries({ queryKey: KEYS.all });
      qc.invalidateQueries({ queryKey: KEYS.detail(vars.providerId) });
    },
  });
}
 
export function useDeactivateProvider() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => providersRepository.deactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useReactivateProvider() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => providersRepository.reactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}