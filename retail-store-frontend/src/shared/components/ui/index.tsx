import {
  forwardRef, useState, useRef, useEffect,
  type ButtonHTMLAttributes, type InputHTMLAttributes, type TextareaHTMLAttributes,
  type SelectHTMLAttributes, type ReactNode, type ComponentType,
} from 'react';
import { cn, formatDateTime } from '@shared/lib/utils';
import { X, MoreHorizontal, Search, Pencil } from 'lucide-react';
 
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

/* ═══════════════════════════════════════════════════════════
   STAT CARD
   ═══════════════════════════════════════════════════════════ */
export interface StatCardProps {
  label: string;
  value: string | number;
  sub?: string;
  icon: ComponentType<{ className?: string }>;
  iconColor: string;
  iconBg: string;
  onClick?: () => void;
}

export function StatCard({ label, value, sub, icon: Icon, iconColor, iconBg, onClick }: StatCardProps) {
  const body = (
    <>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-[var(--text-secondary)]">{label}</p>
          <p className="mt-1 text-2xl font-bold text-[var(--text-primary)] tabular-nums">{value}</p>
        </div>
        <div className={cn('flex h-10 w-10 shrink-0 items-center justify-center rounded-lg', iconBg)}>
          <Icon className={cn('h-5 w-5', iconColor)} />
        </div>
      </div>
      {sub && <p className="mt-1.5 text-xs text-[var(--text-muted)]">{sub}</p>}
    </>
  );

  if (onClick) {
    return (
      <button
        onClick={onClick}
        className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4 text-left hover:border-primary-300 dark:hover:border-primary-700 hover:shadow-sm transition-all w-full"
      >
        {body}
      </button>
    );
  }

  return (
    <div className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
      {body}
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   SLIDE PANEL
   ═══════════════════════════════════════════════════════════ */
interface SlidePanelProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  size?: 'md' | 'lg';
}

const slidePanelSizes = { md: 'max-w-lg', lg: 'max-w-xl' };

export function SlidePanel({ isOpen, onClose, title, children, size = 'md' }: SlidePanelProps) {
  if (!isOpen) return null;
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className={cn('fixed inset-y-0 right-0 z-50 w-full overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl', slidePanelSizes[size])}>
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">{title}</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
        {children}
      </div>
    </>
  );
}

/* ═══════════════════════════════════════════════════════════
   ACTION MENU
   ═══════════════════════════════════════════════════════════ */
export interface ActionMenuItem {
  label: string;
  icon?: ComponentType<{ className?: string }>;
  onClick: () => void;
  variant?: 'default' | 'danger' | 'success';
  iconColor?: string;
  separator?: boolean;
}

interface ActionMenuProps {
  items: ActionMenuItem[];
}

const actionMenuItemStyles: Record<Required<ActionMenuItem>['variant'], string> = {
  default: 'text-[var(--text-primary)] hover:bg-[var(--bg-tertiary)]',
  danger:  'text-red-600 hover:bg-red-50 dark:hover:bg-red-500/10',
  success: 'text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-500/10',
};

