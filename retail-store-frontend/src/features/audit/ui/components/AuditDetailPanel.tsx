import { X, AlertTriangle, ArrowRight, Globe, Fingerprint } from 'lucide-react';
import { Badge, Spinner } from '@shared/components/ui';
import { formatDateTime } from '@shared/lib/utils';
import { useAuditDetail } from '@features/audit/application/hooks/useAuditQueries';
import { outcomeVariant } from '@features/audit';
 
interface AuditDetailPanelProps {
  entryId: string;
  isOpen: boolean;
  onClose: () => void;
}
 
export function AuditDetailPanel({ entryId, isOpen, onClose }: AuditDetailPanelProps) {
  const { data: entry, isLoading } = useAuditDetail(entryId);
 
  if (!isOpen) return null;
 
  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed inset-y-0 right-0 z-50 w-full max-w-xl overflow-y-auto bg-[var(--bg-secondary)] shadow-2xl">
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-[var(--border-color)] bg-[var(--bg-secondary)] px-6 py-4">
          <h2 className="text-lg font-semibold text-[var(--text-primary)]">Audit Entry</h2>
          <button onClick={onClose} className="rounded-lg p-1.5 text-[var(--text-muted)] hover:bg-[var(--bg-tertiary)]">
            <X className="h-5 w-5" />
          </button>
        </div>
 
        {isLoading ? (
          <Spinner />
        ) : entry ? (
          <div className="p-6 space-y-6">
 
            {/* ─── Header ────────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <div className="flex flex-wrap items-center gap-2 mb-3">
                <Badge variant={outcomeVariant(entry.outcome)}>{entry.outcome}</Badge>
                <Badge variant="default">{entry.module}</Badge>
                <span className="text-xs text-[var(--text-muted)] tabular-nums">{entry.durationLabel}</span>
              </div>
              <h3 className="text-lg font-semibold text-[var(--text-primary)]">{entry.actionLabel}</h3>
              {entry.description && (
                <p className="mt-1 text-sm text-[var(--text-secondary)]">{entry.description}</p>
              )}
              <p className="mt-2 text-xs text-[var(--text-muted)]">{formatDateTime(entry.timestamp)}</p>
            </section>
 
            {/* ─── Who ───────────────────────────────────── */}
            <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
              <h4 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Who</h4>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2 text-[var(--text-secondary)]">
                  <Fingerprint className="h-4 w-4 shrink-0 text-[var(--text-muted)]" />
                  <span className="font-medium text-[var(--text-primary)]">{entry.username}</span>
                  {entry.userId && <span className="font-mono text-xs text-[var(--text-muted)]">{entry.userId.substring(0, 8)}</span>}
                </div>
                {entry.ipAddress && (
                  <div className="flex items-center gap-2 text-[var(--text-secondary)]">
                    <Globe className="h-4 w-4 shrink-0 text-[var(--text-muted)]" />
                    <span className="font-mono text-xs">{entry.ipAddress}</span>
                  </div>
                )}
              </div>
            </section>
 
            {/* ─── What ──────────────────────────────────── */}
            {entry.entityType && (
              <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
                <h4 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Entity</h4>
                <div className="text-sm text-[var(--text-secondary)]">
                  <span className="font-medium text-[var(--text-primary)]">{entry.entityType}</span>
                  {entry.entityId && <span className="ml-2 font-mono text-xs text-[var(--text-muted)]">{entry.entityId}</span>}
                </div>
                {entry.responseSummary && (
                  <p className="mt-1 text-xs text-[var(--text-muted)]">{entry.responseSummary}</p>
                )}
              </section>
            )}
 
            {/* ─── Property Changes ──────────────────────── */}
            {entry.changedProperties && entry.changedProperties.length > 0 && (
              <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
                <h4 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Changes</h4>
                <div className="space-y-2">
                  {entry.changedProperties.map((change, idx) => (
                    <div key={idx} className="flex items-center gap-2 rounded-lg bg-[var(--bg-secondary)] border border-[var(--border-color)] px-3 py-2 text-sm">
                      <span className="font-medium text-[var(--text-primary)] min-w-[100px]">{change.Property}</span>
                      <span className="text-red-600 dark:text-red-400 line-through font-mono text-xs">{change.OldValue}</span>
                      <ArrowRight className="h-3 w-3 shrink-0 text-[var(--text-muted)]" />
                      <span className="text-emerald-600 dark:text-emerald-400 font-mono text-xs">{change.NewValue}</span>
                    </div>
                  ))}
                </div>
              </section>
            )}
 
            {/* ─── Request Payload ────────────────────────── */}
            {entry.requestPayload && (
              <section className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] p-5">
                <h4 className="text-sm font-semibold text-[var(--text-primary)] uppercase tracking-wider mb-3">Request Payload</h4>
                <pre className="text-xs font-mono text-[var(--text-secondary)] bg-[var(--bg-secondary)] border border-[var(--border-color)] rounded-lg p-3 overflow-x-auto max-h-60">
                  {JSON.stringify(entry.requestPayload, null, 2)}
                </pre>
              </section>
            )}
 
            {/* ─── Error Details ──────────────────────────── */}
            {!entry.isSuccess && (
              <section className="rounded-lg bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-800 p-5">
                <div className="flex items-start gap-2">
                  <AlertTriangle className="h-4 w-4 text-red-500 mt-0.5 shrink-0" />
                  <div>
                    {entry.errorCode && (
                      <p className="text-sm font-mono font-medium text-red-800 dark:text-red-400">{entry.errorCode}</p>
                    )}
                    {entry.errorMessage && (
                      <p className="text-sm text-red-700 dark:text-red-400 mt-0.5">{entry.errorMessage}</p>
                    )}
                  </div>
                </div>
              </section>
            )}
 
            {/* ─── Correlation ────────────────────────────── */}
            {(entry.correlationId || entry.requestId) && (
              <div className="text-xs text-[var(--text-muted)] space-y-1 font-mono">
                {entry.correlationId && <p>Correlation: {entry.correlationId}</p>}
                {entry.requestId && <p>Request: {entry.requestId}</p>}
              </div>
            )}
          </div>
        ) : null}
      </div>
    </>
  );
}