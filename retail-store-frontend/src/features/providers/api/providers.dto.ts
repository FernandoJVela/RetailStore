/** API DTOs — match backend JSON responses exactly. Never used in UI directly. */
 
export interface ProviderDto {
  id: string;
  companyName: string;
  contactName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  productCount: number;
}
 
export interface ProviderDetailDto {
  id: string;
  companyName: string;
  contactName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  productIds: string[];
  productCount: number;
  createdAt: string;
  updatedAt: string | null;
}
 
export interface RegisterProviderRequestDto {
  companyName: string;
  contactName: string;
  email: string;
  phone?: string | null;
}
 
export interface UpdateProviderRequestDto {
  companyName: string;
  contactName: string;
  phone?: string | null;
}
 
export interface ChangeProviderEmailRequestDto {
  newEmail: string;
}