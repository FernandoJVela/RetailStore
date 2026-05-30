import { useTranslation } from 'react-i18next';
import { X, CheckCircle, Clock, Mail, AlertTriangle } from 'lucide-react';
import { Button, Badge, Spinner } from '@shared/components/ui';
import { formatDateTime } from '@shared/lib/utils';
import { useNotification, useMarkNotificationRead } from '@features/notifications/application/hooks/useNotificationsQueries';
import { notificationStatusVariant, priorityVariant } from '@features/notifications';
 
interface NotificationDetailPanelProps {
  notificationId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
export function NotificationDetailPanel({ notificationId, isOpen, onClose }: NotificationDetailPanelProps) {
  const { t } = useTranslation();
  const { data: notification, isLoading } = useNotification(notificationId);
  const markReadMut = useMarkNotificationRead();
 
  const handleMarkRead = async () => {
    await markReadMut.mutateAsync(notificationId);
  };
 
  if (!isOpen) return null;
 
  const timeline = notification ? [
    { label: 'Created', date: notification.createdAt, icon: Clock },
    { label: 'Sent', date: notification.sentAt, icon: Mail },
    { label: 'Delivered', date: notification.deliveredAt, icon: CheckCircle },
    { label: 'Read', date: notification.readAt, icon: CheckCircle },
    { label: 'Failed', date: notification.failedAt, icon: AlertTriangle },
  ].filter((e) => e.date !== null) : [];
 
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-lg overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Notification</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : notification ? (
          <div className="p-6 space-y-6">
            {/* Header */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex flex-wrap items-center gap-2 mb-3">
                <Badge variant={notificationStatusVariant(notification.status)}>{notification.status}</Badge>
                <Badge variant="default">{notification.channel}</Badge>
                <Badge variant="default">{notification.category}</Badge>
                {notification.priority !== 'Normal' && (
                  <Badge variant={priorityVariant(notification.priority)}>{notification.priority}</Badge>
                )}
              </div>
              <h3 className="text-lg font-semibold text-[var(--text-primary)]">
                {notification.subject ?? `${notification.category} Notification`}
              </h3>
              <p className="mt-1 text-xs text-[var(--text-muted)]">{formatDateTime(notification.createdAt)}</p>
            </section>
 
            {/* Body */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h4 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Message</h4>
              <div className="text-sm text-[var(--text-secondary)] leading-relaxed whitespace-pre-wrap">
                {notification.body}
              </div>
            </section>
 
            {/* Failure info */}
            {notification.failureReason && (
              <div className="flex items-start gap-2 rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-4">
                <AlertTriangle className="h-4 w-4 text-red-500 mt-0.5 shrink-0" />
                <div>
                  <p className="text-sm font-medium text-red-700 dark:text-red-400">Delivery failed</p>
                  <p className="text-sm text-red-600 dark:text-red-400 mt-0.5">{notification.failureReason}</p>
                  <p className="text-xs text-red-500 mt-1">Retry count: {notification.retryCount}</p>
                </div>
              </div>
            )}
 
            {/* Actions */}
            {notification.isUnread && (
              <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
                <Button size="sm" onClick={handleMarkRead} loading={markReadMut.isPending}>
                  <CheckCircle className="h-4 w-4" /> Mark as Read
                </Button>
              </section>
            )}
 
            {/* Timeline */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h4 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Timeline</h4>
              <div className="space-y-3">
                {timeline.map((event, idx) => (
                  <div key={idx} className="flex items-center gap-3">
                    <event.icon className="h-4 w-4 shrink-0 text-[var(--text-muted)]" />
                    <span className="text-sm font-medium text-[var(--text-primary)] flex-1">{event.label}</span>
                    <span className="text-xs text-[var(--text-muted)] tabular-nums">
                      {event.date ? formatDateTime(event.date) : ''}
                    </span>
                  </div>
                ))}
              </div>
            </section>
 
            {/* Reference */}
            {notification.referenceType && (
              <div className="text-xs text-[var(--text-muted)]">
                Reference: {notification.referenceType} {notification.referenceId?.substring(0, 8)}
              </div>
            )}
          </div>
        ) : null}
      </div>
    </>
  );
}