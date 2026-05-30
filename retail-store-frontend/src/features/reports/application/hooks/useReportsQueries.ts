import { useQuery } from '@tanstack/react-query';
import { reportsRepository } from '@features/reports/infrastructure/reports.repository';
 
const KEYS = {
  dashboard: ['reports', 'dashboard'] as const,
  dailySales: (days: number) => ['reports', 'daily-sales', days] as const,
  salesByCategory: ['reports', 'sales-by-category'] as const,
  revenueByPeriod: (months: number) => ['reports', 'revenue', months] as const,
  topProducts: (top: number) => ['reports', 'top-products', top] as const,
  topCustomers: (top: number) => ['reports', 'top-customers', top] as const,
  paymentAnalytics: ['reports', 'payment-analytics'] as const,
  inventoryHealth: ['reports', 'inventory-health'] as const,
  shippingPerformance: ['reports', 'shipping-performance'] as const,
};
 
export function useDashboardKpis() {
  return useQuery({
    queryKey: KEYS.dashboard,
    queryFn: () => reportsRepository.getDashboard(),
    staleTime: 30_000,
  });
}
 
export function useDailySales(days = 30) {
  return useQuery({
    queryKey: KEYS.dailySales(days),
    queryFn: () => reportsRepository.getDailySales(days),
    staleTime: 60_000,
  });
}
 
export function useSalesByCategory() {
  return useQuery({
    queryKey: KEYS.salesByCategory,
    queryFn: () => reportsRepository.getSalesByCategory(),
    staleTime: 60_000,
  });
}
 
export function useRevenueByPeriod(months = 12) {
  return useQuery({
    queryKey: KEYS.revenueByPeriod(months),
    queryFn: () => reportsRepository.getRevenueByPeriod(months),
    staleTime: 60_000,
  });
}
 
export function useTopProducts(top = 10) {
  return useQuery({
    queryKey: KEYS.topProducts(top),
    queryFn: () => reportsRepository.getTopProducts(top),
    staleTime: 60_000,
  });
}
 
export function useTopCustomers(top = 20) {
  return useQuery({
    queryKey: KEYS.topCustomers(top),
    queryFn: () => reportsRepository.getTopCustomers(top),
    staleTime: 60_000,
  });
}
 
export function usePaymentAnalytics() {
  return useQuery({
    queryKey: KEYS.paymentAnalytics,
    queryFn: () => reportsRepository.getPaymentAnalytics(),
    staleTime: 60_000,
  });
}
 
export function useInventoryHealth() {
  return useQuery({
    queryKey: KEYS.inventoryHealth,
    queryFn: () => reportsRepository.getInventoryHealth(),
    staleTime: 60_000,
  });
}
 
export function useShippingPerformance() {
  return useQuery({
    queryKey: KEYS.shippingPerformance,
    queryFn: () => reportsRepository.getShippingPerformance(),
    staleTime: 60_000,
  });
}