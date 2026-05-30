import { httpClient } from '@shared/api/http-client';
import type {
  DashboardKpiDto, DailySalesDto, SalesByCategoryDto,
  RevenueByPeriodDto, TopProductDto, CustomerSpendingDto,
  PaymentMethodDto, InventoryHealthDto, ShippingPerformanceDto,
} from './reports.dto';
 
const BASE = '/reports';
 
export const reportsApi = {
  getDashboard: () =>
    httpClient.get<DashboardKpiDto>(`${BASE}/dashboard`),
 
  getDailySales: (days = 30) =>
    httpClient.get<DailySalesDto[]>(`${BASE}/sales/daily`, { params: { days } }),
 
  getSalesByCategory: () =>
    httpClient.get<SalesByCategoryDto[]>(`${BASE}/sales/by-category`),
 
  getRevenueByPeriod: (months = 12) =>
    httpClient.get<RevenueByPeriodDto[]>(`${BASE}/sales/revenue-by-period`, { params: { months } }),
 
  getTopProducts: (top = 10) =>
    httpClient.get<TopProductDto[]>(`${BASE}/products/top-selling`, { params: { top } }),
 
  getTopCustomers: (top = 20) =>
    httpClient.get<CustomerSpendingDto[]>(`${BASE}/customers/top-spending`, { params: { top } }),
 
  getPaymentAnalytics: () =>
    httpClient.get<PaymentMethodDto[]>(`${BASE}/payments/methods`),
 
  getInventoryHealth: () =>
    httpClient.get<InventoryHealthDto[]>(`${BASE}/inventory/health`),
 
  getShippingPerformance: () =>
    httpClient.get<ShippingPerformanceDto[]>(`${BASE}/shipping/performance`),
};