import { X, CheckCircle, AlertTriangle, AlertCircle, Info } from 'lucide-react';
import { cn } from '@shared/lib/utils';
import { useToastStore } from '@shared/store/toast-store';
 
const icons = {
  success: CheckCircle,
  error: AlertCircle,
  warning: AlertTriangle,
  info: Info,
};
 
const styles = {
  success: 'border-emerald-200 bg-emerald-50 dark:border-emerald-800 dark:bg-emerald-900/20',
  error: 'border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-900/20',
  warning: 'border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-900/20',
  info: 'border-blue-200 bg-blue-50 dark:border-blue-800 dark:bg-blue-900/20',
};
 
const iconColors = {
  success: 'text-emerald-600 dark:text-emerald-400',
  error: 'text-red-600 dark:text-red-400',
  warning: 'text-amber-600 dark:text-amber-400',
  info: 'text-blue-600 dark:text-blue-400',
};
 
export function ToastContainer() {
  const { toasts, removeToast } = useToastStore();
 
  if (!toasts.length) return null;
 
  return (
    <div className="fixed top-4 right-4 z-[100] space-y-2 w-96">
      {toasts.map((toast) => {
        const Icon = icons[toast.type];
        return (
          <div
            key={toast.id}
            className={cn(
              'flex items-start gap-3 rounded-lg border p-4 shadow-lg animate-in slide-in-from-right',
              styles[toast.type]
            )}
          >
            <Icon className={cn('h-5 w-5 shrink-0 mt-0.5', iconColors[toast.type])} />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-[var(--text-primary)]">{toast.title}</p>
              {toast.description && (
                <p className="mt-1 text-xs text-[var(--text-secondary)]">{toast.description}</p>
              )}
            </div>
            <button
              onClick={() => removeToast(toast.id)}
              className="shrink-0 rounded p-0.5 text-[var(--text-muted)] hover:text-[var(--text-primary)]"
            >
              <X className="h-4 w-4" />
            </button>
          </div>
        );
      })}
    </div>
  );
}