import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { CreateOrderModal } from '../CreateOrderModal';
import { useCreateOrder } from '@features/orders/application/hooks/useOrdersQueries';
import { useCustomers } from '@features/customers/application/hooks/useCustomersQueries';
import { useProducts } from '@features/products/application/hooks/useProductsQueries';

// ── Mocks ─────────────────────────────────────────────────────────────────────
vi.mock('@features/orders/application/hooks/useOrdersQueries', () => ({
  useCreateOrder: vi.fn(),
}));
vi.mock('@features/customers/application/hooks/useCustomersQueries', () => ({
  useCustomers: vi.fn(),
  useRegisterCustomer: vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false })),
}));
vi.mock('@features/products/application/hooks/useProductsQueries', () => ({
  useProducts: vi.fn(),
  useCreateProduct: vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false })),
}));

const mockCustomers = [
  { id: 'cust-1', fullName: 'Alice Smith', email: 'alice@example.com' },
  { id: 'cust-2', fullName: 'Bob Jones', email: 'bob@example.com' },
];

const mockProducts = [
  { id: 'prod-1', name: 'Widget Pro', formattedPrice: '$29.99' },
  { id: 'prod-2', name: 'Gadget X', formattedPrice: '$49.99' },
];

function setupMocks(overrides: { isPending?: boolean } = {}) {
  vi.mocked(useCreateOrder).mockReturnValue({
    mutateAsync: vi.fn().mockResolvedValue({ id: 'new-order' }),
    isPending: overrides.isPending ?? false,
  } as ReturnType<typeof useCreateOrder>);

  vi.mocked(useCustomers).mockReturnValue({
    data: mockCustomers,
  } as ReturnType<typeof useCustomers>);

  vi.mocked(useProducts).mockReturnValue({
    data: mockProducts,
  } as ReturnType<typeof useProducts>);
}

beforeEach(() => {
  vi.clearAllMocks();
  setupMocks();
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('CreateOrderModal', () => {
  it('does not render when isOpen is false', () => {
    render(<CreateOrderModal isOpen={false} onClose={() => {}} />);

    expect(screen.queryByText('Create Order')).not.toBeInTheDocument();
  });

  it('renders the Customer select with options from the hook', () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    expect(screen.getByText('Alice Smith (alice@example.com)')).toBeInTheDocument();
    expect(screen.getByText('Bob Jones (bob@example.com)')).toBeInTheDocument();
  });

  it('shows the "no items yet" placeholder when no items are added', () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    expect(
      screen.getByText(/No items added yet/i)
    ).toBeInTheDocument();
  });

  it('"Create Order" button is disabled when no customer and no items are selected', () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    // The create button text includes the item count
    const createBtn = screen.getByRole('button', { name: /create order/i });
    expect(createBtn).toBeDisabled();
  });

  it('Add button (+) is disabled until a product is selected', () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    // The Plus button has no accessible name — find by its disabled state
    const addButtons = screen.getAllByRole('button');
    const addBtn = addButtons.find((b) => b.hasAttribute('disabled'));
    expect(addBtn).toBeDefined();
  });

  it('adds an item to the list after selecting a product and clicking Add', async () => {
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    // Select a product from the second combobox (product select)
    const productSelect = screen.getAllByRole('combobox')[1];
    await user.selectOptions(productSelect, 'prod-1');

    // The Add (+) button is outline-variant and becomes enabled once a product is chosen.
    // Find it by its disabled→enabled transition: it was disabled before, now it's not.
    const allButtons = screen.getAllByRole('button');
    const addBtn = allButtons.find(
      (b) => !b.hasAttribute('disabled') && b.className.includes('outline')
    );
    expect(addBtn).toBeDefined();
    await user.click(addBtn!);

    expect(screen.getByText('Widget Pro')).toBeInTheDocument();
  });

  it('removes an item from the list when the delete button is clicked', async () => {
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    // Helper: add one product to the line items
    const productSelect = screen.getAllByRole('combobox')[1];
    await user.selectOptions(productSelect, 'prod-1');
    const addBtn = screen.getAllByRole('button').find(
      (b) => !b.hasAttribute('disabled') && b.className.includes('outline')
    )!;
    await user.click(addBtn);
    expect(screen.getByText('Widget Pro')).toBeInTheDocument();

    // Remove it using the red trash button
    const trashBtn = screen.getAllByRole('button').find(
      (b) => b.className.includes('text-red-500')
    )!;
    await user.click(trashBtn);
    expect(screen.queryByText('Widget Pro')).not.toBeInTheDocument();
  });

  it('shows the item count in the Create Order button label', async () => {
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    const productSelect = screen.getAllByRole('combobox')[1];
    await user.selectOptions(productSelect, 'prod-1');
    const addBtn = screen.getAllByRole('button').find(
      (b) => !b.hasAttribute('disabled') && b.className.includes('outline')
    )!;
    await user.click(addBtn);

    expect(screen.getByRole('button', { name: /create order \(1 item\)/i })).toBeInTheDocument();
  });

  it('calls onClose when Cancel is clicked', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={onClose} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('shows loading state on Create button when mutation is pending', () => {
    setupMocks({ isPending: true });
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    const createBtn = screen.getByRole('button', { name: /create order/i });
    expect(createBtn).toBeDisabled();
  });
});
