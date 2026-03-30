import type { ProductDto, ProductDetailDto, Product, ProductDetail } from '@features/products';
 
/** Maps API DTOs → domain models. Protects UI from backend changes. */
 
function formatPrice(amount: number, currency: string): string {
  try {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
    }).format(amount);
  } catch {
    return `${amount.toFixed(2)} ${currency}`;
  }
}
 
export function mapProductDto(dto: ProductDto): Product {
  return {
    id: dto.id,
    name: dto.name,
    sku: dto.sku,
    price: dto.price,
    currency: dto.currency,
    formattedPrice: formatPrice(dto.price, dto.currency),
    category: dto.category,
    isActive: dto.isActive,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
  };
}
 
export function mapProductDetailDto(dto: ProductDetailDto): ProductDetail {
  return {
    id: dto.id,
    name: dto.name,
    sku: dto.sku,
    description: dto.description,
    price: dto.price,
    currency: dto.currency,
    formattedPrice: formatPrice(dto.price, dto.currency),
    category: dto.category,
    isActive: dto.isActive,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
    createdAt: new Date(dto.createdAt),
    updatedAt: dto.updatedAt ? new Date(dto.updatedAt) : null,
  };
}