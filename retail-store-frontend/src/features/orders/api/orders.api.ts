import { httpClient } from '@shared/api/http-client';
import type {
  OrderDto, OrderDetailDto,
  CreateOrderRequestDto, AddOrderItemRequestDto, CancelOrderRequestDto,
} from './orders.dto';
 
const BASE = '/orders';
 
export const ordersApi = {
  getAll: (params?: { status?: string }) =>
    httpClient.get<OrderDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<OrderDetailDto>(`${BASE}/${id}`),
 
  create: (data: CreateOrderRequestDto) =>
    httpClient.post<string>(BASE, data),
 
  addItem: (orderId: string, data: AddOrderItemRequestDto) =>
    httpClient.post(`${BASE}/${orderId}/items`, data),
 
  removeItem: (orderId: string, productId: string) =>
    httpClient.delete(`${BASE}/${orderId}/items/${productId}`),
 
  confirm: (id: string) =>
    httpClient.put(`${BASE}/${id}/confirm`),
 
  complete: (id: string) =>
    httpClient.put(`${BASE}/${id}/complete`),
 
  cancel: (id: string, data: CancelOrderRequestDto) =>
    httpClient.put(`${BASE}/${id}/cancel`, data),
};