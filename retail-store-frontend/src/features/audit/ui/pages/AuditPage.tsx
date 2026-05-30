import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Shield, Search, AlertTriangle, BarChart3, Users } from 'lucide-react';
import { Card, Spinner, EmptyState, Avatar, PageHeader } from '@shared/components/ui';
import { cn } from '@shared/lib/utils';
import { formatDateTime } from '@shared/lib/utils';
import {
  useAuditSearch, useAuditFailures, useModuleActivity, useUserActivity,
} from '@features/audit/application/hooks/useAuditQueries';
import { AUDIT_MODULES } from '@features/audit';
import { AuditRow } from '@features/audit';
import { AuditDetailPanel } from '@features/audit';

type Tab = 'log' | 'failures' | 'modules' | 'users';

export function AuditPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<Tab>('log');
  const [moduleFilter, setModuleFilter] = useState<string>('');
  const [outcomeFilter, setOutcomeFilter] = useState<string>('');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const tabs: { key: Tab; label: string; icon: typeof Shield }[] = [
    { key: 'log', label: t('audit.tab_log'), icon: Search },
    { key: 'failures', label: t('audit.tab_failures'), icon: AlertTriangle },
    { key: 'modules', label: t('audit.tab_modules'), icon: BarChart3 },
    { key: 'users', label: t('audit.tab_users'), icon: Users },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader title={t('nav.audit')} subtitle={t('audit.subtitle')} />

      {/* Tabs */}
      <div className="flex gap-1 rounded-lg bg-[var(--bg-tertiary)] p-1">
        {tabs.map(({ key, label, icon: Icon }) => (
          <button
            key={key}
            onClick={() => setActiveTab(key)}
            className={cn(
              'flex items-center gap-2 rounded-md px-4 py-2.5 text-sm font-medium transition-colors flex-1 justify-center',
              activeTab === key
                ? 'bg-[var(--bg-secondary)] text-[var(--text-primary)] shadow-sm'
                : 'text-[var(--text-secondary)] hover:text-[var(--text-primary)]'
            )}
          >
            <Icon className="h-4 w-4 hidden sm:block" />
            {label}
          </button>
        ))}
      </div>

      {/* Tab content */}
      {activeTab === 'log' && (
        <LogTab
          moduleFilter={moduleFilter}
          outcomeFilter={outcomeFilter}
          onModuleChange={setModuleFilter}
          onOutcomeChange={setOutcomeFilter}
          onSelect={setSelectedId}
        />
      )}
      {activeTab === 'failures' && <FailuresTab onSelect={setSelectedId} />}
      {activeTab === 'modules' && <ModulesTab />}
      {activeTab === 'users' && <UsersTab />}

      {/* Detail panel */}
      {selectedId && (
        <AuditDetailPanel
          entryId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
// LOG TAB
// ═══════════════════════════════════════════════════════════
function LogTab({ moduleFilter, outcomeFilter, onModuleChange, onOutcomeChange, onSelect }: {
  moduleFilter: string; outcomeFilter: string;
  onModuleChange: (v: string) => void; onOutcomeChange: (v: string) => void;
  onSelect: (id: string) => void;
}) {
  const { t } = useTranslation();
  const { data, isLoading } = useAuditSearch({
    module: moduleFilter || undefined,
    outcome: outcomeFilter || undefined,
    limit: 100,
  });

  return (
    <>
      {/* Filters */}
      <Card>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <select
            value={moduleFilter}
            onChange={(e) => onModuleChange(e.target.value)}
            className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] px-3 py-2 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:outline-none"
          >
            <option value="">{t('audit.allModules')}</option>
            {AUDIT_MODULES.map((m) => <option key={m} value={m}>{m}</option>)}
          </select>
          <select
            value={outcomeFilter}
            onChange={(e) => onOutcomeChange(e.target.value)}
            className="rounded-lg border border-[var(--border-color)] bg-[var(--bg-primary)] px-3 py-2 text-sm text-[var(--text-primary)] focus:border-primary-500 focus:outline-none"
          >
            <option value="">{t('audit.allOutcomes')}</option>
            <option value="Success">{t('audit.outcome_success')}</option>
            <option value="Failure">{t('audit.outcome_failure')}</option>
            <option value="Error">{t('audit.outcome_error')}</option>
          </select>
          <span className="text-xs text-[var(--text-muted)]">{t('audit.entries', { count: data?.length ?? 0 })}</span>
        </div>
      </Card>

      {/* Table */}
      <Card>
        {isLoading ? <Spinner /> : !data?.length ? (
          <EmptyState icon={<Shield className="h-12 w-12" />} title={t('audit.noEntries')} description={t('audit.noEntriesDesc')} />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_action')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_module')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_entity')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_user')}</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_result')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_duration')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_when')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {data.map((entry) => (
                  <AuditRow key={entry.id} entry={entry} onClick={() => onSelect(entry.id)} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </>
  );
}

