import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersRepository } from '@features/users/infrastructure/users.repository';
import type { LoginCredentials, RegisterData } from '@features/users';
import { useAuthStore } from '@shared/store/auth-store';
 
/** Query keys — single source of truth for cache invalidation */
const KEYS = {
  users: ['users'] as const,
  user: (id: string) => ['users', id] as const,
  roles: ['roles'] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES (reads)
// ═══════════════════════════════════════════════════════════
 
export function useUsers() {
  return useQuery({
    queryKey: KEYS.users,
    queryFn: () => usersRepository.getAll(),
    staleTime: 30_000,
  });
}
 
export function useUser(id: string) {
  return useQuery({
    queryKey: KEYS.user(id),
    queryFn: () => usersRepository.getById(id),
    enabled: !!id,
  });
}
 
export function useRoles() {
  return useQuery({
    queryKey: KEYS.roles,
    queryFn: () => usersRepository.getRoles(),
    staleTime: 60_000,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS (writes)
// ═══════════════════════════════════════════════════════════
 
export function useLogin() {
  const login = useAuthStore((s) => s.login);
 
  return useMutation({
    mutationFn: (credentials: LoginCredentials) =>
      usersRepository.login(credentials),
    onSuccess: (session) => {
      login(
        {
          userId: session.userId,
          username: session.username,
          email: session.email,
          roles: session.roles,
          permissions: session.permissions,
        },
        session.accessToken,
        session.refreshToken
      );
    },
  });
}
 
export function useRegister() {
  return useMutation({
    mutationFn: (data: RegisterData) => usersRepository.register(data),
  });
}
 
export function useDeactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersRepository.deactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.users }); },
  });
}
 
export function useReactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersRepository.reactivate(id),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.users }); },
  });
}
 
export function useAssignRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleName }: { userId: string; roleName: string }) =>
      usersRepository.assignRole(userId, roleName),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.users }); },
  });
}
 
export function useRevokeRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleName }: { userId: string; roleName: string }) =>
      usersRepository.revokeRole(userId, roleName),
    onSuccess: () => { qc.invalidateQueries({ queryKey: KEYS.users }); },
  });
}