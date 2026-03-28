/** Domain models — what the UI actually needs. Not 1:1 with API DTOs. */
 
export interface User {
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  roles: string[];        // Role names, not IDs (mapped from roleIds + role list)
  lastLogin: Date | null;
  createdAt: Date;
  statusLabel: string;    // "Active" | "Inactive" — computed
}
 
export interface Role {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  permissions: string[];
  permissionCount: number; // Computed
}
 
export interface LoginCredentials {
  email: string;
  password: string;
}
 
export interface RegisterData {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}
 
export interface AuthSession {
  userId: string;
  username: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  roles: string[];
  permissions: string[];
}