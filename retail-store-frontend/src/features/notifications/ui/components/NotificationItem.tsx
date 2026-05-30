import { Mail, Bell, Smartphone, MessageSquare, ShoppingCart, Warehouse, Truck, User, AlertCircle, Megaphone } from 'lucide-react';
import { Badge } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { priorityVariant, CATEGORY_COLORS } from '@features/notifications';
import type { Notification, NotificationCategory, NotificationChannel } from '@features/notifications';
 
interface NotificationItemProps {
  notification: Notification;
  onClick: () => void;
}
 
const channelIcons: Record<NotificationChannel, typeof Mail> = {
  Email: Mail, InApp: Bell, Push: Smartphone, Sms: MessageSquare,
};
 
const categoryIcons: Record<NotificationCategory, typeof ShoppingCart> = {
  Order: ShoppingCart, Inventory: Warehouse, Shipping: Truck,
  User: User, System: AlertCircle, Marketing: Megaphone,
};
 
export function NotificationItem({ notification, onClick }: NotificationItemProps) {
  const ChannelIcon = channelIcons[notification.channel] ?? Bell;
  const CategoryIcon = categoryIcons[notification.category] ?? Bell;
  const catColor = CATEGORY_COLORS[notification.category] ?? 'text-[var(--text-muted)]';
 
  return (
    <button
      onClick={onClick}
      className={cn(
        'flex w-full items-start gap-3 rounded-lg px-4 py-3.5 text-left transition-colors',
        notification.isUnread
          ? 'bg-primary-50/50 dark:bg-primary-500/5 hover:bg-primary-100/50 dark:hover:bg-primary-500/10'
          : 'hover:bg-[var(--bg-tertiary)]/50'
      )}
    >
      {/* Category icon */}
      <div className={cn('mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-[var(--bg-tertiary)]', catColor)}>
        <CategoryIcon className="h-4 w-4" />
      </div>
 
      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          {notification.isUnread && (
            <div className="h-2 w-2 shrink-0 rounded-full bg-primary-600" />
          )}
          <p className={cn(
            'text-sm truncate',
            notification.isUnread ? 'font-semibold text-[var(--text-primary)]' : 'text-[var(--text-primary)]'
          )}>
            {notification.subject ?? `${notification.category} notification`}
          </p>
        </div>
        <div className="flex items-center gap-2 mt-1">
          <Badge variant="default">{notification.category}</Badge>
          {notification.priority !== 'Normal' && (
            <Badge variant={priorityVariant(notification.priority)}>{notification.priority}</Badge>
          )}
          <ChannelIcon className="h-3 w-3 text-[var(--text-muted)]" />
        </div>
      </div>
 
      {/* Time */}
      <span className="shrink-0 text-xs text-[var(--text-muted)] mt-1">{notification.timeAgo}</span>
    </button>
  );
}