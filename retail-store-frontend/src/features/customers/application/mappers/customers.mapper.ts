import type { CustomerDto, CustomerDetailDto, ShippingAddressResponseDto } from '@features/customers';
import type { Customer, CustomerDetail, ShippingAddress } from '@features/customers';
 
/** Maps API DTOs → domain models. Protects UI from backend changes. */
 
export function mapCustomerDto(dto: CustomerDto): Customer {
  return {
    id: dto.id,
    firstName: dto.firstName,
    lastName: dto.lastName,
    fullName: dto.fullName,
    email: dto.email,
    phone: dto.phone,
    isActive: dto.isActive,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
    createdAt: new Date(dto.createdAt),
  };
}
 
export function mapCustomerDetailDto(dto: CustomerDetailDto): CustomerDetail {
  const address = dto.shippingAddress ? mapAddressDto(dto.shippingAddress) : null;
  return {
    id: dto.id,
    firstName: dto.firstName,
    lastName: dto.lastName,
    fullName: dto.fullName,
    email: dto.email,
    phone: dto.phone,
    isActive: dto.isActive,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
    createdAt: new Date(dto.createdAt),
    updatedAt: dto.updatedAt ? new Date(dto.updatedAt) : null,
    shippingAddress: address,
    hasAddress: address !== null,
  };
}
 
function mapAddressDto(dto: ShippingAddressResponseDto): ShippingAddress {
  return {
    street: dto.street,
    city: dto.city,
    state: dto.state,
    zipCode: dto.zipCode,
    country: dto.country,
    fullAddress: [dto.street, dto.city, dto.state, dto.zipCode, dto.country]
      .filter(Boolean)
      .join(', '),
  };
}