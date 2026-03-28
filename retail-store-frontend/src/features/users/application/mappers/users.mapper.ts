import type { UserDto, RoleDto, LoginResponseDto, User, Role, AuthSession } from '@features/users';
 
/** Maps API DTOs → domain models. Protects UI from backend changes. */
 
/**
 * Normalize a permission value to a string.
 * Backend may return permissions as:
 *   - strings: "products:read"
 *   - objects: { resource: "products", action: "read", fullName: "products:read" }
 */
function normalizePermission(perm: unknown): string {
  if (typeof perm === 'string') return perm;
  if (perm && typeof perm === 'object') {
    const obj = perm as Record<string, unknown>;
    if (typeof obj.fullName === 'string') return obj.fullName;
    if (typeof obj.resource === 'string' && typeof obj.action === 'string')
      return `${obj.resource}:${obj.action}`;
  }
  return String(perm);
}
 
export function mapUserDto(dto: UserDto, roleNames: string[] = []): User {
  return {
    id: dto.id,
    username: dto.username,
    email: dto.email,
    isActive: dto.isActive,
    roles: roleNames,
    lastLogin: dto.lastLoginAt ? new Date(dto.lastLoginAt) : null,
    createdAt: new Date(dto.createdAt),
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
  };
}
 
export function mapRoleDto(dto: RoleDto): Role {
  // Normalize permissions: handle both string[] and object[]
  const permissions = Array.isArray(dto.permissions)
    ? dto.permissions.map(normalizePermission)
    : [];
 
  return {
    id: dto.id,
    name: dto.name,
    description: dto.description ?? '',
    isSystem: dto.isSystem,
    permissions,
    permissionCount: permissions.length,
  };
}
 
export function mapLoginResponse(dto: LoginResponseDto): AuthSession {
  return {
    userId: dto.userId,
    username: dto.username,
    email: dto.email,
    accessToken: dto.accessToken,
    refreshToken: dto.refreshToken,
    roles: dto.roles,
    permissions: Array.isArray(dto.permissions)
      ? dto.permissions.map(normalizePermission)
      : [],
  };
}