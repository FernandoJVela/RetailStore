import { httpClient } from '@shared/api/http-client';
import type {
  CustomerDto, CustomerDetailDto,
  RegisterCustomerRequestDto, UpdateCustomerRequestDto,
  ChangeEmailRequestDto, UpdateShippingAddressRequestDto,
} from './customers.dto';
 
const BASE = '/customers';
 
/** Raw API calls. No business logic. No mapping. Just HTTP. */
export const customersApi = {
  getAll: (params?: { search?: string; isActive?: boolean }) =>
    httpClient.get<CustomerDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<CustomerDetailDto>(`${BASE}/${id}`),
 
  getByEmail: (email: string) =>
    httpClient.get<CustomerDetailDto>(`${BASE}/by-email/${email}`),
 
  register: (data: RegisterCustomerRequestDto) =>
    httpClient.post<string>(BASE, data),
 
  update: (id: string, data: UpdateCustomerRequestDto) =>
    httpClient.put(`${BASE}/${id}`, data),
 
  changeEmail: (id: string, data: ChangeEmailRequestDto) =>
    httpClient.put(`${BASE}/${id}/email`, data),
 
  updateAddress: (id: string, data: UpdateShippingAddressRequestDto) =>
    httpClient.put(`${BASE}/${id}/address`, data),
 
  deactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/deactivate`),
 
  reactivate: (id: string) =>
    httpClient.put(`${BASE}/${id}/reactivate`),
};