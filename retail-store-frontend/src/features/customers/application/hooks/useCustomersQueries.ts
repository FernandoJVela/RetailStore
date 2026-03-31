import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customersRepository } from '@features/customers/infrastructure/customers.repository';
import type { RegisterCustomerData, UpdateCustomerData, UpdateAddressData } from '@features/customers';
 
/** Query keys — single source of truth for cache invalidation */
const KEYS = {
  all: ['customers'] as const,
  detail: (id: string) => ['customers', id] as const,
  list: (params?: { search?: string; isActive?: boolean }) =>
    ['customers', 'list', params] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES (reads)
// ═══════════════════════════════════════════════════════════
 
export function useCustomers(params?: { search?: string; isActive?: boolean }) {
  return useQuery({
    queryKey: KEYS.list(params),
    queryFn: () => customersRepository.getAll(params),
    staleTime: 30_000,
  });
}
 
export function useCustomer(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => customersRepository.getById(id),
    enabled: !!id,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS (writes)
// ═══════════════════════════════════════════════════════════
 
export function useRegisterCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterCustomerData) => customersRepository.register(data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useUpdateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCustomerData }) =>
      customersRepository.update(id, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useChangeCustomerEmail() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, newEmail }: { id: string; newEmail: string }) =>
      customersRepository.changeEmail(id, newEmail),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useUpdateCustomerAddress() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAddressData }) =>
      customersRepository.updateAddress(id, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useDeactivateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customersRepository.deactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}
 
export function useReactivateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customersRepository.reactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.all }); },
  });
}