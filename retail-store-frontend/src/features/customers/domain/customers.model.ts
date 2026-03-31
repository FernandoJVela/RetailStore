/** Domain models — what the UI actually needs. Not 1:1 with API DTOs. */
 
export interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  statusLabel: string;
  createdAt: Date;
}
 
export interface CustomerDetail extends Customer {
  shippingAddress: ShippingAddress | null;
  updatedAt: Date | null;
  hasAddress: boolean;
}
 
export interface ShippingAddress {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  fullAddress: string; // Computed: "Street, City, State ZipCode, Country"
}
 
export interface RegisterCustomerData {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  shippingAddress?: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
  };
}
 
export interface UpdateCustomerData {
  firstName: string;
  lastName: string;
  phone?: string;
}
 
export interface UpdateAddressData {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}