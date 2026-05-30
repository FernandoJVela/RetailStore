import { httpClient } from '@shared/api/http-client';
import type {
  ShipmentDto, ShipmentDetailDto,
  AssignCarrierRequestDto, SetShippingCostRequestDto, FailReasonRequestDto,
} from './shipping.dto';
 
const BASE = '/shipments';
 
export const shippingApi = {
  getAll: (params?: { status?: string; customerId?: string }) =>
    httpClient.get<ShipmentDto[]>(BASE, { params }),
 
  getById: (id: string) =>
    httpClient.get<ShipmentDetailDto>(`${BASE}/${id}`),
 
  getByOrder: (orderId: string) =>
    httpClient.get<ShipmentDetailDto>(`${BASE}/by-order/${orderId}`),
 
  track: (trackingNumber: string) =>
    httpClient.get<ShipmentDetailDto>(`${BASE}/track/${trackingNumber}`),
 
  create: (orderId: string) =>
    httpClient.post<string>(BASE, { orderId }),
 
  assignCarrier: (id: string, data: AssignCarrierRequestDto) =>
    httpClient.put(`${BASE}/${id}/carrier`, data),
 
  setCost: (id: string, data: SetShippingCostRequestDto) =>
    httpClient.put(`${BASE}/${id}/cost`, data),
 
  markShipped: (id: string) =>
    httpClient.put(`${BASE}/${id}/ship`),
 
  markInTransit: (id: string) =>
    httpClient.put(`${BASE}/${id}/in-transit`),
 
  markDelivered: (id: string) =>
    httpClient.put(`${BASE}/${id}/deliver`),
 
  markFailed: (id: string, data: FailReasonRequestDto) =>
    httpClient.put(`${BASE}/${id}/fail`, data),
 
  markReturned: (id: string, data: FailReasonRequestDto) =>
    httpClient.put(`${BASE}/${id}/return`, data),
 
  cancel: (id: string, data: FailReasonRequestDto) =>
    httpClient.put(`${BASE}/${id}/cancel`, data),
};