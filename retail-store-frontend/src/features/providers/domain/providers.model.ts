/** Domain models — what the UI actually needs. */
 
export interface Provider {
  id: string;
  companyName: string;
  contactName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  statusLabel: string;
  productCount: number;
}
 
export interface ProviderDetail extends Provider {
  productIds: string[];
  createdAt: Date;
  updatedAt: Date | null;
}
 
export interface RegisterProviderData {
  companyName: string;
  contactName: string;
  email: string;
  phone?: string;
}
 
export interface UpdateProviderData {
  companyName: string;
  contactName: string;
  phone?: string;
}