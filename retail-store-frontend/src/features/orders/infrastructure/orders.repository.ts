import { ordersApi } from '@features/orders';
import type { Order, OrderDetail, CreateOrderData } from '@features/orders';
import { mapOrderDto, mapOrderDetailDto } from '@features/orders';
 
export const ordersRepository = {
  async getAll(params?: { status?: string }): Promise<Order[]> {
    const { data } = await ordersApi.getAll(params);
    return data.map(mapOrderDto);
  },
 
  async getById(id: string): Promise<OrderDetail> {
    const { data } = await ordersApi.getById(id);
    return mapOrderDetailDto(data);
  },
 
  async create(input: CreateOrderData): Promise<string> {
    const { data } = await ordersApi.create({
      customerId: input.customerId,
      items: input.items,
    });
    return data;
  },
 
  async addItem(orderId: string, productId: string, quantity: number): Promise<void> {
    await ordersApi.addItem(orderId, { productId, quantity });
  },
 
  async removeItem(orderId: string, productId: string): Promise<void> {
    await ordersApi.removeItem(orderId, productId);
  },
 
  async confirm(id: string): Promise<void> { await ordersApi.confirm(id); },
  async complete(id: string): Promise<void> { await ordersApi.complete(id); },
 
  async cancel(id: string, reason: string): Promise<void> {
    await ordersApi.cancel(id, { reason });
  },
};