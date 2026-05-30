import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Truck, Package, CheckCircle, Clock } from 'lucide-react';
import { Card, Spinner, EmptyState, StatCard, FilterPillBar, PageHeader } from '@shared/components/ui';
import { useShipments } from '@features/shipping/application/hooks/useShippingQueries';
import { ShipmentRow } from '@features/shipping/ui/components/ShipmentRow';
import { ShipmentDetailPanel } from '@features/shipping/ui/components/ShipmentDetailPanel';
import type { ShipmentStatus } from '@features/shipping';

type FilterStatus = 'all' | ShipmentStatus;

export function ShippingListPage() {
  const { t } = useTranslation();
  const [statusFilter, setStatusFilter] = useState<FilterStatus>('all');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const filters: { key: FilterStatus; label: string }[] = [
    { key: 'all', label: t('shipping.status_all') },
    { key: 'Pending', label: t('shipping.status_pending') },
    { key: 'Processing', label: t('shipping.status_processing') },
    { key: 'Shipped', label: t('shipping.status_shipped') },
    { key: 'InTransit', label: t('shipping.status_inTransit') },
    { key: 'Delivered', label: t('shipping.status_delivered') },
    { key: 'Failed', label: t('shipping.status_failed') },
  ];

  const { data: shipments, isLoading } = useShipments({
    status: statusFilter === 'all' ? undefined : statusFilter,
  });

  const total = shipments?.length ?? 0;
  const pendingCount = shipments?.filter((s) => s.status === 'Pending' || s.status === 'Processing').length ?? 0;
  const inTransitCount = shipments?.filter((s) => s.status === 'Shipped' || s.status === 'InTransit').length ?? 0;
  const deliveredCount = shipments?.filter((s) => s.status === 'Delivered').length ?? 0;

  const summaryCards = [
    { labelKey: 'shipping.totalShipments', value: total, icon: Package, iconColor: 'text-primary-600 dark:text-primary-400', iconBg: 'bg-primary-100 dark:bg-primary-500/15' },
    { labelKey: 'orders.status_pending', value: pendingCount, icon: Clock, iconColor: 'text-amber-600 dark:text-amber-400', iconBg: 'bg-amber-100 dark:bg-amber-500/15' },
    { labelKey: 'shipping.inTransit', value: inTransitCount, icon: Truck, iconColor: 'text-blue-600 dark:text-blue-400', iconBg: 'bg-blue-100 dark:bg-blue-500/15' },
    { labelKey: 'shipping.delivered', value: deliveredCount, icon: CheckCircle, iconColor: 'text-emerald-600 dark:text-emerald-400', iconBg: 'bg-emerald-100 dark:bg-emerald-500/15' },
  ];

  return (
    <div className="space-y-6">
      <PageHeader title={t('nav.shipping')} subtitle={t('shipping.subtitle')} />

      {/* Summary cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {summaryCards.map(({ labelKey, value, icon, iconColor, iconBg }) => (
          <StatCard
            key={labelKey}
            label={t(labelKey)}
            value={value}
            icon={icon}
            iconColor={iconColor}
            iconBg={iconBg}
          />
        ))}
      </div>

      {/* Filters */}
      <Card>
        <FilterPillBar
          options={filters}
          value={statusFilter}
          onChange={(v) => setStatusFilter(v as typeof statusFilter)}
        />
      </Card>

      {/* Table */}
      <Card>
        {isLoading ? (
          <Spinner />
        ) : !shipments?.length ? (
          <EmptyState
            icon={<Truck className="h-12 w-12" />}
            title={t('shipping.noShipmentsFound')}
            description={t('shipping.noShipmentsDesc')}
          />
        ) : (
          <div className="overflow-x-auto -mx-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--border-color)]">
                  <th className="px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('shipping.col_shipment')}</th>
                  <th className="hidden md:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('shipping.col_carrier')}</th>
                  <th className="hidden lg:table-cell px-6 pb-3 text-left font-medium text-[var(--text-secondary)]">{t('shipping.col_tracking')}</th>
                  <th className="px-6 pb-3 text-center font-medium text-[var(--text-secondary)]">{t('shipping.col_items')}</th>
                  <th className="hidden sm:table-cell px-6 pb-3 text-right font-medium text-[var(--text-secondary)]">{t('shipping.col_cost')}</th>
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
