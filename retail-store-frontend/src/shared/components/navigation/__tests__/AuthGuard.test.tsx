import { Route, Routes } from 'react-router-dom';
import { render, screen } from '@/test/test-utils';
import { AuthGuard } from '../AuthGuard';
import { useAuthStore } from '@shared/store/auth-store';

// ── Test helpers ──────────────────────────────────────────────────────────────

const mockUser = {
  userId: 'user-1',
  username: 'testuser',
  email: 'test@example.com',
  roles: ['Staff'],
  permissions: ['products:write', 'orders:read'],
};

/** Renders AuthGuard inside a router that has both the protected route and /login. */
function renderWithRoutes(guardProps: { requiredPermission?: string } = {}) {
  return render(
    <Routes>
      <Route
        path="/"
        element={
          <AuthGuard {...guardProps}>
            <p>Protected content</p>
          </AuthGuard>
        }
      />
      <Route path="/login" element={<p data-testid="login-page">Login</p>} />
      <Route path="/dashboard" element={<p data-testid="dashboard">Dashboard</p>} />
    </Routes>
  );
}

// ── Setup ──────────────────────────────────────────────────────────────────────

beforeEach(() => {
  localStorage.clear();
  useAuthStore.setState({
    user: null,
    accessToken: null,
    isAuthenticated: false,
  });
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('AuthGuard', () => {
  it('redirects to /login when user is not authenticated', () => {
    renderWithRoutes();

    expect(screen.getByTestId('login-page')).toBeInTheDocument();
    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  });

  it('renders children when user is authenticated and no permission required', () => {
    useAuthStore.setState({ user: mockUser, accessToken: 'token', isAuthenticated: true });

    renderWithRoutes();

    expect(screen.getByText('Protected content')).toBeInTheDocument();
    expect(screen.queryByTestId('login-page')).not.toBeInTheDocument();
  });

  it('renders children when user has the required permission', () => {
    useAuthStore.setState({ user: mockUser, accessToken: 'token', isAuthenticated: true });

    renderWithRoutes({ requiredPermission: 'products:write' });

    expect(screen.getByText('Protected content')).toBeInTheDocument();
  });

  it('redirects to / when user lacks the required permission', () => {
    useAuthStore.setState({ user: mockUser, accessToken: 'token', isAuthenticated: true });

    // mockUser has no 'users:manage' permission
    render(
      <Routes>
        <Route
          path="/"
          element={
            <AuthGuard requiredPermission="users:manage">
              <p>Admin area</p>
            </AuthGuard>
          }
        />
        <Route path="/dashboard" element={<p data-testid="dashboard">Dashboard</p>} />
      </Routes>
    );

    expect(screen.queryByText('Admin area')).not.toBeInTheDocument();
  });

  it('grants access when user has a wildcard resource permission', () => {
    useAuthStore.setState({
      user: { ...mockUser, permissions: ['products:*'] },
      accessToken: 'token',
      isAuthenticated: true,
    });

    renderWithRoutes({ requiredPermission: 'products:write' });

    expect(screen.getByText('Protected content')).toBeInTheDocument();
  });
});
