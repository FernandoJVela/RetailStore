// Public API of the Users feature module.
// Other modules should only import from this barrel.
export { LoginPage } from './ui/pages/LoginPage';
export { RegisterPage } from './ui/pages/RegisterPage';
export { UsersListPage } from './ui/pages/UsersListPage';
export { usersApi } from './api/users.api';
export { useLogin, useRegister, useUsers, useUser, useRoles, useAssignRole, useRevokeRole } from './application/hooks/useUsersQueries';
export { mapRoleDto, mapUserDto, mapLoginResponse } from './application/mappers/users.mapper';
export { loginSchema, registerSchema } from './application/useCases/auth.validation';
export type { LoginFormData, RegisterFormData } from './application/useCases/auth.validation';
export type { User, Role, AuthSession, LoginCredentials, RegisterData } from './domain/users.model';
export type { 
    RoleDto, 
    UserDto, 
    LoginRequestDto, 
    LoginResponseDto, 
    RegisterRequestDto, 
    AssignRoleRequestDto, 
    RevokeRoleRequestDto } from './api/users.dto';