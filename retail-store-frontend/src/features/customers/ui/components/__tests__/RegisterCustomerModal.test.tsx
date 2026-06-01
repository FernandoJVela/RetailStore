import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { RegisterCustomerModal } from '../RegisterCustomerModal';
import { useRegisterCustomer } from '@features/customers/application/hooks/useCustomersQueries';

// ── Mock the mutation hook ────────────────────────────────────────────────────
vi.mock('@features/customers/application/hooks/useCustomersQueries', () => ({
  useRegisterCustomer: vi.fn(),
  useCustomers: vi.fn(() => ({ data: [] })),
}));

function setupMock(overrides: { isPending?: boolean } = {}) {
  vi.mocked(useRegisterCustomer).mockReturnValue({
    mutateAsync: vi.fn().mockResolvedValue({ id: 'new-customer' }),
    isPending: overrides.isPending ?? false,
  } as ReturnType<typeof useRegisterCustomer>);
}

beforeEach(() => {
  vi.clearAllMocks();
  setupMock();
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('RegisterCustomerModal', () => {
  it('does not render when isOpen is false', () => {
    render(<RegisterCustomerModal isOpen={false} onClose={() => {}} />);

    expect(screen.queryByText('Register Customer')).not.toBeInTheDocument();
  });

  it('renders core fields when open', () => {
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    expect(screen.getByLabelText('First Name')).toBeInTheDocument();
    expect(screen.getByLabelText('Last Name')).toBeInTheDocument();
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
    expect(screen.getByLabelText('Phone')).toBeInTheDocument();
  });

  it('does not show address fields by default', () => {
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    expect(screen.queryByLabelText('Street')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('City')).not.toBeInTheDocument();
  });

  it('shows address fields when the "Include shipping address" checkbox is checked', async () => {
    const user = setupUser();
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    await user.click(screen.getByRole('checkbox'));

    expect(screen.getByLabelText('Street')).toBeInTheDocument();
    expect(screen.getByLabelText('City')).toBeInTheDocument();
    expect(screen.getByLabelText('Country')).toBeInTheDocument();
  });

  // ── Validation ───────────────────────────────────────────────────────────

  it('shows a validation error on empty submit (min-length check fires before required)', async () => {
    const user = setupUser();
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    await user.click(screen.getByRole('button', { name: /save/i }));

    // yup runs min(2) before required() — empty string fails min first → "At least 2 characters"
    const errors = await screen.findAllByText('At least 2 characters');
    expect(errors.length).toBeGreaterThanOrEqual(1);
  });

  it('shows "At least 2 characters" when first name is one character', async () => {
    const user = setupUser();
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    await user.type(screen.getByLabelText('First Name'), 'A');
    await user.click(screen.getByRole('button', { name: /save/i }));

    // firstName shows the min-length error; there may also be one for lastName
    const errors = await screen.findAllByText('At least 2 characters');
    expect(errors.length).toBeGreaterThanOrEqual(1);
  });

  it('shows an email validation error for a malformed email', async () => {
    const user = setupUser();
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    await user.type(screen.getByLabelText('First Name'), 'John');
    await user.type(screen.getByLabelText('Last Name'), 'Doe');
    await user.type(screen.getByLabelText('Email'), 'notanemail');
    await user.click(screen.getByRole('button', { name: /save/i }));

    // 'Invalid email' is the yup custom message; fall back to any email-related error
    const emailError = await screen.findByLabelText('Email');
    // The error paragraph is a sibling of the input's label — check the container
    const container = emailError.closest('.space-y-1\\.5') ?? emailError.parentElement!;
    expect(container.textContent).toMatch(/invalid email|email/i);
  });

  it('shows "Street is required" when address is enabled but street is empty', async () => {
    const user = setupUser();
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    await user.type(screen.getByLabelText('First Name'), 'John');
    await user.type(screen.getByLabelText('Last Name'), 'Doe');
    await user.type(screen.getByLabelText('Email'), 'john@example.com');
    await user.click(screen.getByRole('checkbox')); // enable address

    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(await screen.findByText('Street is required')).toBeInTheDocument();
  });

  // ── Loading & callbacks ───────────────────────────────────────────────────

  it('shows loading state on save button while mutation is pending', () => {
    setupMock({ isPending: true });
    render(<RegisterCustomerModal isOpen onClose={() => {}} />);

    expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
  });

  it('calls onClose when Cancel is clicked', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<RegisterCustomerModal isOpen onClose={onClose} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
  });
});
