export { reportsApi } from './api/reports.api';
export type * from './api/reports.dto';
export type * from './domain/reports.model';
export { ReportsPage } from './ui/pages/ReportsPage';
export {
  useDashboardKpis, useSalesByCategory, useRevenueByPeriod,
  useTopProducts, useTopCustomers, usePaymentAnalytics,
  useInventoryHealth, useShippingPerformance,
} from './application/hooks/useReportsQueries';
export type {
  DashboardKpis, SalesByCategory, RevenueByPeriod,
  TopProduct, CustomerSpending, PaymentMethodAnalytics,
  InventoryHealth, ShippingPerformance,
} from './domain/reports.model';
export { KpiCards } from './ui/components/KpiCards';
export { 
    PaymentMethodsPanel, 
    InventoryHealthPanel, 
    ShippingPerformancePanel } from './ui/components/AnalyticsPanels';
export { RevenueTrendChart } from './ui/components/RevenueTrendChart';
export { SalesByCategoryChart } from './ui/components/SalesByCategoryChart';
export { TopCustomersTable } from './ui/components/TopCustomersTable';
export { TopProductsTable } from './ui/components/TopProductsTable';