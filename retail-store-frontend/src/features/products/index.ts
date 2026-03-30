export { ProductsListPage } from './ui/pages/ProductsListPage';
export { useProducts, useProduct, useCreateProduct } from './application/hooks/useProductsQueries';
export type { Product, ProductDetail, CreateProductData, UpdateProductDetailsData, UpdateProductPriceData } from './domain/products.model';
export { PRODUCT_CATEGORIES } from './domain/products.model';
export { productsApi } from '@features/products/api/products.api';
export { createProductSchema } from './application/useCases/products.validation';
export type { CreateProductFormData } from './application/useCases/products.validation';
export type { ProductDto, 
    ProductDetailDto, 
    CreateProductRequestDto, 
    UpdateProductDetailsRequestDto, 
    UpdateProductPriceRequestDto } from '@features/products/api/products.dto';