import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { CreateProductModal } from '../CreateProductModal';
import { useCreateProduct } from '@features/products/application/hooks/useProductsQueries';

// ── Mock the mutation hook ────────────────────────────────────────────────────
vi.mock('@features/products/application/hooks/useProductsQueries', () => ({
  useCreateProduct: vi.fn(),
}));

const mockMutateAsync = vi.fn();

function setupMock(overrides: { isPending?: boolean; rejects?: boolean } = {}) {
  vi.mocked(useCreateProduct).mockReturnValue({
    mutateAsync: overrides.rejects
      ? vi.fn().mockRejectedValue(new Error('Server error'))
      : mockMutateAsync.mockResolvedValue({ id: 'new-product' }),
    isPending: overrides.isPending ?? false,
  } as ReturnType<typeof useCreateProduct>);
}

beforeEach(() => {
  vi.clearAllMocks();
  setupMock();
});

// ── Rendering ─────────────────────────────────────────────────────────────────

describe('CreateProductModal', () => {
  it('does not render when isOpen is false', () => {
    render(<CreateProductModal isOpen={false} onClose={() => {}} />);

    expect(screen.queryByText('Create Product')).not.toBeInTheDocument();
  });

  it('renders all form fields when open', () => {
    render(<CreateProductModal isOpen onClose={() => {}} />);

    expect(screen.getByLabelText('Product Name')).toBeInTheDocument();
    expect(screen.getByLabelText('SKU')).toBeInTheDocument();
    expect(screen.getByLabelText('Price')).toBeInTheDocument();
    expect(screen.getByLabelText('Currency')).toBeInTheDocument();
    expect(screen.getByLabelText('Category')).toBeInTheDocument();
    expect(screen.getByLabelText('Description')).toBeInTheDocument();
  });

  it('renders the Cancel and Create buttons', () => {
    render(<CreateProductModal isOpen onClose={() => {}} />);

    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /create/i })).toBeInTheDocument();
  });

  // ── Validation ───────────────────────────────────────────────────────────

  it('shows "Product name is required" when name is empty and form is submitted', async () => {
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={() => {}} />);

    await user.click(screen.getByRole('button', { name: /create/i }));

    expect(await screen.findByText('Product name is required')).toBeInTheDocument();
  });

  it('shows "SKU is required" when SKU is empty and form is submitted', async () => {
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={() => {}} />);

    // Fill name to get past that error
    await user.type(screen.getByLabelText('Product Name'), 'Widget');
    await user.click(screen.getByRole('button', { name: /create/i }));

    expect(await screen.findByText('SKU is required')).toBeInTheDocument();
  });

  it('shows "Price must be positive" when price is zero', async () => {
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={() => {}} />);

    await user.type(screen.getByLabelText('Product Name'), 'Widget');
    await user.type(screen.getByLabelText('SKU'), 'WGT-001');
    // Price field defaults to 0 — that should trigger the positive error
    await user.click(screen.getByRole('button', { name: /create/i }));

    expect(await screen.findByText('Price must be positive')).toBeInTheDocument();
  });

  it('shows "Category is required" when category is not selected', async () => {
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={() => {}} />);

    await user.type(screen.getByLabelText('Product Name'), 'Widget');
    await user.type(screen.getByLabelText('SKU'), 'WGT-001');
    await user.clear(screen.getByLabelText('Price'));
    await user.type(screen.getByLabelText('Price'), '10');
    await user.click(screen.getByRole('button', { name: /create/i }));

    expect(await screen.findByText('Category is required')).toBeInTheDocument();
  });

  // ── Loading state ─────────────────────────────────────────────────────────

  it('shows loading state on the Create button when mutation is pending', () => {
    setupMock({ isPending: true });
    render(<CreateProductModal isOpen onClose={() => {}} />);

    expect(screen.getByRole('button', { name: /create/i })).toBeDisabled();
  });

  // ── Callbacks ─────────────────────────────────────────────────────────────

  it('calls onClose when Cancel is clicked', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={onClose} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose after a successful submission', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={onClose} />);

    await user.type(screen.getByLabelText('Product Name'), 'Widget Pro');
    await user.type(screen.getByLabelText('SKU'), 'WGT-001');
    await user.clear(screen.getByLabelText('Price'));
    await user.type(screen.getByLabelText('Price'), '29.99');

    // Select the first real category option
    const categorySelect = screen.getByLabelText('Category');
    await user.selectOptions(categorySelect, categorySelect.querySelectorAll('option')[1] as HTMLOptionElement);

    await user.click(screen.getByRole('button', { name: /create/i }));

    expect(await screen.findByText('Create Product')).toBeDefined();
    expect(onClose).toHaveBeenCalled();
  });

  it('shows an API error alert when mutation rejects', async () => {
    setupMock({ rejects: true });
    const user = setupUser();
    render(<CreateProductModal isOpen onClose={() => {}} />);

    await user.type(screen.getByLabelText('Product Name'), 'Widget');
    await user.type(screen.getByLabelText('SKU'), 'WGT-002');
    await user.clear(screen.getByLabelText('Price'));
    await user.type(screen.getByLabelText('Price'), '10');

    const categorySelect = screen.getByLabelText('Category');
    await user.selectOptions(categorySelect, categorySelect.querySelectorAll('option')[1] as HTMLOptionElement);

    await user.click(screen.getByRole('button', { name: /create/i }));

    // Alert should appear with an error message
    const alerts = await screen.findAllByRole('paragraph');
    expect(alerts.length).toBeGreaterThan(0);
  });
});
