/** API DTOs — match backend JSON responses exactly. Never used in UI directly. */
 
export interface ProductDto {
  id: string;
  name: string;
  sku: string;
  price: number;
  currency: string;
  category: string;
  isActive: boolean;
}
 
export interface ProductDetailDto {
  id: string;
  name: string;
  sku: string;
  description: string | null;
  price: number;
  currency: string;
  category: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}
 
export interface CreateProductRequestDto {
  name: string;
  sku: string;
  price: number;
  currency: string;
  category: string;
  description?: string | null;
}
 
export interface UpdateProductDetailsRequestDto {
  name: string;
  category: string;
  description?: string | null;
}
 
export interface UpdateProductPriceRequestDto {
  price: number;
  currency: string;
}