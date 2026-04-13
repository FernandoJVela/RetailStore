import { httpClient } from '@shared/api/http-client';
import type {
  ProviderDto, ProviderDetailDto,
  RegisterProviderRequestDto, UpdateProviderRequestDto,
  ChangeProviderEmailRequestDto,
} from './providers.dto';
 
const BASE = '/providers';
 
/** Raw API calls. No business logic. No mapping. Just HTTP. */
export const providersApi = {
  getAll: (params?: { search?: string; isActive?: boolean }) =>
    httpClient.get<ProviderDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<ProviderDetailDto>(`${BASE}/${id}`),
 
  getByProduct: (productId: string) =>
    httpClient.get<ProviderDto[]>(`${BASE}/by-product/${productId}`),
 
  register: (data: RegisterProviderRequestDto) =>
    httpClient.post<string>(BASE, data),
 
  update: (id: string, data: UpdateProviderRequestDto) =>
    httpClient.put(`${BASE}/${id}`, data),
 
  changeEmail: (id: string, data: ChangeProviderEmailRequestDto) =>
    httpClient.put(`${BASE}/${id}/email`, data),
 
  associateProduct: (id: string, productId: string) =>
    httpClient.post(`${BASE}/${id}/products/${productId}`),
 
  dissociateProduct: (id: string, productId: string) =>
    httpClient.delete(`${BASE}/${id}/products/${productId}`),
 
  deactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/deactivate`),
 
  reactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/reactivate`),
};