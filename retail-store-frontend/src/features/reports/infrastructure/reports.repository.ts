import { reportsApi } from '@features/reports';
import type {
  DashboardKpis, DailySales, SalesByCategory, RevenueByPeriod,
  TopProduct, CustomerSpending, PaymentMethodAnalytics,
  InventoryHealth, ShippingPerformance,
} from '@features/reports';
import {
  mapDashboardKpis, mapDailySales, mapSalesByCategory, mapRevenueByPeriod,
  mapTopProduct, mapCustomerSpending, mapPaymentMethod,
  mapInventoryHealth, mapShippingPerformance,
} from '@features/reports/application/mappers/reports.mapper';
 
/** Repository: read-only. No mutations in reports. */
export const reportsRepository = {
  async getDashboard(): Promise<DashboardKpis> {
    const { data } = await reportsApi.getDashboard();
    return mapDashboardKpis(data);
  },
 
  async getDailySales(days = 30): Promise<DailySales[]> {
    const { data } = await reportsApi.getDailySales(days);
    return data.map(mapDailySales);
  },
 
  async getSalesByCategory(): Promise<SalesByCategory[]> {
    const { data } = await reportsApi.getSalesByCategory();
    return data.map(mapSalesByCategory);
  },
 
  async getRevenueByPeriod(months = 12): Promise<RevenueByPeriod[]> {
    const { data } = await reportsApi.getRevenueByPeriod(months);
    return data.map(mapRevenueByPeriod);
  },
 
  async getTopProducts(top = 10): Promise<TopProduct[]> {
    const { data } = await reportsApi.getTopProducts(top);
    return data.map(mapTopProduct);
  },
 
  async getTopCustomers(top = 20): Promise<CustomerSpending[]> {
    const { data } = await reportsApi.getTopCustomers(top);
    return data.map(mapCustomerSpending);
  },
 
  async getPaymentAnalytics(): Promise<PaymentMethodAnalytics[]> {
    const { data } = await reportsApi.getPaymentAnalytics();
    return data.map(mapPaymentMethod);
  },
 
  async getInventoryHealth(): Promise<InventoryHealth[]> {
    const { data } = await reportsApi.getInventoryHealth();
    return data.map(mapInventoryHealth);
  },
 
  async getShippingPerformance(): Promise<ShippingPerformance[]> {
    const { data } = await reportsApi.getShippingPerformance();
    return data.map(mapShippingPerformance);
  },
};