import { useAuthStore } from '../auth-store';

const mockUser = {
  userId: 'user-1',
  username: 'testadmin',
  email: 'admin@example.com',
  roles: ['Admin', 'Staff'],
  permissions: ['products:write', 'orders:read', 'inventory:*'],
};

// Reset store + localStorage before every test so they're fully isolated
beforeEach(() => {
  localStorage.clear();
  useAuthStore.setState({
    user: null,
    accessToken: null,
    isAuthenticated: false,
  });
});

// ── Initial state ─────────────────────────────────────────────────────────────

describe('useAuthStore – initial state', () => {
  it('starts unauthenticated when localStorage has no token', () => {
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
    expect(state.accessToken).toBeNull();
  });
});

// ── login ─────────────────────────────────────────────────────────────────────

describe('useAuthStore – login', () => {
  it('sets isAuthenticated to true', () => {
    useAuthStore.getState().login(mockUser, 'access-token', 'refresh-token');
    expect(useAuthStore.getState().isAuthenticated).toBe(true);
  });

  it('stores the user object', () => {
    useAuthStore.getState().login(mockUser, 'access-token', 'refresh-token');
    expect(useAuthStore.getState().user).toEqual(mockUser);
  });

  it('stores the access token in state', () => {
    useAuthStore.getState().login(mockUser, 'access-token', 'refresh-token');
    expect(useAuthStore.getState().accessToken).toBe('access-token');
  });

  it('persists access token to localStorage', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    expect(localStorage.getItem('accessToken')).toBe('at');
  });

  it('persists refresh token to localStorage', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    expect(localStorage.getItem('refreshToken')).toBe('rt');
  });
});

// ── logout ────────────────────────────────────────────────────────────────────

describe('useAuthStore – logout', () => {
  beforeEach(() => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
  });

  it('sets isAuthenticated to false', () => {
    useAuthStore.getState().logout();
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('clears the user', () => {
    useAuthStore.getState().logout();
    expect(useAuthStore.getState().user).toBeNull();
  });

  it('clears the access token from state', () => {
    useAuthStore.getState().logout();
    expect(useAuthStore.getState().accessToken).toBeNull();
  });

  it('removes access token from localStorage', () => {
    useAuthStore.getState().logout();
    expect(localStorage.getItem('accessToken')).toBeNull();
  });

  it('removes refresh token from localStorage', () => {
    useAuthStore.getState().logout();
    expect(localStorage.getItem('refreshToken')).toBeNull();
  });
});

// ── hasPermission ─────────────────────────────────────────────────────────────

describe('useAuthStore – hasPermission', () => {
  it('returns false when user is not logged in', () => {
    expect(useAuthStore.getState().hasPermission('products:write')).toBe(false);
  });

  it('returns true for an exact permission match', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    expect(useAuthStore.getState().hasPermission('products:write')).toBe(true);
  });

  it('returns true for a permission covered by a resource wildcard (inventory:*)', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    // mockUser has 'inventory:*' which covers any inventory action
    expect(useAuthStore.getState().hasPermission('inventory:adjust')).toBe(true);
  });

  it('returns false for a permission not in the list', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    expect(useAuthStore.getState().hasPermission('users:manage')).toBe(false);
  });

  it('returns false when wildcard is for a different resource', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    // inventory:* does NOT cover orders:write
    expect(useAuthStore.getState().hasPermission('orders:write')).toBe(false);
  });
});

// ── hasRole ───────────────────────────────────────────────────────────────────

describe('useAuthStore – hasRole', () => {
  it('returns false when user is not logged in', () => {
    expect(useAuthStore.getState().hasRole('Admin')).toBe(false);
  });

  it('returns true for a role the user has', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    expect(useAuthStore.getState().hasRole('Admin')).toBe(true);
    expect(useAuthStore.getState().hasRole('Staff')).toBe(true);
  });

  it('returns false for a role the user does not have', () => {
    useAuthStore.getState().login(mockUser, 'at', 'rt');
    expect(useAuthStore.getState().hasRole('SuperAdmin')).toBe(false);
  });
});
