/** Domain models — what the UI actually needs. Not 1:1 with API DTOs. */
 
export interface Product {
  id: string;
  name: string;
  sku: string;
  price: number;
  currency: string;
  formattedPrice: string;   // Computed: "$79.99 USD"
  category: string;
  isActive: boolean;
  statusLabel: string;
}
 
export interface ProductDetail extends Product {
  description: string | null;
  createdAt: Date;
  updatedAt: Date | null;
}
 
export interface CreateProductData { 
  description?: string; 
  name: string; 
  sku: string; 
  price: number; 
  currency: string; 
  category: string; 
}
 
export interface UpdateProductDetailsData {
  name: string;
  category: string;
  description?: string;
}
 
export interface UpdateProductPriceData {
  price: number;
  currency: string;
}
 
/** Available product categories (derived from seed data) */
export const PRODUCT_CATEGORIES = [
  'Electronics',
  'Furniture',
  'Stationery',
] as const;
 
export type ProductCategory = (typeof PRODUCT_CATEGORIES)[number];