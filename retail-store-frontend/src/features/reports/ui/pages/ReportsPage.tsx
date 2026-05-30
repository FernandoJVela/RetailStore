import { useTranslation } from 'react-i18next';
import { KpiCards } from '@features/reports';
import { RevenueTrendChart } from '@features/reports';
import { SalesByCategoryChart } from '@features/reports';
import { TopProductsTable } from '@features/reports';
import { TopCustomersTable } from '@features/reports';
import { PaymentMethodsPanel, InventoryHealthPanel, ShippingPerformancePanel } from '@features/reports';
 
export function ReportsPage() {
  const { t } = useTranslation();
 
  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-[var(--text-primary)]">{t('nav.reports')}</h1>
        <p className="mt-1 text-sm text-[var(--text-secondary)]">Business intelligence across all modules</p>
      </div>
 
      {/* KPI Cards */}
      <KpiCards />
 
      {/* Row 1: Revenue trend + Sales by category */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <RevenueTrendChart />
        <SalesByCategoryChart />
      </div>
 
      {/* Row 2: Top products + Top customers */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <TopProductsTable />
        <TopCustomersTable />
      </div>
 
      {/* Row 3: Payment + Inventory + Shipping */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <PaymentMethodsPanel />
        <InventoryHealthPanel />
        <ShippingPerformancePanel />
      </div>
    </div>
  );
}