import { usersApi } from '@features/users';
import type { LoginCredentials, RegisterData, User, Role, AuthSession } from '@features/users';
import { mapUserDto, mapRoleDto, mapLoginResponse } from '@features/users';
 
/** Repository: calls API service, maps through mapper, returns domain models.
 *  UI never sees raw API DTOs. Backend changes are absorbed here + in mapper. */
 
export const usersRepository = {
  async login(credentials: LoginCredentials): Promise<AuthSession> {
    const { data } = await usersApi.login({
      email: credentials.email,
      password: credentials.password,
    });
    return mapLoginResponse(data);
  },
 
  async register(registerData: RegisterData): Promise<string> {
    const { data } = await usersApi.register({
      username: registerData.username,
      email: registerData.email,
      password: registerData.password,
    });
    return data;
  },
 
  async getAll(): Promise<User[]> {
    const [usersRes, rolesRes] = await Promise.all([
      usersApi.getAll(),
      usersApi.getRoles(),
    ]);
    const roles = rolesRes.data;
    const roleMap = new Map(roles.map((r) => [r.id, r.name]));
 
    return usersRes.data.map((dto) => {
      const roleIds: string[] = typeof dto.roleIds === 'string'
        ? JSON.parse(dto.roleIds)
        : dto.roleIds ?? [];
      const roleNames = roleIds.map((id) => roleMap.get(id) ?? 'Unknown');
      return mapUserDto(dto, roleNames);
    });
  },
 
  async getById(id: string): Promise<User> {
    const [userRes, rolesRes] = await Promise.all([
      usersApi.getById(id),
      usersApi.getRoles(),
    ]);
    const roleMap = new Map(rolesRes.data.map((r) => [r.id, r.name]));
    const roleIds: string[] = typeof userRes.data.roleIds === 'string'
      ? JSON.parse(userRes.data.roleIds as unknown as string)
      : userRes.data.roleIds ?? [];
    const roleNames = roleIds.map((id) => roleMap.get(id) ?? 'Unknown');
    return mapUserDto(userRes.data, roleNames);
  },
 
  async getRoles(): Promise<Role[]> {
    const { data } = await usersApi.getRoles();
    return data.map(mapRoleDto);
  },
 
  async deactivate(id: string): Promise<void> {
    await usersApi.deactivate(id);
  },
 
  async reactivate(id: string): Promise<void> {
    await usersApi.reactivate(id);
  },
 
  async assignRole(userId: string, roleName: string): Promise<void> {
    await usersApi.assignRole(userId, { roleName });
  },
 
  async revokeRole(userId: string, roleName: string): Promise<void> {
    await usersApi.revokeRole(userId, { roleName });
  },
};