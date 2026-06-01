/**
 * Order creation workflow integration tests.
 * Uses the REAL useCreateOrder / useCustomers / useProducts hooks + MSW.
 * Verifies: API loads customers+products, user builds an order, POST is called.
 */
import { render, screen, waitFor } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';
import { CreateOrderModal } from '../CreateOrderModal';
import { mockCustomers, mockProducts } from '@/test/mocks/data/fixtures';

// ── Server lifecycle ──────────────────────────────────────────────────────────
beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

// ── Helper: wait until the product select has real options loaded ──────────────
async function getLoadedProductSelect() {
  return waitFor(() => {
    const selects = screen.getAllByRole('combobox');
    const ps = selects[1];
    // More than 1 option means the placeholder + at least one real product
    if (ps.querySelectorAll('option').length <= 1)
      throw new Error('Product options not yet loaded');
    return ps;
  });
}

// ── Helper: add one product item to the order ─────────────────────────────────
async function addProductItem(user: ReturnType<typeof setupUser>) {
  const productSelect = await getLoadedProductSelect();
  // Select by HTMLOptionElement reference — avoids exact-text/value matching issues.
  // Option text is "{name} ({formattedPrice})" which differs from the raw name string.
  const firstProductOption = productSelect.querySelectorAll('option')[1] as HTMLOptionElement;
  await user.selectOptions(productSelect, firstProductOption);
  const addBtn = screen.getAllByRole('button').find(
    (b) => !b.hasAttribute('disabled') && b.className.includes('outline')
  )!;
  await user.click(addBtn);
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('Create order workflow', () => {
  it('loads and renders customer options from MSW', async () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    await waitFor(() => {
      expect(screen.getByText(`${mockCustomers[0].fullName} (${mockCustomers[0].email})`)).toBeInTheDocument();
    });
  });

  it('loads and renders product options in the product select', async () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    const productSelect = await getLoadedProductSelect();
    expect(productSelect.textContent).toContain(mockProducts[0].name);
  });

  it('Create Order button is disabled until a customer and at least one item are selected', async () => {
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    await waitFor(() =>
      screen.getByText(`${mockCustomers[0].fullName} (${mockCustomers[0].email})`)
    );

    expect(screen.getByRole('button', { name: /create order/i })).toBeDisabled();
  });

  it('adds an item to the line-item list when product is selected and + is clicked', async () => {
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    await addProductItem(user);

    // The line item shows the product name from the domain model
    expect(screen.getByText('Widget Pro')).toBeInTheDocument();
  });

  it('submits POST /api/v1/orders with correct payload and calls onClose', async () => {
    let capturedBody: unknown = null;
    server.use(
      http.post('/api/v1/orders', async ({ request }) => {
        capturedBody = await request.json();
        return HttpResponse.json('new-order-id', { status: 201 });
      })
    );
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={onClose} />);

    // Wait for customers to load then select one
    await waitFor(() =>
      screen.getByText(`${mockCustomers[0].fullName} (${mockCustomers[0].email})`)
    );
    const selects = screen.getAllByRole('combobox');
    await user.selectOptions(selects[0], mockCustomers[0].id);

    // Add a product
    await addProductItem(user);

    // Submit
    await user.click(screen.getByRole('button', { name: /create order/i }));

    await waitFor(() => expect(onClose).toHaveBeenCalledOnce());

    const body = capturedBody as { customerId: string; items: { productId: string; quantity: number }[] };
    expect(body.customerId).toBe(mockCustomers[0].id);
    expect(body.items).toHaveLength(1);
    expect(body.items[0].productId).toBe(mockProducts[0].id);
    expect(body.items[0].quantity).toBeGreaterThanOrEqual(1);
  });

  it('shows an API error alert when POST /api/v1/orders fails', async () => {
    server.use(
      http.post('/api/v1/orders', () =>
        HttpResponse.json(
          { detail: 'Insufficient stock for one or more items' },
          { status: 422 }
        )
      )
    );
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={() => {}} />);

    await waitFor(() =>
      screen.getByText(`${mockCustomers[0].fullName} (${mockCustomers[0].email})`)
    );
    const selects = screen.getAllByRole('combobox');
    await user.selectOptions(selects[0], mockCustomers[0].id);

    await addProductItem(user);
    await user.click(screen.getByRole('button', { name: /create order/i }));

    await waitFor(() => {
      expect(screen.getByText('Insufficient stock for one or more items')).toBeInTheDocument();
    });
  });

  it('calls onClose when Cancel is clicked without posting to the API', async () => {
    let apiCalled = false;
    server.use(
      http.post('/api/v1/orders', () => {
        apiCalled = true;
        return HttpResponse.json('new-id', { status: 201 });
      })
    );
    const onClose = vi.fn();
    const user = setupUser();
    render(<CreateOrderModal isOpen onClose={onClose} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
    expect(apiCalled).toBe(false);
  });
});
