import { productsApi } from '@features/products';
import type {
  Product, ProductDetail, CreateProductData,
  UpdateProductDetailsData, UpdateProductPriceData,
} from '@features/products';
import { mapProductDto, mapProductDetailDto } from '@features/products/application/mappers/products.mapper';
 
/** Repository: calls API, maps through mapper, returns domain models. */
export const productsRepository = {
  async getAll(params?: { category?: string; isActive?: boolean; search?: string }): Promise<Product[]> {
    const { data } = await productsApi.getAll(params);
    return data.map(mapProductDto);
  },
 
  async getById(id: string): Promise<ProductDetail> {
    const { data } = await productsApi.getById(id);
    return mapProductDetailDto(data);
  },
 
  async create(input: CreateProductData): Promise<string> {
    const { data } = await productsApi.create({
      name: input.name,
      sku: input.sku,
      price: input.price,
      currency: input.currency,
      category: input.category,
      description: input.description || null,
    });
    return data;
  },
 
  async updateDetails(id: string, input: UpdateProductDetailsData): Promise<void> {
    await productsApi.updateDetails(id, {
      name: input.name,
      category: input.category,
      description: input.description || null,
    });
  },
 
  async updatePrice(id: string, input: UpdateProductPriceData): Promise<void> {
    await productsApi.updatePrice(id, {
      price: input.price,
      currency: input.currency,
    });
  },
 
  async deactivate(id: string): Promise<void> {
    await productsApi.deactivate(id);
  },
 
  async reactivate(id: string): Promise<void> {
    await productsApi.reactivate(id);
  },
};