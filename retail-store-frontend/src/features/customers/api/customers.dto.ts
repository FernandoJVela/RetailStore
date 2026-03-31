/** API DTOs — match backend JSON responses exactly. Never used in UI directly. */
 
export interface CustomerDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  createdAt: string;
}
 
export interface CustomerDetailDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  shippingAddress: ShippingAddressResponseDto | null;
  createdAt: string;
  updatedAt: string | null;
}
 
export interface ShippingAddressResponseDto {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}
 
export interface RegisterCustomerRequestDto {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string | null;
  shippingAddress?: ShippingAddressRequestDto | null;
}
 
export interface ShippingAddressRequestDto {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}
 
export interface UpdateCustomerRequestDto {
  firstName: string;
  lastName: string;
  phone?: string | null;
}
 
export interface ChangeEmailRequestDto {
  newEmail: string;
}
 
export interface UpdateShippingAddressRequestDto {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}