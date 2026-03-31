/** Domain models — what the UI actually needs. */
 
export type StockStatus = 'InStock' | 'LowStock' | 'OutOfStock';
 
export interface InventoryItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderThreshold: number;
  stockStatus: StockStatus;
  stockHealthPercent: number;   // Computed: available / threshold ratio for visual gauge
  isLowStock: boolean;          // Computed
  isOutOfStock: boolean;        // Computed
}
 
export interface InventoryDetail extends InventoryItem {
  createdAt: Date;
  updatedAt: Date | null;
}
 
export interface CreateInventoryData {
  productId: string;
  initialQuantity: number;
  reorderThreshold: number;
}
 
export interface AdjustStockData {
  newQuantity: number;
  reason: string;
}
 
export interface StockOperationData {
  quantity: number;
}
 
/** Badge variant for each stock status */
export function stockStatusVariant(status: StockStatus): 'success' | 'warning' | 'danger' {
  switch (status) {
    case 'InStock': return 'success';
    case 'LowStock': return 'warning';
    case 'OutOfStock': return 'danger';
  }
}