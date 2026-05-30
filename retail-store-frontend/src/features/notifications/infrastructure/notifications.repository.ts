import { notificationsApi } from '@features/notifications';
import type { Notification, NotificationDetail, NotificationPreference } from '@features/notifications';
import { mapNotificationDto, mapNotificationDetailDto, mapPreferenceDto } from '@features/notifications/application/mappers/notifications.mapper';
 
export const notificationsRepository = {
  async getForRecipient(recipientId: string, params?: { status?: string; category?: string; limit?: number }): Promise<Notification[]> {
    const { data } = await notificationsApi.getForRecipient(recipientId, params);
    return data.map(mapNotificationDto);
  },
 
  async getById(id: string): Promise<NotificationDetail> {
    const { data } = await notificationsApi.getById(id);
    return mapNotificationDetailDto(data);
  },
 
  async getUnreadCount(recipientId: string): Promise<number> {
    const { data } = await notificationsApi.getUnreadCount(recipientId);
    return data.count;
  },
 
  async markRead(id: string): Promise<void> {
    await notificationsApi.markRead(id);
  },
 
  async getPreferences(recipientId: string): Promise<NotificationPreference[]> {
    const { data } = await notificationsApi.getPreferences(recipientId);
    return data.map(mapPreferenceDto);
  },
 
  async updatePreference(recipientId: string, category: string, channel: string, isEnabled: boolean): Promise<void> {
    await notificationsApi.updatePreference({
      recipientId, recipientType: 'User', category, channel, isEnabled,
    });
  },
};