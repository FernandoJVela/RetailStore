/**
 * Stock action workflow integration tests.
 * Uses the REAL useAddStock / useAdjustStock / useUpdateThreshold hooks + MSW.
 * Verifies the full flow: form submit → API call → onClose triggered.
 */
import { render, screen, waitFor } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';
import { StockActionModal } from '../StockActionModal';
import type { InventoryItem } from '@features/inventory';

// ── Server lifecycle ──────────────────────────────────────────────────────────
beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

// ── Fixture item ──────────────────────────────────────────────────────────────
const mockItem: InventoryItem = {
  id: 'inv-1',
  productId: 'prod-1',
  productName: 'Widget Pro',
  sku: 'WGT-001',
  quantityOnHand: 50,
  reservedQuantity: 5,
  availableQuantity: 45,
  reorderThreshold: 10,
  stockStatus: 'InStock',
  stockHealthPercent: 100,
  isLowStock: false,
  isOutOfStock: false,
};

// ── Add Stock ─────────────────────────────────────────────────────────────────

describe('StockActionModal – add stock workflow', () => {
  it('renders the quantity field and current item context', () => {
    render(<StockActionModal item={mockItem} action="add" isOpen onClose={() => {}} />);

    expect(screen.getByText('Widget Pro')).toBeInTheDocument();
    expect(screen.getByLabelText('Quantity to add')).toBeInTheDocument();
  });

  it('submits the correct quantity to PUT /add-stock and calls onClose', async () => {
    let capturedBody: unknown = null;
    server.use(
      http.put('/api/v1/inventory/:productId/add-stock', async ({ request }) => {
        capturedBody = await request.json();
        return new HttpResponse(null, { status: 204 });
      })
    );
    const onClose = vi.fn();
    const user = setupUser();
    render(<StockActionModal item={mockItem} action="add" isOpen onClose={onClose} />);

    await user.clear(screen.getByLabelText('Quantity to add'));
    await user.type(screen.getByLabelText('Quantity to add'), '15');
    await user.click(screen.getByRole('button', { name: /confirm/i }));

    await waitFor(() => expect(onClose).toHaveBeenCalledOnce());
    expect((capturedBody as { quantity: number }).quantity).toBe(15);
  });

  it('shows an API error alert when the server rejects', async () => {
    server.use(
      http.put('/api/v1/inventory/:productId/add-stock', () =>
        HttpResponse.json(
          { detail: 'Insufficient stock capacity' },
          { status: 422 }
        )
      )
    );
    const user = setupUser();
    render(<StockActionModal item={mockItem} action="add" isOpen onClose={() => {}} />);

    await user.click(screen.getByRole('button', { name: /confirm/i }));

    await waitFor(() => {
      expect(screen.getByText('Insufficient stock capacity')).toBeInTheDocument();
    });
  });

  it('does not call the API when quantity fails validation (≤ 0)', async () => {
    let apiCalled = false;
    server.use(
      http.put('/api/v1/inventory/:productId/add-stock', () => {
        apiCalled = true;
        return new HttpResponse(null, { status: 204 });
      })
    );
    const user = setupUser();
    render(<StockActionModal item={mockItem} action="add" isOpen onClose={() => {}} />);

    await user.clear(screen.getByLabelText('Quantity to add'));
    await user.type(screen.getByLabelText('Quantity to add'), '0');
    await user.click(screen.getByRole('button', { name: /confirm/i }));

    await waitFor(() => {
      expect(screen.getByText('Must be positive')).toBeInTheDocument();
    });
    expect(apiCalled).toBe(false);
  });
});

// ── Adjust Stock ──────────────────────────────────────────────────────────────

describe('StockActionModal – adjust stock workflow', () => {
  it('renders newQuantity and reason fields', () => {
    render(<StockActionModal item={mockItem} action="adjust" isOpen onClose={() => {}} />);

    expect(screen.getByLabelText('New quantity on hand')).toBeInTheDocument();
    expect(screen.getByLabelText('Reason for adjustment')).toBeInTheDocument();
  });

  it('submits correct payload to PUT /adjust and calls onClose', async () => {
    let capturedBody: unknown = null;
    server.use(
      http.put('/api/v1/inventory/:productId/adjust', async ({ request }) => {
        capturedBody = await request.json();
        return new HttpResponse(null, { status: 204 });
      })
    );
    const onClose = vi.fn();
    const user = setupUser();
    render(<StockActionModal item={mockItem} action="adjust" isOpen onClose={onClose} />);

    await user.clear(screen.getByLabelText('New quantity on hand'));
    await user.type(screen.getByLabelText('New quantity on hand'), '30');
    await user.type(screen.getByLabelText('Reason for adjustment'), 'Physical count');
    await user.click(screen.getByRole('button', { name: /confirm/i }));

    await waitFor(() => expect(onClose).toHaveBeenCalledOnce());
    expect((capturedBody as { newQuantity: number; reason: string }).newQuantity).toBe(30);
    expect((capturedBody as { reason: string }).reason).toBe('Physical count');
  });
});

// ── Update Threshold ──────────────────────────────────────────────────────────

describe('StockActionModal – update threshold workflow', () => {
  it('renders the threshold field pre-filled with the current threshold', () => {
    render(<StockActionModal item={mockItem} action="threshold" isOpen onClose={() => {}} />);

    const input = screen.getByLabelText('New reorder threshold') as HTMLInputElement;
    expect(Number(input.value)).toBe(mockItem.reorderThreshold);
  });

  it('submits to PUT /reorder-threshold and calls onClose', async () => {
    let capturedBody: unknown = null;
    server.use(
      http.put('/api/v1/inventory/:productId/reorder-threshold', async ({ request }) => {
        capturedBody = await request.json();
        return new HttpResponse(null, { status: 204 });
      })
    );
    const onClose = vi.fn();
    const user = setupUser();
    render(<StockActionModal item={mockItem} action="threshold" isOpen onClose={onClose} />);

    await user.clear(screen.getByLabelText('New reorder threshold'));
    await user.type(screen.getByLabelText('New reorder threshold'), '20');
    await user.click(screen.getByRole('button', { name: /confirm/i }));

    await waitFor(() => expect(onClose).toHaveBeenCalledOnce());
    expect((capturedBody as { newThreshold: number }).newThreshold).toBe(20);
  });
});

// ── Cancel button ─────────────────────────────────────────────────────────────

describe('StockActionModal – cancel', () => {
  it('calls onClose when Cancel is clicked without any API call', async () => {
    let apiCalled = false;
    server.use(
      http.put('/api/v1/inventory/:productId/add-stock', () => {
        apiCalled = true;
        return new HttpResponse(null, { status: 204 });
      })
    );
    const onClose = vi.fn();
    const user = setupUser();
    render(<StockActionModal item={mockItem} action="add" isOpen onClose={onClose} />);

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onClose).toHaveBeenCalledOnce();
    expect(apiCalled).toBe(false);
  });
});
