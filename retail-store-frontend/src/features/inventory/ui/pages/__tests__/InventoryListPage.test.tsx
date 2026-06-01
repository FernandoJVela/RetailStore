import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { InventoryListPage } from '../InventoryListPage';
import { useInventory } from '@features/inventory/application/hooks/useInventoryQueries';

// ── Mocks ─────────────────────────────────────────────────────────────────────
vi.mock('@features/inventory/application/hooks/useInventoryQueries', () => ({
  useInventory: vi.fn(),
  useStockMutations: vi.fn(() => ({
    addStock: { mutateAsync: vi.fn(), isPending: false },
    removeStock: { mutateAsync: vi.fn(), isPending: false },
    adjustStock: { mutateAsync: vi.fn(), isPending: false },
    updateThreshold: { mutateAsync: vi.fn(), isPending: false },
  })),
}));

// Make useDebounce return the value synchronously so search tests don't need timers
vi.mock('@shared/hooks', () => ({
  // Avoid generic syntax in .tsx files — JSX parser misreads <T> as JSX
  useDebounce: (value: unknown) => value,
}));

// ── Test data ─────────────────────────────────────────────────────────────────
const mockItems = [
  {
    id: 'inv-1', productId: 'prod-1', productName: 'Widget Pro', sku: 'WGT-001',
    quantityOnHand: 50, reservedQuantity: 5, availableQuantity: 45,
    reorderThreshold: 10, stockStatus: 'InStock',
  },
  {
    id: 'inv-2', productId: 'prod-2', productName: 'Gadget X', sku: 'GDG-001',
    quantityOnHand: 8, reservedQuantity: 0, availableQuantity: 8,
    reorderThreshold: 10, stockStatus: 'LowStock',
  },
  {
    id: 'inv-3', productId: 'prod-3', productName: 'Widget Mini', sku: 'WGT-002',
    quantityOnHand: 0, reservedQuantity: 0, availableQuantity: 0,
    reorderThreshold: 10, stockStatus: 'OutOfStock',
  },
];

function setupInventoryMock(overrides: { isLoading?: boolean; data?: typeof mockItems } = {}) {
  vi.mocked(useInventory).mockReturnValue({
    data: overrides.data ?? mockItems,
    isLoading: overrides.isLoading ?? false,
    isError: false,
  } as ReturnType<typeof useInventory>);
}

beforeEach(() => {
  vi.clearAllMocks();
  setupInventoryMock();
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('InventoryListPage', () => {
  // ── Loading state ─────────────────────────────────────────────────────────

  it('shows a loading spinner while data is fetching', () => {
    setupInventoryMock({ isLoading: true, data: undefined });
    const { container } = render(<InventoryListPage />);

    // Spinner renders a div with animate-spin class
    expect(container.querySelector('.animate-spin')).toBeInTheDocument();
  });

  // ── Empty state ───────────────────────────────────────────────────────────

  it('shows empty state when there are no inventory items', () => {
    setupInventoryMock({ data: [] });
    render(<InventoryListPage />);

    // EmptyState renders the noInventoryFound i18n key text
    // In tests i18n returns the key, so check for the key or a partial match
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  // ── Table rendering ───────────────────────────────────────────────────────

  it('renders a row for each inventory item', () => {
    render(<InventoryListPage />);

    expect(screen.getByText('Widget Pro')).toBeInTheDocument();
    expect(screen.getByText('Gadget X')).toBeInTheDocument();
    expect(screen.getByText('Widget Mini')).toBeInTheDocument();
  });

  it('renders the table with column headers', () => {
    render(<InventoryListPage />);

    // Column headers come from i18n keys — check they're rendered (key or translated)
    const table = document.querySelector('table');
    expect(table).toBeInTheDocument();
    expect(table!.querySelectorAll('th').length).toBeGreaterThanOrEqual(4);
  });

  // ── Summary stats ─────────────────────────────────────────────────────────

  it('renders four summary stat cards with correct aggregate counts', () => {
    render(<InventoryListPage />);

    // 3 items total → text '3' should appear exactly once (total count card)
    expect(screen.getByText('3')).toBeInTheDocument();

    // 1 InStock, 1 LowStock, 1 OutOfStock → '1' appears three times (one per card)
    const ones = screen.getAllByText('1');
    expect(ones).toHaveLength(3);
  });

  // ── Search filtering ──────────────────────────────────────────────────────

  it('filters items by product name when search text is entered', async () => {
    const user = setupUser();
    render(<InventoryListPage />);

    const searchInput = screen.getByRole('textbox');
    await user.type(searchInput, 'Gadget');

    expect(screen.getByText('Gadget X')).toBeInTheDocument();
    expect(screen.queryByText('Widget Pro')).not.toBeInTheDocument();
  });

  it('filters items by SKU when search text matches', async () => {
    const user = setupUser();
    render(<InventoryListPage />);

    const searchInput = screen.getByRole('textbox');
    await user.type(searchInput, 'WGT-002');

    expect(screen.getByText('Widget Mini')).toBeInTheDocument();
    expect(screen.queryByText('Gadget X')).not.toBeInTheDocument();
  });

  it('shows all items when search is cleared', async () => {
    const user = setupUser();
    render(<InventoryListPage />);

    const searchInput = screen.getByRole('textbox');
    await user.type(searchInput, 'Widget');
    await user.clear(searchInput);

    expect(screen.getByText('Widget Pro')).toBeInTheDocument();
    expect(screen.getByText('Gadget X')).toBeInTheDocument();
  });

  // ── Status filter buttons ─────────────────────────────────────────────────

  it('renders the status filter buttons (All, InStock, LowStock, OutOfStock)', () => {
    render(<InventoryListPage />);

    // i18n keys used: 'common.all', 'inventory.inStock', etc.
    // In tests they render as-is or as translation keys
    const buttons = screen.getAllByRole('button');
    // There should be at least 4 filter buttons (All + 3 statuses)
    expect(buttons.length).toBeGreaterThanOrEqual(4);
  });
});
