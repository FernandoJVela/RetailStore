/**
 * Login workflow integration test.
 * Uses the REAL useLogin hook + MSW to intercept POST /api/v1/users/login.
 * Verifies the full flow: form submit → API call → auth store updated.
 */
import { render, screen, waitFor } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';
import { LoginPage } from '../LoginPage';
import { useAuthStore } from '@shared/store/auth-store';
import { mockLoginResponse } from '@/test/mocks/data/fixtures';

// ── Server lifecycle ──────────────────────────────────────────────────────────
beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }));
afterEach(() => {
  server.resetHandlers();
  localStorage.clear();
  useAuthStore.setState({ user: null, accessToken: null, isAuthenticated: false });
});
afterAll(() => server.close());

// ── Helpers ───────────────────────────────────────────────────────────────────

/** Type into the email field (found by input type, not by i18n label). */
async function fillLoginForm(user: ReturnType<typeof setupUser>, email: string, password: string) {
  // Email input: type="email" → textbox role
  const emailInput = screen.getByRole('textbox');
  // Password input: type="password" — not a textbox, find by placeholder
  const passwordInput = document.querySelector<HTMLInputElement>('input[type="password"]')!;

  await user.clear(emailInput);
  await user.type(emailInput, email);
  await user.clear(passwordInput);
  await user.type(passwordInput, password);
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('Login workflow', () => {
  it('renders email and password fields and a submit button', () => {
    render(<LoginPage />);

    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(document.querySelector('input[type="password"]')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i })).toBeInTheDocument();
  });

  it('successful login updates the auth store', async () => {
    const user = setupUser();
    render(<LoginPage />);

    await fillLoginForm(user, 'testuser@example.com', 'Password123!');
    await user.click(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i }));

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(true);
    });

    const state = useAuthStore.getState();
    expect(state.user?.username).toBe(mockLoginResponse.username);
    expect(state.accessToken).toBe(mockLoginResponse.accessToken);
    expect(localStorage.getItem('accessToken')).toBe(mockLoginResponse.accessToken);
  });

  it('successful login stores the user permissions', async () => {
    const user = setupUser();
    render(<LoginPage />);

    await fillLoginForm(user, 'testuser@example.com', 'Password123!');
    await user.click(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i }));

    await waitFor(() =>
      expect(useAuthStore.getState().isAuthenticated).toBe(true)
    );

    expect(useAuthStore.getState().user?.permissions).toContain('products:write');
  });

  it('wrong credentials show an error alert with the API error message', async () => {
    const user = setupUser();
    render(<LoginPage />);

    await fillLoginForm(user, 'testuser@example.com', 'wrong-password');
    await user.click(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i }));

    // The MSW handler returns: { detail: 'Email or password is incorrect.' }
    // getApiErrorMessage extracts .detail and the Alert component renders it
    await waitFor(() => {
      expect(screen.getByText('Email or password is incorrect.')).toBeInTheDocument();
    });

    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('custom server error shows the error message in an alert', async () => {
    server.use(
      http.post('/api/v1/users/login', () =>
        HttpResponse.json(
          { detail: 'Account is locked' },
          { status: 401 }
        )
      )
    );
    const user = setupUser();
    render(<LoginPage />);

    await fillLoginForm(user, 'testuser@example.com', 'Password123!');
    await user.click(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i }));

    await waitFor(() => {
      expect(screen.getByText('Account is locked')).toBeInTheDocument();
    });
  });

  it('login button is disabled while mutation is in-flight', async () => {
    // Delay the response so we can inspect the pending state
    server.use(
      http.post('/api/v1/users/login', async () => {
        await new Promise((r) => setTimeout(r, 300));
        return HttpResponse.json(mockLoginResponse);
      })
    );
    const user = setupUser();
    render(<LoginPage />);

    await fillLoginForm(user, 'testuser@example.com', 'Password123!');
    user.click(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i }));

    // Immediately after click, button should be disabled (spinner / loading state)
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i })).toBeDisabled();
    });
  });

  it('empty form submission shows validation errors without calling the API', async () => {
    let apiCalled = false;
    server.use(
      http.post('/api/v1/users/login', () => {
        apiCalled = true;
        return HttpResponse.json(mockLoginResponse);
      })
    );
    const user = setupUser();
    render(<LoginPage />);

    await user.click(screen.getByRole('button', { name: /login|sign in|entrar|iniciar/i }));

    // Validation fires client-side — API must not be called
    await waitFor(() => {
      // At least one error paragraph should appear
      const errors = document.querySelectorAll('p.text-xs');
      expect(errors.length).toBeGreaterThan(0);
    });
    expect(apiCalled).toBe(false);
  });
});
