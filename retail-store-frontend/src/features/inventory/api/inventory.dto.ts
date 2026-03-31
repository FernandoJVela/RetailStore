/** API DTOs — match backend JSON responses exactly. Never used in UI directly. */
 
export interface InventoryItemDto {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderThreshold: number;
  stockStatus: string;  // "InStock" | "LowStock" | "OutOfStock"
}
 
export interface InventoryDetailDto {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderThreshold: number;
  stockStatus: string;
  createdAt: string;
  updatedAt: string | null;
}
 
export interface CreateInventoryItemRequestDto {
  productId: string;
  initialQuantity: number;
  reorderThreshold?: number;
}
 
export interface StockQuantityRequestDto {
  quantity: number;
}
 
export interface AdjustStockRequestDto {
  newQuantity: number;
  reason: string;
}
 
export interface UpdateReorderThresholdRequestDto {
  newThreshold: number;
}