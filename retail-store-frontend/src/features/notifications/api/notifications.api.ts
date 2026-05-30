import { httpClient } from '@shared/api/http-client';
import type {
  NotificationDto, NotificationDetailDto,
  PreferenceDto, UnreadCountDto, UpdatePreferenceRequestDto,
} from './notifications.dto';
 
const BASE = '/notifications';
 
export const notificationsApi = {
  getForRecipient: (recipientId: string, params?: { status?: string; category?: string; limit?: number }) =>
    httpClient.get<NotificationDto[]>(`${BASE}/recipient/${recipientId}`, { params }),
 
  getById: (id: string) =>
    httpClient.get<NotificationDetailDto>(`${BASE}/${id}`),
 
  getUnreadCount: (recipientId: string) =>
    httpClient.get<UnreadCountDto>(`${BASE}/recipient/${recipientId}/unread-count`),
 
  markRead: (id: string) =>
    httpClient.put(`${BASE}/${id}/read`),
 
  getPreferences: (recipientId: string) =>
    httpClient.get<PreferenceDto[]>(`${BASE}/preferences/${recipientId}`),
 
  updatePreference: (data: UpdatePreferenceRequestDto) =>
    httpClient.put(`${BASE}/preferences`, data),
};