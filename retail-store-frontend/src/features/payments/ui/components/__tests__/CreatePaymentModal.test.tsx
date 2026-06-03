import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { CreatePaymentModal } from '../CreatePaymentModal';
import { useCreatePayment } from '@features/payments/application/hooks/usePaymentsQueries';
import { useOrders } from '@features/orders/application/hooks/useOrdersQueries';

// ── Mocks ─────────────────────────────────────────────────────────────────────
vi.mock('@features/payments/application/hooks/usePaymentsQueries', () => ({
  useCreatePayment: vi.fn(),
}));
vi.mock('@features/orders/application/hooks/useOrdersQueries', () => ({
  useOrders: vi.fn(),
}));

const mockOrders = [
  { id: 'order-aabbccdd-1234', status: 'Confirmed', formattedTotal: '$99.99', itemCount: 2, orderDate: new Date('2026-01-15') },
  { id: 'order-eeff0011-5678', status: 'Confirmed', formattedTotal: '$45.00', itemCount: 1, orderDate: new Date('2026-01-20') },
];

function setupMocks(overrides: { isPending?: boolean } = {}) {
  vi.mocked(useCreatePayment).mockReturnValue({
    mutateAsync: vi.fn().mockResolvedValue('new-payment-id'),
    isPending: overrides.isPending ?? false,
  } as ReturnType<typeof useCreatePayment>);

  vi.mocked(useOrders).mockReturnValue({
    data: mockOrders,
    isLoading: false,
  } as ReturnType<typeof useOrders>);
}

beforeEach(() => {
  vi.clearAllMocks();
  setupMocks();
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('CreatePaymentModal', () => {
  it('does not render when isOpen is false', () => {
    render(<CreatePaymentModal isOpen={false} onClose={() => {}} />);

    expect(screen.queryByText(/create payment/i)).not.toBeInTheDocument();
  });

  it('renders the order dropdown populated with confirmed orders', () => {
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    // Both orders' formatted totals appear in the dropdown options
    expect(screen.getByText(/\$99\.99/)).toBeInTheDocument();
    expect(screen.getByText(/\$45\.00/)).toBeInTheDocument();
  });

  it('renders all payment method options', () => {
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    expect(screen.getByText('Credit Card')).toBeInTheDocument();
    expect(screen.getByText('Debit Card')).toBeInTheDocument();
    expect(screen.getByText('Bank Transfer')).toBeInTheDocument();
    expect(screen.getByText('Cash')).toBeInTheDocument();
    expect(screen.getByText('Digital Wallet')).toBeInTheDocument();
    expect(screen.getByText('PSE')).toBeInTheDocument();
  });

  it('"Create Payment" button is disabled when no order or method is selected', () => {
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    const createBtn = screen.getByRole('button', { name: /create payment/i });
    expect(createBtn).toBeDisabled();
  });

  it('shows the order total when an order is selected', async () => {
    const user = setupUser();
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    await user.selectOptions(screen.getAllByRole('combobox')[0], 'order-aabbccdd-1234');

    expect(screen.getByText('$99.99')).toBeInTheDocument();
  });

  it('hides the method detail field when Cash is selected', async () => {
    const user = setupUser();
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    // Select an order first
    await user.selectOptions(screen.getAllByRole('combobox')[0], 'order-aabbccdd-1234');
    // Select Cash
    await user.selectOptions(screen.getAllByRole('combobox')[1], 'Cash');

    expect(screen.queryByLabelText(/method detail/i)).not.toBeInTheDocument();
  });

  it('shows the method detail field when CreditCard is selected', async () => {
    const user = setupUser();
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    await user.selectOptions(screen.getAllByRole('combobox')[0], 'order-aabbccdd-1234');
    await user.selectOptions(screen.getAllByRole('combobox')[1], 'CreditCard');

    expect(screen.getByPlaceholderText(/\*{4}/)).toBeInTheDocument();
  });

  it('enables the Create button when both order and method are selected', async () => {
    const user = setupUser();
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    await user.selectOptions(screen.getAllByRole('combobox')[0], 'order-aabbccdd-1234');
    await user.selectOptions(screen.getAllByRole('combobox')[1], 'CreditCard');

    const createBtn = screen.getByRole('button', { name: /create payment/i });
    expect(createBtn).not.toBeDisabled();
  });

  it('calls mutateAsync with correct payload on submit', async () => {
    const mutateAsync = vi.fn().mockResolvedValue('new-payment-id');
    vi.mocked(useCreatePayment).mockReturnValue({
      mutateAsync,
      isPending: false,
    } as ReturnType<typeof useCreatePayment>);

    const user = setupUser();
    const onClose = vi.fn();
    render(<CreatePaymentModal isOpen onClose={onClose} />);

    await user.selectOptions(screen.getAllByRole('combobox')[0], 'order-aabbccdd-1234');
    await user.selectOptions(screen.getAllByRole('combobox')[1], 'CreditCard');

    await user.click(screen.getByRole('button', { name: /create payment/i }));

    expect(mutateAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        orderId: 'order-aabbccdd-1234',
        method: 'CreditCard',
      })
    );
  });

  it('closes the modal after successful creation', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreatePaymentModal isOpen onClose={onClose} />);

    await user.selectOptions(screen.getAllByRole('combobox')[0], 'order-aabbccdd-1234');
    await user.selectOptions(screen.getAllByRole('combobox')[1], 'CreditCard');
    await user.click(screen.getByRole('button', { name: /create payment/i }));

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose when Cancel is clicked', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreatePaymentModal isOpen onClose={onClose} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('shows loading state on Create button when mutation is pending', () => {
    setupMocks({ isPending: true });
    render(<CreatePaymentModal isOpen onClose={() => {}} />);

    const createBtn = screen.getByRole('button', { name: /create payment/i });
    expect(createBtn).toBeDisabled();
  });
});
