import { renderHook, act, waitFor } from '@/test/test-utils';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createTestQueryClient } from '@/test/test-utils';
import { useOrders, useOrder, useCreateOrder, useConfirmOrder, useCancelOrder } from '../useOrdersQueries';
import { ordersRepository } from '@features/orders/infrastructure/orders.repository';
import type { ReactNode } from 'react';

// ── Mock the repository ───────────────────────────────────────────────────────
vi.mock('@features/orders/infrastructure/orders.repository', () => ({
  ordersRepository: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    addItem: vi.fn(),
    removeItem: vi.fn(),
    confirm: vi.fn(),
    complete: vi.fn(),
    cancel: vi.fn(),
  },
}));

// ── Wrapper ───────────────────────────────────────────────────────────────────
function makeWrapper(qc?: QueryClient) {
  const client = qc ?? createTestQueryClient();
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
  };
}

// ── Test data ─────────────────────────────────────────────────────────────────
const mockOrder = {
  id: 'ord-1',
  customerId: 'cust-1',
  status: 'Draft' as const,
  orderDate: new Date().toISOString(),
  totalAmount: 100,
  itemCount: 2,
  completedAt: null,
  cancelledAt: null,
};

beforeEach(() => {
  vi.clearAllMocks();
});

// ── useOrders ─────────────────────────────────────────────────────────────────

describe('useOrders', () => {
  it('returns orders from the repository', async () => {
    vi.mocked(ordersRepository.getAll).mockResolvedValue([mockOrder]);

    const { result } = renderHook(() => useOrders(), { wrapper: makeWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toHaveLength(1);
    expect(result.current.data![0].id).toBe('ord-1');
  });

  it('passes the status filter to the repository', async () => {
    vi.mocked(ordersRepository.getAll).mockResolvedValue([]);

    const { result } = renderHook(
      () => useOrders({ status: 'Confirmed' }),
      { wrapper: makeWrapper() }
    );
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(ordersRepository.getAll).toHaveBeenCalledWith({ status: 'Confirmed' });
  });

  it('reflects isError when the repository rejects', async () => {
    vi.mocked(ordersRepository.getAll).mockRejectedValue(new Error('fetch failed'));

    const { result } = renderHook(() => useOrders(), { wrapper: makeWrapper() });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

// ── useOrder ──────────────────────────────────────────────────────────────────

describe('useOrder', () => {
  it('is disabled when id is empty', () => {
    const { result } = renderHook(
      () => useOrder(''),
      { wrapper: makeWrapper() }
    );

    expect(result.current.fetchStatus).toBe('idle');
    expect(ordersRepository.getById).not.toHaveBeenCalled();
  });

  it('fetches order detail when id is provided', async () => {
    const detail = {
      ...mockOrder,
      items: [{ id: 'item-1', productId: 'p-1', quantity: 2, unitPrice: 50, currency: 'USD', subtotal: 100 }],
    };
    vi.mocked(ordersRepository.getById).mockResolvedValue(detail);

    const { result } = renderHook(
      () => useOrder('ord-1'),
      { wrapper: makeWrapper() }
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(detail);
    expect(ordersRepository.getById).toHaveBeenCalledWith('ord-1');
  });
});

// ── useCreateOrder ────────────────────────────────────────────────────────────

describe('useCreateOrder', () => {
  it('calls the repository with the order data', async () => {
    vi.mocked(ordersRepository.create).mockResolvedValue('new-order-id');

    const { result } = renderHook(() => useCreateOrder(), { wrapper: makeWrapper() });

    await act(async () => {
      await result.current.mutateAsync({
        customerId: 'cust-1',
        items: [{ productId: 'p-1', quantity: 3 }],
      });
    });

    expect(ordersRepository.create).toHaveBeenCalledWith({
      customerId: 'cust-1',
      items: [{ productId: 'p-1', quantity: 3 }],
    });
  });

  it('starts not pending', () => {
    const { result } = renderHook(() => useCreateOrder(), { wrapper: makeWrapper() });
    expect(result.current.isPending).toBe(false);
  });
});

// ── useConfirmOrder ───────────────────────────────────────────────────────────

describe('useConfirmOrder', () => {
  it('calls ordersRepository.confirm with the order id', async () => {
    vi.mocked(ordersRepository.confirm).mockResolvedValue(undefined);

    const { result } = renderHook(() => useConfirmOrder(), { wrapper: makeWrapper() });

    await act(async () => {
      await result.current.mutateAsync('ord-1');
    });

    expect(ordersRepository.confirm).toHaveBeenCalledWith('ord-1');
  });
});

// ── useCancelOrder ────────────────────────────────────────────────────────────

describe('useCancelOrder', () => {
  it('calls ordersRepository.cancel with id and reason', async () => {
    vi.mocked(ordersRepository.cancel).mockResolvedValue(undefined);

    const { result } = renderHook(() => useCancelOrder(), { wrapper: makeWrapper() });

    await act(async () => {
      await result.current.mutateAsync({ id: 'ord-1', reason: 'customer request' });
    });

    expect(ordersRepository.cancel).toHaveBeenCalledWith('ord-1', 'customer request');
  });
});
