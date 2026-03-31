import { httpClient } from '@shared/api/http-client';
import type {
  InventoryItemDto, InventoryDetailDto,
  CreateInventoryItemRequestDto, StockQuantityRequestDto,
  AdjustStockRequestDto, UpdateReorderThresholdRequestDto,
} from './inventory.dto';
 
const BASE = '/inventory';
 
/** Raw API calls. No business logic. No mapping. Just HTTP. */
export const inventoryApi = {
  getAll: (params?: { stockStatus?: string }) =>
    httpClient.get<InventoryItemDto[]>(BASE, { params }),
 
  getByProduct: (productId: string) =>
    httpClient.get<InventoryDetailDto>(`${BASE}/${productId}`),
 
  getLowStock: () =>
    httpClient.get<InventoryItemDto[]>(`${BASE}/low-stock`),
 
  create: (data: CreateInventoryItemRequestDto) =>
    httpClient.post<string>(BASE, data),
 
  addStock: (productId: string, data: StockQuantityRequestDto) =>
    httpClient.put(`${BASE}/${productId}/add-stock`, data),
 
  removeStock: (productId: string, data: StockQuantityRequestDto) =>
    httpClient.put(`${BASE}/${productId}/remove-stock`, data),
 
  reserve: (productId: string, data: StockQuantityRequestDto) =>
    httpClient.put(`${BASE}/${productId}/reserve`, data),
 
  release: (productId: string, data: StockQuantityRequestDto) =>
    httpClient.put(`${BASE}/${productId}/release`, data),
 
  fulfill: (productId: string, data: StockQuantityRequestDto) =>
    httpClient.put(`${BASE}/${productId}/fulfill`, data),
 
  adjust: (productId: string, data: AdjustStockRequestDto) =>
    httpClient.put(`${BASE}/${productId}/adjust`, data),
 
  updateThreshold: (productId: string, data: UpdateReorderThresholdRequestDto) =>
    httpClient.put(`${BASE}/${productId}/reorder-threshold`, data),
};