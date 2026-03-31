import { inventoryApi } from '@features/inventory';
import type { InventoryItem, InventoryDetail, CreateInventoryData, AdjustStockData } from '@features/inventory';
import { mapInventoryItemDto, mapInventoryDetailDto } from '@features/inventory/application/mappers/inventory.mapper';
 
/** Repository: calls API, maps through mapper, returns domain models. */
export const inventoryRepository = {
  async getAll(params?: { stockStatus?: string }): Promise<InventoryItem[]> {
    const { data } = await inventoryApi.getAll(params);
    return data.map(mapInventoryItemDto);
  },
 
  async getByProduct(productId: string): Promise<InventoryDetail> {
    const { data } = await inventoryApi.getByProduct(productId);
    return mapInventoryDetailDto(data);
  },
 
  async getLowStock(): Promise<InventoryItem[]> {
    const { data } = await inventoryApi.getLowStock();
    return data.map(mapInventoryItemDto);
  },
 
  async create(input: CreateInventoryData): Promise<string> {
    const { data } = await inventoryApi.create({
      productId: input.productId,
      initialQuantity: input.initialQuantity,
      reorderThreshold: input.reorderThreshold,
    });
    return data;
  },
 
  async addStock(productId: string, quantity: number): Promise<void> {
    await inventoryApi.addStock(productId, { quantity });
  },
 
  async removeStock(productId: string, quantity: number): Promise<void> {
    await inventoryApi.removeStock(productId, { quantity });
  },
 
  async adjustStock(productId: string, input: AdjustStockData): Promise<void> {
    await inventoryApi.adjust(productId, {
      newQuantity: input.newQuantity,
      reason: input.reason,
    });
  },
 
  async updateThreshold(productId: string, newThreshold: number): Promise<void> {
    await inventoryApi.updateThreshold(productId, { newThreshold });
  },
};