export function ActionMenu({ items }: ActionMenuProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  return (
    <div className="relative" ref={ref}>
      <button
        onClick={() => setOpen((prev) => !prev)}
        className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)] transition-colors"
      >
        <MoreHorizontal className="h-5 w-5" />
      </button>
      {open && (
        <div className="absolute right-0 top-full z-10 mt-1 min-w-[11rem] rounded-lg border border-[var(--border-color)] bg-[var(--bg-secondary)] py-1 shadow-lg">
          {items.map((item, idx) => (
            <div key={idx}>
              {item.separator && <div className="my-1 border-t border-[var(--border-color)]" />}
              <button
                onClick={() => { setOpen(false); item.onClick(); }}
                className={cn(
                  'flex w-full items-center gap-2 px-4 py-2.5 text-sm',
                  actionMenuItemStyles[item.variant ?? 'default']
                )}
              >
                {item.icon && <item.icon className={cn('h-4 w-4 shrink-0', item.iconColor)} />}
                {item.label}
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   SEARCH INPUT
   ═══════════════════════════════════════════════════════════ */
interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
}

export function SearchInput({ value, onChange, placeholder, className }: SearchInputProps) {
  return (
    <div className={cn('relative flex-1', className)}>
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--text-muted)]" />
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] py-2.5 pl-10 pr-4 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500"
      />
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   FILTER PILL BAR
   ═══════════════════════════════════════════════════════════ */
export interface FilterPillOption {
  key: string;
  label: string;
}

interface FilterPillBarProps {
  options: FilterPillOption[];
  value: string;
  onChange: (value: string) => void;
  className?: string;
}

export function FilterPillBar({ options, value, onChange, className }: FilterPillBarProps) {
  return (
    <div className={cn('flex flex-wrap gap-2', className)}>
      {options.map(({ key, label }) => (
        <button
          key={key}
          onClick={() => onChange(key)}
          className={cn(
            'rounded-lg px-3 py-2 text-sm font-medium transition-colors',
            value === key
              ? 'bg-primary-600 text-white'
              : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]'
          )}
        >
          {label}
        </button>
      ))}
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   TEXTAREA
   ═══════════════════════════════════════════════════════════ */
interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ label, error, className, id, ...props }, ref) => {
    const textareaId = id || label?.toLowerCase().replace(/\s+/g, '-');
    return (
      <div className="space-y-1.5">
        {label && (
          <label htmlFor={textareaId} className="block text-sm font-medium text-[var(--text-secondary)]">
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          id={textareaId}
          className={cn(
            'w-full rounded-lg border bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] placeholder:text-[var(--text-muted)] focus:border-primary-500 focus:ring-1 focus:ring-primary-500 focus:outline-none resize-none',
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
Textarea.displayName = 'Textarea';

/* ═══════════════════════════════════════════════════════════
   SELECT
   ═══════════════════════════════════════════════════════════ */
interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, error, className, id, children, ...props }, ref) => {
    const selectId = id ?? label?.toLowerCase().replace(/\s+/g, '-');

    const selectEl = (
      <select
        ref={ref}
        id={selectId}
        className={cn(
          'w-full rounded-lg border bg-[var(--bg-secondary)] px-3.5 py-2.5 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:ring-1 focus:ring-primary-500 focus:outline-none',
          error ? 'border-danger' : 'border-[var(--border-color)]',
          className
        )}
        {...props}
      >
        {children}
      </select>
    );

    if (!label) return selectEl;

    return (
      <div className="space-y-1.5">
        <label htmlFor={selectId} className="block text-sm font-medium text-[var(--text-secondary)]">
          {label}
        </label>
        {selectEl}
        {error && <p className="text-xs text-danger">{error}</p>}
      </div>
    );
  }
);
Select.displayName = 'Select';

/* ═══════════════════════════════════════════════════════════
   TIMELINE
   ═══════════════════════════════════════════════════════════ */
export interface TimelineEvent {
  label: string;
  date: string | Date | null;
  icon: ComponentType<{ className?: string }>;
  iconColor?: string;
}

interface TimelineProps {
  events: TimelineEvent[];
}

export function Timeline({ events }: TimelineProps) {
  return (
    <div className="space-y-3">
      {events.map((event, idx) => (
        <div key={idx} className="flex items-center gap-3">
          <event.icon className={cn('h-4 w-4 shrink-0', event.iconColor ?? 'text-[var(--text-muted)]')} />
          <span className="text-sm font-medium text-[var(--text-primary)] flex-1">{event.label}</span>
          {event.date && (
            <span className="text-xs text-[var(--text-muted)] tabular-nums shrink-0">
              {formatDateTime(event.date)}
            </span>
          )}
        </div>
      ))}
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   AVATAR
   ═══════════════════════════════════════════════════════════ */
interface AvatarProps {
  initials: string;
  size?: 'sm' | 'md' | 'lg';
  variant?: 'primary' | 'amber';
  shape?: 'circle' | 'square';
  className?: string;
}

const avatarSizes = {
  sm: 'h-8 w-8 text-xs',
  md: 'h-9 w-9 text-sm',
  lg: 'h-12 w-12 text-lg',
};

const avatarVariants = {
  primary: 'bg-primary-100 dark:bg-primary-500/15 text-primary-700 dark:text-primary-400',
  amber:   'bg-amber-100 dark:bg-amber-500/15 text-amber-700 dark:text-amber-400',
};

export function Avatar({ initials, size = 'md', variant = 'primary', shape = 'circle', className }: AvatarProps) {
  return (
    <div
      className={cn(
        'flex shrink-0 items-center justify-center font-bold',
        avatarSizes[size],
        avatarVariants[variant],
        shape === 'circle' ? 'rounded-full' : 'rounded-lg',
        className
      )}
    >
      {initials.toUpperCase()}
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   ALERT
   ═══════════════════════════════════════════════════════════ */
interface AlertProps {
  message: string;
  variant?: 'error' | 'warning' | 'success' | 'info';
  className?: string;
}

const alertVariants = {
  error:   'bg-red-50 dark:bg-red-500/10 border-red-200 dark:border-red-800 text-red-700 dark:text-red-400',
  warning: 'bg-amber-50 dark:bg-amber-500/10 border-amber-200 dark:border-amber-800 text-amber-700 dark:text-amber-400',
  success: 'bg-emerald-50 dark:bg-emerald-500/10 border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400',
  info:    'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-800 text-blue-700 dark:text-blue-400',
};

export function Alert({ message, variant = 'error', className }: AlertProps) {
  return (
    <div className={cn('rounded-lg border p-3', alertVariants[variant], className)}>
      <p className="text-sm">{message}</p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   TOGGLE
   ═══════════════════════════════════════════════════════════ */
interface ToggleProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  className?: string;
}

/* ═══════════════════════════════════════════════════════════
   DETAIL SECTION
   ═══════════════════════════════════════════════════════════ */
interface DetailSectionProps {
  title: string;
  icon?: ComponentType<{ className?: string }>;
  onEdit?: () => void;
  children: ReactNode;
  className?: string;
}

export function DetailSection({ title, icon: Icon, onEdit, children, className }: DetailSectionProps) {
  return (
    <section className={cn('rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5', className)}>
      {onEdit ? (
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider flex items-center gap-2">
            {Icon && <Icon className="h-4 w-4" />}
            {title}
          </h3>
          <button onClick={onEdit} className="text-primary-600 hover:text-primary-500">
            <Pencil className="h-4 w-4" />
          </button>
        </div>
      ) : (
        <h3 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3 flex items-center gap-2">
          {Icon && <Icon className="h-4 w-4" />}
          {title}
        </h3>
      )}
      {children}
    </section>
  );
}

/* ═══════════════════════════════════════════════════════════
   PAGE HEADER
   ═══════════════════════════════════════════════════════════ */
interface PageHeaderProps {
  title: string;
  subtitle?: string;
  action?: ReactNode;
}

export function PageHeader({ title, subtitle, action }: PageHeaderProps) {
  return (
    <div className={action ? 'flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between' : undefined}>
      <div>
        <h1 className="text-2xl font-bold text-[var(--text-primary)]">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-[var(--text-secondary)]">{subtitle}</p>}
      </div>
      {action}
    </div>
  );
}

export function Toggle({ checked, onChange, disabled, className }: ToggleProps) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      disabled={disabled}
      onClick={() => onChange(!checked)}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors',
        'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
        checked ? 'bg-primary-600' : 'bg-[var(--bg-tertiary)]',
        disabled && 'cursor-not-allowed opacity-50',
        className
      )}
    >
      <span
        className={cn(
          'pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow transition-transform',
          checked ? 'translate-x-5' : 'translate-x-0'
        )}
      />
    </button>
  );
}