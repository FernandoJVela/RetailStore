import type { ProviderDto, ProviderDetailDto, Provider, ProviderDetail } from '@features/providers';
 
/** Maps API DTOs → domain models. */
 
export function mapProviderDto(dto: ProviderDto): Provider {
  return {
    id: dto.id,
    companyName: dto.companyName,
    contactName: dto.contactName,
    email: dto.email,
    phone: dto.phone,
    isActive: dto.isActive,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
    productCount: dto.productCount,
  };
}
 
export function mapProviderDetailDto(dto: ProviderDetailDto): ProviderDetail {
  return {
    id: dto.id,
    companyName: dto.companyName,
    contactName: dto.contactName,
    email: dto.email,
    phone: dto.phone,
    isActive: dto.isActive,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
    productCount: dto.productCount,
    productIds: dto.productIds ?? [],
    createdAt: new Date(dto.createdAt),
    updatedAt: dto.updatedAt ? new Date(dto.updatedAt) : null,
  };
}