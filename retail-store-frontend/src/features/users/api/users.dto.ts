/** API DTOs — match backend JSON responses exactly. Never used in UI directly. */
 
export interface LoginRequestDto {
  email: string;
  password: string;
}
 
export interface LoginResponseDto {
  accessToken: string;
  refreshToken: string;
  userId: string;
  username: string;
  email: string;
  roles: string[];
  permissions: string[];
}
 
export interface RegisterRequestDto {
  username: string;
  email: string;
  password: string;
}
 
export interface UserDto {
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  roleIds: string[];
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}
 
export interface RoleDto {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissions: string[];
  createdAt: string;
}
 
export interface AssignRoleRequestDto {
  roleName: string;
}
 
export interface RevokeRoleRequestDto {
  roleName: string;
}