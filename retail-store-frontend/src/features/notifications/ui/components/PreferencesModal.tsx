import { useTranslation } from 'react-i18next';
import { Modal, Spinner } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { useNotificationPreferences, useUpdatePreference } from '@features/notifications/application/hooks/useNotificationsQueries';
import type { NotificationCategory, NotificationChannel } from '@features/notifications';
 
interface PreferencesModalProps {
  recipientId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
const CATEGORIES: NotificationCategory[] = ['Order', 'Inventory', 'Shipping', 'User', 'System', 'Marketing'];
const CHANNELS: NotificationChannel[] = ['Email', 'InApp'];
 
export function PreferencesModal({ recipientId, isOpen, onClose }: PreferencesModalProps) {
  const { t } = useTranslation();
  const { data: preferences, isLoading } = useNotificationPreferences(recipientId);
  const updateMut = useUpdatePreference();
 
  const isEnabled = (category: string, channel: string): boolean => {
    const pref = preferences?.find((p) => p.category === category && p.channel === channel);
    return pref?.isEnabled ?? true; // Default: enabled
  };
 
  const handleToggle = (category: string, channel: string) => {
    const current = isEnabled(category, channel);
    updateMut.mutate({ recipientId, category, channel, isEnabled: !current });
  };
 
  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Notification Preferences" size="lg">
      {isLoading ? (
        <Spinner />
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--border-color)]">
                <th className="pb-3 text-left font-medium text-[var(--text-secondary)]">Category</th>
                {CHANNELS.map((ch) => (
                  <th key={ch} className="pb-3 text-center font-medium text-[var(--text-secondary)]">{ch}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-[var(--border-color)]">
              {CATEGORIES.map((cat) => (
                <tr key={cat}>
                  <td className="py-3 font-medium text-[var(--text-primary)]">{cat}</td>
                  {CHANNELS.map((ch) => {
                    const enabled = isEnabled(cat, ch);
                    return (
                      <td key={ch} className="py-3 text-center">
                        <button
                          onClick={() => handleToggle(cat, ch)}
                          className={cn(
                            'relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors',
                            enabled ? 'bg-primary-600' : 'bg-[var(--bg-tertiary)]'
                          )}
                        >
                          <span
                            className={cn(
                              'pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow transition-transform',
                              enabled ? 'translate-x-5' : 'translate-x-0'
                            )}
                          />
                        </button>
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
          <p className="mt-4 text-xs text-[var(--text-muted)]">
            Disabled channels will not receive notifications for that category.
          </p>
        </div>
      )}
    </Modal>
  );
}