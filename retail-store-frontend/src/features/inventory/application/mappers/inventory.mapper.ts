import type { InventoryItemDto, InventoryDetailDto } from '@features/inventory';
import type { InventoryItem, InventoryDetail, StockStatus } from '@features/inventory';
 
/** Maps API DTOs → domain models. Computes UI-specific fields. */
 
function parseStockStatus(status: string): StockStatus {
  if (status === 'LowStock') return 'LowStock';
  if (status === 'OutOfStock') return 'OutOfStock';
  return 'InStock';
}
 
function computeHealthPercent(available: number, threshold: number): number {
  if (threshold <= 0) return available > 0 ? 100 : 0;
  // 100% = 3x threshold (healthy), 0% = 0 available
  const ratio = Math.min((available / (threshold * 3)) * 100, 100);
  return Math.max(Math.round(ratio), 0);
}
 
export function mapInventoryItemDto(dto: InventoryItemDto): InventoryItem {
  const status = parseStockStatus(dto.stockStatus);
  return {
    id: dto.id,
    productId: dto.productId,
    productName: dto.productName,
    sku: dto.sku,
    quantityOnHand: dto.quantityOnHand,
    reservedQuantity: dto.reservedQuantity,
    availableQuantity: dto.availableQuantity,
    reorderThreshold: dto.reorderThreshold,
    stockStatus: status,
    stockHealthPercent: computeHealthPercent(dto.availableQuantity, dto.reorderThreshold),
    isLowStock: status === 'LowStock',
    isOutOfStock: status === 'OutOfStock',
  };
}
 
export function mapInventoryDetailDto(dto: InventoryDetailDto): InventoryDetail {
  const base = mapInventoryItemDto(dto);
  return {
    ...base,
    createdAt: new Date(dto.createdAt),
    updatedAt: dto.updatedAt ? new Date(dto.updatedAt) : null,
  };
}