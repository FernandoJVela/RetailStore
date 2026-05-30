import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsRepository } from '@features/notifications/infrastructure/notifications.repository';
 
const KEYS = {
  all: ['notifications'] as const,
  list: (recipientId: string, params?: { status?: string; category?: string }) =>
    ['notifications', 'list', recipientId, params] as const,
  detail: (id: string) => ['notifications', id] as const,
  unread: (recipientId: string) => ['notifications', 'unread', recipientId] as const,
  preferences: (recipientId: string) => ['notifications', 'preferences', recipientId] as const,
};
 
// ═══════════════════════════════════════════════════════════
// QUERIES
// ═══════════════════════════════════════════════════════════
 
export function useNotifications(recipientId: string, params?: { status?: string; category?: string }) {
  return useQuery({
    queryKey: KEYS.list(recipientId, params),
    queryFn: () => notificationsRepository.getForRecipient(recipientId, params),
    enabled: !!recipientId,
    staleTime: 15_000,
  });
}
 
export function useNotification(id: string) {
  return useQuery({
    queryKey: KEYS.detail(id),
    queryFn: () => notificationsRepository.getById(id),
    enabled: !!id,
  });
}
 
export function useUnreadCount(recipientId: string) {
  return useQuery({
    queryKey: KEYS.unread(recipientId),
    queryFn: () => notificationsRepository.getUnreadCount(recipientId),
    enabled: !!recipientId,
    staleTime: 10_000,
    refetchInterval: 30_000, // Poll every 30s for new notifications
  });
}
 
export function useNotificationPreferences(recipientId: string) {
  return useQuery({
    queryKey: KEYS.preferences(recipientId),
    queryFn: () => notificationsRepository.getPreferences(recipientId),
    enabled: !!recipientId,
    staleTime: 60_000,
  });
}
 
// ═══════════════════════════════════════════════════════════
// MUTATIONS
// ═══════════════════════════════════════════════════════════
 
export function useMarkNotificationRead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => notificationsRepository.markRead(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.all });
    },
  });
}
 
export function useUpdatePreference() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ recipientId, category, channel, isEnabled }: {
      recipientId: string; category: string; channel: string; isEnabled: boolean;
    }) => notificationsRepository.updatePreference(recipientId, category, channel, isEnabled),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEYS.all });
    },
  });
}