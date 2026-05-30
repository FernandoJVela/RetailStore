import { Badge } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { outcomeVariant } from '@features/audit';
import type { AuditEntry } from '@features/audit';
 
interface AuditRowProps {
  entry: AuditEntry;
  onClick: () => void;
}
 
export function AuditRow({ entry, onClick }: AuditRowProps) {
  return (
    <tr
      onClick={onClick}
      className={cn(
        'cursor-pointer transition-colors',
        entry.isSuccess
          ? 'hover:bg-[var(--bg-tertiary)]/50'
          : 'bg-red-50/30 dark:bg-red-500/5 hover:bg-red-50/60 dark:hover:bg-red-500/10'
      )}
    >
      <td className="px-6 py-3">
        <p className="text-sm font-medium text-[var(--text-primary)]">{entry.actionLabel}</p>
        <p className="text-xs text-[var(--text-muted)]">{entry.description ?? entry.action}</p>
      </td>
      <td className="hidden md:table-cell px-6 py-3">
        <Badge variant="default">{entry.module}</Badge>
      </td>
      <td className="hidden lg:table-cell px-6 py-3 text-sm text-[var(--text-secondary)]">
        {entry.entityType ? (
          <span>{entry.entityType} <span className="font-mono text-xs text-[var(--text-muted)]">{entry.entityIdShort}</span></span>
        ) : '—'}
      </td>
      <td className="hidden sm:table-cell px-6 py-3 text-sm text-[var(--text-secondary)]">{entry.username}</td>
      <td className="px-6 py-3">
        <Badge variant={outcomeVariant(entry.outcome)}>{entry.outcome}</Badge>
      </td>
      <td className="hidden lg:table-cell px-6 py-3 text-xs text-[var(--text-muted)] tabular-nums">{entry.durationLabel}</td>
      <td className="px-6 py-3 text-xs text-[var(--text-muted)] text-right">{entry.timeAgo}</td>
    </tr>
  );
}