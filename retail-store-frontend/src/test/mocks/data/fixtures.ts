/**
 * Shared mock data used across MSW handlers and individual tests.
 * Shape matches the API DTOs (what the backend actually returns).
 * Import and override in specific tests with server.use().
 */

// ── Products ──────────────────────────────────────────────────────────────────

export const mockProductDto = {
  id: 'prod-1',
  name: 'Widget Pro',
  sku: 'WGT-001',
  price: 29.99,
  currency: 'USD',
  category: 'Electronics',
  isActive: true,
};

export const mockProductDetailDto = {
  ...mockProductDto,
  description: 'A premium widget for all occasions.',
  createdAt: '2024-01-10T09:00:00Z',
  updatedAt: '2024-06-15T12:00:00Z',
};

export const mockProducts = [
  mockProductDto,
  { ...mockProductDto, id: 'prod-2', name: 'Gadget X', sku: 'GDG-001', price: 49.99 },
];

// ── Orders ────────────────────────────────────────────────────────────────────

export const mockOrderDto = {
  id: 'ord-1',
  customerId: 'cust-1',
  status: 'Draft',
  orderDate: '2024-06-15T10:00:00Z',
  totalAmount: 59.98,
  itemCount: 2,
  completedAt: null,
  cancelledAt: null,
};

export const mockOrderDetailDto = {
  id: 'ord-1',
  customerId: 'cust-1',
  status: 'Draft',
  orderDate: '2024-06-15T10:00:00Z',
  totalAmount: 59.98,
  completedAt: null,
  cancelledAt: null,
  items: [
    {
      id: 'item-1',
      productId: 'prod-1',
      quantity: 2,
      unitPrice: 29.99,
      currency: 'USD',
      subtotal: 59.98,
    },
  ],
};

export const mockOrders = [mockOrderDto];

// ── Inventory ─────────────────────────────────────────────────────────────────

export const mockInventoryItemDto = {
  id: 'inv-1',
  productId: 'prod-1',
  productName: 'Widget Pro',
  sku: 'WGT-001',
  quantityOnHand: 50,
  reservedQuantity: 5,
  availableQuantity: 45,
  reorderThreshold: 10,
  stockStatus: 'InStock',
};

export const mockInventoryDetailDto = {
  ...mockInventoryItemDto,
  createdAt: '2024-01-10T09:00:00Z',
  updatedAt: null,
};

export const mockInventoryItems = [
  mockInventoryItemDto,
  {
    ...mockInventoryItemDto,
    id: 'inv-2',
    productId: 'prod-2',
    productName: 'Gadget X',
    sku: 'GDG-001',
    quantityOnHand: 8,
    availableQuantity: 8,
    stockStatus: 'LowStock',
  },
];

// ── Customers ─────────────────────────────────────────────────────────────────

export const mockCustomerDto = {
  id: 'cust-1',
  firstName: 'Alice',
  lastName: 'Smith',
  fullName: 'Alice Smith',
  email: 'alice@example.com',
  phone: '+1 555-0100',
  isActive: true,
  createdAt: '2024-01-05T08:00:00Z',
};

export const mockCustomerDetailDto = {
  ...mockCustomerDto,
  shippingAddress: {
    street: '123 Main St',
    city: 'Springfield',
    state: 'IL',
    zipCode: '62701',
    country: 'US',
  },
  updatedAt: null,
};

export const mockCustomers = [
  mockCustomerDto,
  { ...mockCustomerDto, id: 'cust-2', firstName: 'Bob', lastName: 'Jones', fullName: 'Bob Jones', email: 'bob@example.com' },
];

// ── Users / Auth ──────────────────────────────────────────────────────────────

export const mockLoginResponse = {
  accessToken: 'mock.jwt.access-token',
  refreshToken: 'mock.refresh-token',
  expiresAt: '2099-01-01T00:00:00Z',
  userId: 'user-1',
  username: 'testuser',
  // Required by mapLoginResponse → useLogin → useAuthStore.login()
  email: 'testuser@example.com',
  roles: ['Staff'],
  permissions: ['products:write', 'orders:write', 'inventory:adjust'],
};

export const mockUserDto = {
  id: 'user-1',
  username: 'testuser',
  email: 'testuser@example.com',
  isActive: true,
  roles: [],
};

export const mockRoleDto = {
  id: 'role-1',
  name: 'Admin',
  description: 'Full system access',
  permissions: ['*:*'],
};
