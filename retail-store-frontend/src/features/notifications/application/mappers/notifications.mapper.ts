import type { NotificationDto, NotificationDetailDto, PreferenceDto } from '@features/notifications';
import type {
  Notification, NotificationDetail, NotificationPreference,
  NotificationChannel, NotificationCategory, NotificationStatus, NotificationPriority,
} from '@features/notifications';
 
function computeTimeAgo(date: Date): string {
  const now = Date.now();
  const diffMs = now - date.getTime();
  const mins = Math.floor(diffMs / 60_000);
  if (mins < 1) return 'Just now';
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;
  const months = Math.floor(days / 30);
  return `${months}mo ago`;
}
 
export function mapNotificationDto(dto: NotificationDto): Notification {
  const createdAt = new Date(dto.createdAt);
  const readAt = dto.readAt ? new Date(dto.readAt) : null;
  return {
    id: dto.id,
    channel: dto.channel as NotificationChannel,
    category: dto.category as NotificationCategory,
    subject: dto.subject,
    status: dto.status as NotificationStatus,
    priority: dto.priority as NotificationPriority,
    createdAt,
    readAt,
    isRead: dto.status === 'Read' || readAt !== null,
    isUnread: dto.status !== 'Read' && readAt === null,
    timeAgo: computeTimeAgo(createdAt),
  };
}
 
export function mapNotificationDetailDto(dto: NotificationDetailDto): NotificationDetail {
  const base = mapNotificationDto(dto);
  return {
    ...base,
    templateId: dto.templateId,
    recipientId: dto.recipientId,
    recipientType: dto.recipientType,
    body: dto.body,
    sentAt: dto.sentAt ? new Date(dto.sentAt) : null,
    deliveredAt: dto.deliveredAt ? new Date(dto.deliveredAt) : null,
    failedAt: dto.failedAt ? new Date(dto.failedAt) : null,
    failureReason: dto.failureReason,
    retryCount: dto.retryCount,
    referenceType: dto.referenceType,
    referenceId: dto.referenceId,
  };
}
 
export function mapPreferenceDto(dto: PreferenceDto): NotificationPreference {
  return {
    id: dto.id,
    category: dto.category as NotificationCategory,
    channel: dto.channel as NotificationChannel,
    isEnabled: dto.isEnabled,
  };
}