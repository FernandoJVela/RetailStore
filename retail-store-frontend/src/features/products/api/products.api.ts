import { httpClient } from '@shared/api/http-client';
import type {
  ProductDto, ProductDetailDto,
  CreateProductRequestDto, UpdateProductDetailsRequestDto,
  UpdateProductPriceRequestDto,
} from './products.dto';
 
const BASE = '/products';
 
/** Raw API calls. No business logic. No mapping. Just HTTP. */
export const productsApi = {
  getAll: (params?: { category?: string; isActive?: boolean; search?: string }) =>
    httpClient.get<ProductDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<ProductDetailDto>(`${BASE}/${id}`),
 
  getByCategory: (category: string) =>
    httpClient.get<ProductDto[]>(`${BASE}/category/${category}`),
 
  create: (data: CreateProductRequestDto) =>
    httpClient.post<string>(BASE, data),
 
  updateDetails: (id: string, data: UpdateProductDetailsRequestDto) =>
    httpClient.put(`${BASE}/${id}`, data),
 
  updatePrice: (id: string, data: UpdateProductPriceRequestDto) =>
    httpClient.put(`${BASE}/${id}/price`, data),
 
  deactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/deactivate`),
 
  reactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/reactivate`),
};