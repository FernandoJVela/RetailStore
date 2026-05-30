import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Settings, CheckCheck, Inbox } from 'lucide-react';
import { Button, Card, Badge, Spinner, EmptyState, FilterPillBar } from '@shared/components/ui';
import { useAuthStore } from '@shared/store/auth-store';
import {
  useNotifications, useUnreadCount, useMarkNotificationRead,
} from '@features/notifications/application/hooks/useNotificationsQueries';
import { NotificationItem } from '@features/notifications';
import { NotificationDetailPanel } from '@features/notifications';
import { PreferencesModal } from '@features/notifications';
import type { NotificationCategory } from '@features/notifications';

type FilterTab = 'all' | 'unread' | NotificationCategory;

export function NotificationsPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const recipientId = user?.userId ?? '';

  const [activeTab, setActiveTab] = useState<FilterTab>('all');
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [showPreferences, setShowPreferences] = useState(false);

  const tabs: { key: FilterTab; label: string }[] = [
    { key: 'all', label: t('notifications.tab_all') },
    { key: 'unread', label: t('notifications.tab_unread') },
    { key: 'Order', label: t('notifications.tab_orders') },
    { key: 'Inventory', label: t('notifications.tab_inventory') },
    { key: 'Shipping', label: t('notifications.tab_shipping') },
    { key: 'System', label: t('notifications.tab_system') },
  ];

  const queryParams = {
    status: activeTab === 'unread' ? 'Delivered' as string : undefined,
    category: !['all', 'unread'].includes(activeTab) ? activeTab : undefined,
  };

  const { data: notifications, isLoading } = useNotifications(recipientId, queryParams);
  const { data: unreadCount } = useUnreadCount(recipientId);
  const markReadMut = useMarkNotificationRead();

  const handleMarkAllRead = async () => {
    const unread = notifications?.filter((n) => n.isUnread) ?? [];
    for (const n of unread) {
      await markReadMut.mutateAsync(n.id);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div>
            <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.notifications')}</h1>
            <p className="mt-1 text-sm text-[var(--text-secondary)]">
              {unreadCount ? t('notifications.unreadCount', { count: unreadCount }) : t('notifications.allCaughtUp')}
            </p>
          </div>
          {!!unreadCount && unreadCount > 0 && (
            <Badge variant="danger">{unreadCount}</Badge>
          )}
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleMarkAllRead} disabled={!unreadCount}>
            <CheckCheck className="h-4 w-4" /> {t('notifications.markAllRead')}
          </Button>
          <Button variant="ghost" size="sm" onClick={() => setShowPreferences(true)}>
            <Settings className="h-4 w-4" /> {t('notifications.preferences')}
          </Button>
        </div>
      </div>

      {/* Filter tabs */}
      <Card>
        <FilterPillBar
          options={tabs}
          value={activeTab}
          onChange={(v) => setActiveTab(v as typeof activeTab)}
        />
      </Card>

      {/* Feed */}
      <div className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] overflow-hidden">
        {isLoading ? (
          <Spinner />
        ) : !notifications?.length ? (
          <EmptyState
            icon={<Inbox className="h-12 w-12" />}
            title={t('notifications.noNotifications')}
            description={activeTab === 'unread' ? t('notifications.noNotificationsUnreadDesc') : t('notifications.noNotificationsAllDesc')}
          />
        ) : (
          <div className="divide-y divide-[var(--border-color)]">
            {notifications.map((notification) => (
              <NotificationItem
                key={notification.id}
                notification={notification}
                onClick={() => {
                  setSelectedId(notification.id);
                  if (notification.isUnread) markReadMut.mutate(notification.id);
                }}
              />
            ))}
          </div>
        )}
      </div>

      {/* Detail panel */}
      {selectedId && (
        <NotificationDetailPanel
          notificationId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}

      {/* Preferences modal */}
      <PreferencesModal
        recipientId={recipientId}
        isOpen={showPreferences}
        onClose={() => setShowPreferences(false)}
      />
    </div>
  );
}