// ═══════════════════════════════════════════════════════════
// FAILURES TAB
// ═══════════════════════════════════════════════════════════
function FailuresTab({ onSelect }: { onSelect: (id: string) => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useAuditFailures(50);

  return (
    <Card title={t('audit.recentFailures')} subtitle={t('audit.recentFailuresSubtitle')}>
      {isLoading ? <Spinner /> : !data?.length ? (
        <EmptyState icon={<AlertTriangle className="h-12 w-12" />} title={t('audit.noFailures')} description={t('audit.noFailuresDesc')} />
      ) : (
        <div className="overflow-x-auto -mx-6">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--border-color)]">
                <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_action')}</th>
                <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_module')}</th>
                <th className="hidden sm:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_user')}</th>
                <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_result')}</th>
                <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_when')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[var(--border-color)]">
              {data.map((entry) => (
                <AuditRow key={entry.id} entry={entry} onClick={() => onSelect(entry.id)} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════
// MODULES TAB
// ═══════════════════════════════════════════════════════════
function ModulesTab() {
  const { t } = useTranslation();
  const { data, isLoading } = useModuleActivity();

  return (
    <Card title={t('audit.activityByModule')} subtitle={t('audit.activityByModuleSubtitle')}>
      {isLoading ? <Spinner /> : !data?.length ? (
        <EmptyState icon={<BarChart3 className="h-12 w-12" />} title={t('audit.noActivityData')} />
      ) : (
        <div className="overflow-x-auto -mx-6">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--border-color)]">
                <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_module')}</th>
                <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_success')}</th>
                <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_failed')}</th>
                <th className="hidden lg:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_avgTime')}</th>
                <th className="hidden lg:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_maxTime')}</th>
                <th className="hidden sm:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_users')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[var(--border-color)]">
              {data.map((mod) => (
                <tr key={mod.module} className="hover:bg-[var(--bg-tertiary)]/50 transition-colors">
                  <td className="px-6 py-3 font-medium text-[var(--text-primary)]">{mod.module}</td>
                  <td className="px-6 py-3 text-right tabular-nums text-[var(--text-primary)]">{mod.totalActions.toLocaleString()}</td>
                  <td className="px-6 py-3 text-right">
                    <span className={cn('font-semibold tabular-nums',
                      mod.successRate >= 95 ? 'text-emerald-600 dark:text-emerald-400'
                        : mod.successRate >= 80 ? 'text-amber-600 dark:text-amber-400'
                        : 'text-red-600 dark:text-red-400'
                    )}>
                      {mod.successRate}%
                    </span>
                  </td>
                  <td className="hidden md:table-cell px-6 py-3 text-right tabular-nums text-red-600 dark:text-red-400">
                    {mod.failureCount > 0 ? mod.failureCount : '—'}
                  </td>
                  <td className="hidden lg:table-cell px-6 py-3 text-right tabular-nums text-[var(--text-secondary)]">{mod.avgDurationMs}ms</td>
                  <td className="hidden lg:table-cell px-6 py-3 text-right tabular-nums text-[var(--text-secondary)]">{mod.maxDurationMs}ms</td>
                  <td className="hidden sm:table-cell px-6 py-3 text-right tabular-nums text-[var(--text-secondary)]">{mod.uniqueUsers}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════
// USERS TAB
// ═══════════════════════════════════════════════════════════
function UsersTab() {
  const { t } = useTranslation();
  const { data, isLoading } = useUserActivity();

  return (
    <Card title={t('audit.activityByUser')} subtitle={t('audit.activityByUserSubtitle')}>
      {isLoading ? <Spinner /> : !data?.length ? (
        <EmptyState icon={<Users className="h-12 w-12" />} title={t('audit.noUserActivity')} />
      ) : (
        <div className="overflow-x-auto -mx-6">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--border-color)]">
                <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('audit.col_user')}</th>
                <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_failed')}</th>
                <th className="hidden md:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_modules')}</th>
                <th className="hidden sm:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('audit.col_lastActive')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[var(--border-color)]">
              {data.map((user) => (
                <tr key={user.userId ?? user.username} className="hover:bg-[var(--bg-tertiary)]/50 transition-colors">
                  <td className="px-6 py-3">
                    <div className="flex items-center gap-2">
                      <Avatar initials={user.username.charAt(0)} size="sm" />
                      <span className="font-medium text-[var(--text-primary)]">{user.username}</span>
                    </div>
                  </td>
                  <td className="px-6 py-3 text-right tabular-nums text-[var(--text-primary)]">{user.totalActions.toLocaleString()}</td>
                  <td className="px-6 py-3 text-right">
                    {user.failedActions > 0 ? (
                      <span className="tabular-nums text-red-600 dark:text-red-400">{user.failedActions} ({user.failureRate}%)</span>
                    ) : (
                      <span className="text-[var(--text-muted)]">—</span>
                    )}
                  </td>
                  <td className="hidden md:table-cell px-6 py-3 text-right tabular-nums text-[var(--text-secondary)]">{user.modulesAccessed}</td>
                  <td className="hidden sm:table-cell px-6 py-3 text-right text-xs text-[var(--text-muted)]">
                    {user.lastAction ? formatDateTime(user.lastAction) : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}
