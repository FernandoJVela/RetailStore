export { notificationsApi } from './api/notifications.api';
export type * from './api/notifications.dto';
export type {
  Notification, NotificationDetail, NotificationPreference,
  NotificationChannel, NotificationCategory, NotificationStatus, NotificationPriority,
} from './domain/notifications.model';
export { notificationStatusVariant, priorityVariant, CATEGORY_COLORS } from './domain/notifications.model';
export { NotificationsPage } from './ui/pages/NotificationsPage';
export { useNotifications, useUnreadCount, useNotificationPreferences } from './application/hooks/useNotificationsQueries';
export { NotificationDetailPanel } from './ui/components/NotificationDetailPanel';
export { NotificationItem } from './ui/components/NotificationItem';
export { PreferencesModal } from './ui/components/PreferencesModal';