import { forwardRef, type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@shared/lib/utils';
import { X } from 'lucide-react';
 
/* ═══════════════════════════════════════════════════════════
   BUTTON
   ═══════════════════════════════════════════════════════════ */
type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'outline';
type ButtonSize = 'sm' | 'md' | 'lg';
 
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
}
 
const variantStyles: Record<ButtonVariant, string> = {
  primary: 'bg-primary-600 text-white hover:bg-primary-700 focus:ring-primary-500',
  secondary: 'bg-[var(--bg-tertiary)] text-[var(--text-primary)] hover:opacity-80',
  danger: 'bg-danger text-white hover:bg-red-600 focus:ring-red-500',
  ghost: 'text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]',
  outline: 'border border-[var(--border-color)] text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]',
};
 
const sizeStyles: Record<ButtonSize, string> = {
  sm: 'px-3 py-1.5 text-sm',
  md: 'px-4 py-2 text-sm',
  lg: 'px-6 py-3 text-base',
};
 
export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, className, children, disabled, ...props }, ref) => (
    <button
      ref={ref}
      disabled={disabled || loading}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-lg font-medium transition-all focus:outline-none focus:ring-2 focus:ring-offset-1 disabled:opacity-50 disabled:cursor-not-allowed',
        variantStyles[variant], sizeStyles[size], className
      )}
      {...props}
    >
      {loading && (
        <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
      )}
      {children}
    </button>
  )
);
Button.displayName = 'Button';
 
/* ═══════════════════════════════════════════════════════════
   INPUT
   ═══════════════════════════════════════════════════════════ */
interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}
 
export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className, id, ...props }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s/g, '-');
    return (
      <div className="space-y-1.5">
        {label && (
          <label htmlFor={inputId} className="block text-sm font-medium text-[var(--text-secondary)]">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={cn(
            'w-full rounded-lg border bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:ring-1 focus:ring-primary-500 focus:outline-none transition-colors',
            error ? 'border-danger' : 'border-[var(--border-color)]',
            className
          )}
          {...props}
        />
        {error && <p className="text-xs text-danger">{error}</p>}
      </div>
    );
  }
);
Input.displayName = 'Input';
 
/* ═══════════════════════════════════════════════════════════
   BADGE — FIXED: uses CSS variables for proper light/dark switching
   
   Problem was: hardcoded Tailwind dark: classes with specific color 
   values that didn't respond to the .dark class toggle on <html>.
   
   Fix: use opacity-based backgrounds that naturally adapt, and
   explicit dark: variants with alpha channel backgrounds.
   ═══════════════════════════════════════════════════════════ */
type BadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info';
 
interface BadgeProps {
  children: ReactNode;
  variant?: BadgeVariant;
  className?: string;
}
 
const badgeVariants: Record<BadgeVariant, string> = {
  default: 'bg-[var(--bg-tertiary)] text-[var(--text-secondary)]',
  success: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-400',
  warning: 'bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-400',
  danger: 'bg-red-100 text-red-800 dark:bg-red-500/15 dark:text-red-400',
  info: 'bg-blue-100 text-blue-800 dark:bg-blue-500/15 dark:text-blue-400',
};
 
export function Badge({ children, variant = 'default', className }: BadgeProps) {
  return (
    <span className={cn(
      'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
      badgeVariants[variant],
      className
    )}>
      {children}
    </span>
  );
}
 
/* ═══════════════════════════════════════════════════════════
   CARD
   ═══════════════════════════════════════════════════════════ */
interface CardProps {
  children: ReactNode;
  className?: string;
  title?: string;
  subtitle?: string;
  actions?: ReactNode;
}
 
export function Card({ children, className, title, subtitle, actions }: CardProps) {
  return (
    <div className={cn('rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] shadow-sm', className)}>
      {(title || actions) && (
        <div className="flex items-center justify-between border-b border-[var(--border-color)] px-6 py-4">
          <div>
            {title && <h3 className="text-lg font-semibold text-[var(--text-primary)]">{title}</h3>}
            {subtitle && <p className="mt-0.5 text-sm text-[var(--text-secondary)]">{subtitle}</p>}
          </div>
          {actions && <div className="flex items-center gap-2">{actions}</div>}
        </div>
      )}
      <div className="p-6">{children}</div>
    </div>
  );
}
 
/* ═══════════════════════════════════════════════════════════
   MODAL
   ═══════════════════════════════════════════════════════════ */
interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  size?: 'sm' | 'md' | 'lg';
}
 
const modalSizes = { sm: 'max-w-md', md: 'max-w-lg', lg: 'max-w-2xl' };
 
export function Modal({ isOpen, onClose, title, children, size = 'md' }: ModalProps) {
  if (!isOpen) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className={cn('relative w-full rounded-xl bg-[var(--bg-secondary)] shadow-2xl', modalSizes[size])}>
        <div className="flex items-center justify-between border-b border-[var(--border-color)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">{title}</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
        <div className="max-h-[70vh] overflow-y-auto px-6 py-4">{children}</div>
      </div>
    </div>
  );
}
 
/* ═══════════════════════════════════════════════════════════
   SPINNER
   ═══════════════════════════════════════════════════════════ */
export function Spinner({ className }: { className?: string }) {
  return (
    <div className={cn('flex items-center justify-center py-12', className)}>
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary-200 border-t-primary-600" />
    </div>
  );
}
 
/* ═══════════════════════════════════════════════════════════
   EMPTY STATE
   ═══════════════════════════════════════════════════════════ */
interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
}
 
export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      {icon && <div className="mb-4 text-[var(--text-muted)]">{icon}</div>}
      <h3 className="text-lg font-medium text-[var(--text-primary)]">{title}</h3>
      {description && <p className="mt-1 text-sm text-[var(--text-secondary)]">{description}</p>}
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}