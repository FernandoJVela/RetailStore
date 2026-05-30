import { shippingApi } from '@features/shipping';
import type { Shipment, ShipmentDetail, AssignCarrierData } from '@features/shipping';
import { mapShipmentDto, mapShipmentDetailDto } from '@features/shipping/application/mappers/shipping.mapper';
 
export const shippingRepository = {
  async getAll(params?: { status?: string; customerId?: string }): Promise<Shipment[]> {
    const { data } = await shippingApi.getAll(params);
    return data.map(mapShipmentDto);
  },
 
  async getById(id: string): Promise<ShipmentDetail> {
    const { data } = await shippingApi.getById(id);
    return mapShipmentDetailDto(data);
  },
 
  async assignCarrier(id: string, input: AssignCarrierData): Promise<void> {
    await shippingApi.assignCarrier(id, {
      carrier: input.carrier, trackingNumber: input.trackingNumber,
      estimatedDelivery: input.estimatedDelivery || null,
    });
  },
 
  async setCost(id: string, cost: number, currency: string): Promise<void> {
    await shippingApi.setCost(id, { cost, currency });
  },
 
  async markShipped(id: string): Promise<void> { await shippingApi.markShipped(id); },
  async markInTransit(id: string): Promise<void> { await shippingApi.markInTransit(id); },
  async markDelivered(id: string): Promise<void> { await shippingApi.markDelivered(id); },
 
  async markFailed(id: string, reason: string): Promise<void> {
    await shippingApi.markFailed(id, { reason });
  },
  async markReturned(id: string, reason: string): Promise<void> {
    await shippingApi.markReturned(id, { reason });
  },
  async cancel(id: string, reason: string): Promise<void> {
    await shippingApi.cancel(id, { reason });
  },
};