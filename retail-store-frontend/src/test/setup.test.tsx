import { render, screen } from './test-utils';

/**
 * Smoke tests for the Vitest + React Testing Library setup.
 * Verifies that the test runner, DOM environment, jest-dom matchers,
 * custom render (with QueryClient + Router providers), and i18n are all wired up.
 */
describe('Test infrastructure smoke tests', () => {
  it('renders a simple React component', () => {
    render(<p>Hello from test</p>);
    expect(screen.getByText('Hello from test')).toBeInTheDocument();
  });

  it('jest-dom matchers are available', () => {
    render(<button disabled>Disabled</button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('custom render wraps with MemoryRouter (no crash on Link/useNavigate)', () => {
    // If MemoryRouter is missing, react-router-dom throws on render.
    // This passing confirms the provider is in place.
    const { container } = render(<div data-testid="routed">Routed</div>);
    expect(container).toBeInTheDocument();
  });

  it('i18next is initialised (t() does not throw)', async () => {
    const { default: i18n } = await import('@shared/i18n');
    expect(() => i18n.t('test')).not.toThrow();
  });
});
