import { renderHook, waitFor } from '@/test/test-utils';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createTestQueryClient } from '@/test/test-utils';
import { useInventory, useInventoryDetail, useLowStock } from '../useInventoryQueries';
import { inventoryRepository } from '@features/inventory/infrastructure/inventory.repository';
import type { ReactNode } from 'react';

// ── Mock the repository ───────────────────────────────────────────────────────
vi.mock('@features/inventory/infrastructure/inventory.repository', () => ({
  inventoryRepository: {
    getAll: vi.fn(),
    getByProduct: vi.fn(),
    getLowStock: vi.fn(),
    create: vi.fn(),
    addStock: vi.fn(),
    removeStock: vi.fn(),
    adjustStock: vi.fn(),
    updateThreshold: vi.fn(),
  },
}));

// ── QueryClient wrapper ───────────────────────────────────────────────────────
function makeWrapper(qc?: QueryClient) {
  const client = qc ?? createTestQueryClient();
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
  };
}

// ── Test data ─────────────────────────────────────────────────────────────────
const mockItems = [
  {
    id: 'inv-1', productId: 'p-1', productName: 'Widget', sku: 'W-001',
    quantityOnHand: 50, reservedQuantity: 0, availableQuantity: 50,
    reorderThreshold: 10, stockStatus: 'InStock' as const,
  },
];

const mockDetail = {
  id: 'inv-1', productId: 'p-1', productName: 'Widget', sku: 'W-001',
  quantityOnHand: 50, reservedQuantity: 0, availableQuantity: 50,
  reorderThreshold: 10, stockStatus: 'InStock' as const,
  createdAt: new Date().toISOString(), updatedAt: null,
};

beforeEach(() => {
  vi.clearAllMocks();
});

// ── useInventory ──────────────────────────────────────────────────────────────

describe('useInventory', () => {
  it('starts in loading state then returns data from the repository', async () => {
    vi.mocked(inventoryRepository.getAll).mockResolvedValue(mockItems);

    const { result } = renderHook(() => useInventory(), { wrapper: makeWrapper() });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockItems);
  });

  it('calls the repository with no params when none provided', async () => {
    vi.mocked(inventoryRepository.getAll).mockResolvedValue([]);

    const { result } = renderHook(() => useInventory(), { wrapper: makeWrapper() });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(inventoryRepository.getAll).toHaveBeenCalledWith(undefined);
  });

  it('passes stockStatus filter to the repository', async () => {
    vi.mocked(inventoryRepository.getAll).mockResolvedValue([]);

    const { result } = renderHook(
      () => useInventory({ stockStatus: 'LowStock' }),
      { wrapper: makeWrapper() }
    );
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(inventoryRepository.getAll).toHaveBeenCalledWith({ stockStatus: 'LowStock' });
  });

  it('reflects isError state when the repository rejects', async () => {
    vi.mocked(inventoryRepository.getAll).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useInventory(), { wrapper: makeWrapper() });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

// ── useInventoryDetail ────────────────────────────────────────────────────────

describe('useInventoryDetail', () => {
  it('is disabled (not fetching) when productId is empty', () => {
    const { result } = renderHook(
      () => useInventoryDetail(''),
      { wrapper: makeWrapper() }
    );

    // enabled: false → query never fires; fetchStatus is 'idle'
    expect(result.current.fetchStatus).toBe('idle');
    expect(inventoryRepository.getByProduct).not.toHaveBeenCalled();
  });

  it('fetches and returns detail when productId is provided', async () => {
    vi.mocked(inventoryRepository.getByProduct).mockResolvedValue(mockDetail);

    const { result } = renderHook(
      () => useInventoryDetail('p-1'),
      { wrapper: makeWrapper() }
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockDetail);
    expect(inventoryRepository.getByProduct).toHaveBeenCalledWith('p-1');
  });
});

// ── useLowStock ───────────────────────────────────────────────────────────────

describe('useLowStock', () => {
  it('returns low-stock items from the repository', async () => {
    const lowItem = { ...mockItems[0], stockStatus: 'LowStock' as const, quantityOnHand: 5 };
    vi.mocked(inventoryRepository.getLowStock).mockResolvedValue([lowItem]);

    const { result } = renderHook(() => useLowStock(), { wrapper: makeWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toHaveLength(1);
    expect(result.current.data![0].stockStatus).toBe('LowStock');
  });
});
