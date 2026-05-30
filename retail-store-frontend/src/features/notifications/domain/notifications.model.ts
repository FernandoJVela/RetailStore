/** Domain models — what the UI actually needs. */
 
export type NotificationChannel = 'Email' | 'Sms' | 'Push' | 'InApp';
export type NotificationCategory = 'Order' | 'Inventory' | 'Shipping' | 'User' | 'System' | 'Marketing';
export type NotificationStatus = 'Pending' | 'Queued' | 'Sending' | 'Sent' | 'Delivered' | 'Read' | 'Failed' | 'Cancelled';
export type NotificationPriority = 'Low' | 'Normal' | 'High' | 'Urgent';
 
export interface Notification {
  id: string;
  channel: NotificationChannel;
  category: NotificationCategory;
  subject: string | null;
  status: NotificationStatus;
  priority: NotificationPriority;
  createdAt: Date;
  readAt: Date | null;
  isRead: boolean;
  isUnread: boolean;
  timeAgo: string;         // Computed: "2h ago", "3d ago"
}
 
export interface NotificationDetail extends Notification {
  templateId: string | null;
  recipientId: string | null;
  recipientType: string;
  body: string;
  sentAt: Date | null;
  deliveredAt: Date | null;
  failedAt: Date | null;
  failureReason: string | null;
  retryCount: number;
  referenceType: string | null;
  referenceId: string | null;
}
 
export interface NotificationPreference {
  id: string;
  category: NotificationCategory;
  channel: NotificationChannel;
  isEnabled: boolean;
}
 
/** Badge variant per notification status */
export function notificationStatusVariant(status: NotificationStatus): 'success' | 'warning' | 'danger' | 'info' | 'default' {
  switch (status) {
    case 'Read': case 'Delivered': return 'success';
    case 'Sent': return 'info';
    case 'Pending': case 'Queued': case 'Sending': return 'warning';
    case 'Failed': case 'Cancelled': return 'danger';
  }
}
 
/** Priority badge variant */
export function priorityVariant(priority: NotificationPriority): 'danger' | 'warning' | 'info' | 'default' {
  switch (priority) {
    case 'Urgent': return 'danger';
    case 'High': return 'warning';
    case 'Normal': return 'info';
    case 'Low': return 'default';
  }
}
 
/** Category icon color */
export const CATEGORY_COLORS: Record<string, string> = {
  Order: 'text-blue-600 dark:text-blue-400',
  Inventory: 'text-amber-600 dark:text-amber-400',
  Shipping: 'text-violet-600 dark:text-violet-400',
  User: 'text-primary-600 dark:text-primary-400',
  System: 'text-red-600 dark:text-red-400',
  Marketing: 'text-emerald-600 dark:text-emerald-400',
};