import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Truck, Package, CheckCircle, Clock } from 'lucide-react';
import { Card, Spinner, EmptyState } from '@shared/components/ui';
import { useShipments } from '@features/shipping/application/hooks/useShippingQueries';
import { ShipmentRow } from '@features/shipping/ui/components/ShipmentRow';
import { ShipmentDetailPanel } from '@features/shipping/ui/components/ShipmentDetailPanel';
import type { ShipmentStatus } from '@features/shipping';
 
type FilterStatus = 'all' | ShipmentStatus;
 
const FILTERS: { key: FilterStatus; label: string }[] = [
  { key: 'all', label: 'All' },
  { key: 'Pending', label: 'Pending' },
  { key: 'Processing', label: 'Processing' },
  { key: 'Shipped', label: 'Shipped' },
  { key: 'InTransit', label: 'In Transit' },
  { key: 'Delivered', label: 'Delivered' },
  { key: 'Failed', label: 'Failed' },
];
 
export function ShippingListPage() {
  const { t } = useTranslation();
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [selectedId, setSelectedId] = useState<string | null>(null);
 
  const { data: shipments, isLoading } = useShipments({
    status: statusFilter === 'all' ? undefined : statusFilter,
  });
 
  const total = shipments?.length ?? 0;
  const pendingCount = shipments?.filter((s) => s.status === 'Pending' || s.status === 'Processing').length ?? 0;
  const inTransitCount = shipments?.filter((s) => s.status === 'Shipped' || s.status === 'InTransit').length ?? 0;
  const deliveredCount = shipments?.filter((s) => s.status === 'Delivered').length ?? 0;
 
  const summaryCards = [
    { label: 'Total Shipments', value: total, icon: Package, color: 'text-primary-600 dark:text-primary-400', bg: 'bg-primary-100 dark:bg-primary-500/15' },
    { label: 'Pending', value: pendingCount, icon: Clock, color: 'text-amber-600 dark:text-amber-400', bg: 'bg-amber-100 dark:bg-amber-500/15' },
    { label: 'In Transit', value: inTransitCount, icon: Truck, color: 'text-blue-600 dark:text-blue-400', bg: 'bg-blue-100 dark:bg-blue-500/15' },
    { label: 'Delivered', value: deliveredCount, icon: CheckCircle, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-emerald-100 dark:bg-emerald-500/15' },
  ];
 
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.shipping')}</h1>
        <p className="mt-1 text-sm text-[var(--text-secondary)]">Track shipments, assign carriers, and manage deliveries</p>
      </div>
 
      {/* Summary cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {summaryCards.map(({ label, value, icon: Icon, color, bg }) => (
          <div key={label} className="rounded-xl border border-[var(--border-color)] bg-[var(--bg-secondary)] p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-[var(--text-secondary)]">{label}</p>
                <p className="mt-1 text-2xl font-bold text-[var(--text-primary)]">{value}</p>
              </div>
              <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${bg}`}>
                <Icon className={`h-5 w-5 ${color}`} />
              </div>
            </div>
          </div>
        ))}
      </div>
 
      {/* Filters */}
      <Card>
        <div className="flex flex-wrap gap-2">
          {FILTERS.map(({ key, label }) => (
            <button
              key={key}
              onClick={() => setStatusFilter(key)}
              className={`rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                statusFilter === key
                  ? 'bg-primary-600 text-white'
                  : 'bg-[var(--bg-primary)] text-[var(--text-secondary)] hover:bg-[var(--bg-tertiary)]'
              }`}
            >
              {label}
            </button>
          ))}
        </div>
      </Card>
 
      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !shipments?.length ? (
          <EmptyState
            icon={<Truck className="h-12 w-12" />}
            title="No shipments found"
            description="Shipments appear here when orders are confirmed."
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Shipment</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Carrier</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">Tracking</th>
                  <th className="px-6 pb-3 text-center font-medium text-[var(--text-secondary)]">Items</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">Cost</th>
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('common.status')}</th>
                  <th className="px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--border-color)]">
                {shipments.map((shipment) => (
                  <ShipmentRow
                    key={shipment.id}
                    shipment={shipment}
                    onViewDetail={() => setSelectedId(shipment.id)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
 
      {selectedId && (
        <ShipmentDetailPanel
          shipmentId={selectedId}
          isOpen={!!selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  );
}