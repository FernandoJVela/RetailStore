import { render, type RenderOptions } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, type MemoryRouterProps } from 'react-router-dom';
import type { ReactNode } from 'react';

// ── QueryClient factory ───────────────────────────────────────────────────────
// Each test gets a fresh client with retries disabled so failures are immediate.
export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0, staleTime: 0 },
      mutations: { retry: false },
    },
  });
}

// ── Wrapper ───────────────────────────────────────────────────────────────────

interface WrapperOptions {
  /** Initial URL for MemoryRouter — useful when testing route-dependent components. */
  initialEntries?: MemoryRouterProps['initialEntries'];
  /** Supply a pre-configured QueryClient (e.g. pre-seeded with data). */
  queryClient?: QueryClient;
}

function createWrapper({ initialEntries = ['/'], queryClient }: WrapperOptions = {}) {
  const client = queryClient ?? createTestQueryClient();

  return function AllProviders({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={initialEntries}>
          {children}
        </MemoryRouter>
      </QueryClientProvider>
    );
  };
}

// ── Custom render ─────────────────────────────────────────────────────────────

interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'>, WrapperOptions {}

/**
 * Drop-in replacement for @testing-library/react `render`.
 * Wraps the component in QueryClientProvider + MemoryRouter.
 *
 * ```tsx
 * import { render, screen } from '@/test/test-utils';
 *
 * it('shows the product name', () => {
 *   render(<ProductCard name="Widget" />);
 *   expect(screen.getByText('Widget')).toBeInTheDocument();
 * });
 * ```
 */
function customRender(ui: ReactNode, options: CustomRenderOptions = {}) {
  const { initialEntries, queryClient, ...renderOptions } = options;
  return render(ui, {
    wrapper: createWrapper({ initialEntries, queryClient }),
    ...renderOptions,
  });
}

// ── userEvent setup ───────────────────────────────────────────────────────────

/**
 * Returns a pre-configured userEvent instance with pointer simulation enabled.
 * Call this at the top of each test that simulates user interaction.
 *
 * ```tsx
 * const user = setupUser();
 * await user.click(screen.getByRole('button'));
 * ```
 */
export function setupUser() {
  return userEvent.setup();
}

// Re-export everything from testing-library so tests import from one place.
export * from '@testing-library/react';
export { customRender as render };
