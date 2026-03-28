import { create } from 'zustand';
 
export interface AuthUser {
  userId: string;
  username: string;
  email: string;
  roles: string[];
  permissions: string[];
}
 
interface AuthState {
  user: AuthUser | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  login: (user: AuthUser, accessToken: string, refreshToken: string) => void;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}
 
export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: localStorage.getItem('accessToken'),
  isAuthenticated: !!localStorage.getItem('accessToken'),
 
  login: (user, accessToken, refreshToken) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    set({ user, accessToken, isAuthenticated: true });
  },
 
  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    set({ user: null, accessToken: null, isAuthenticated: false });
  },
 
  hasPermission: (permission) => {
    const { user } = get();
    if (!user) return false;
    return user.permissions.some(
      (p) => p === permission || p === `${permission.split(':')[0]}:*`
    );
  },
 
  hasRole: (role) => {
    const { user } = get();
    if (!user) return false;
    return user.roles.includes(role);
  },
}));