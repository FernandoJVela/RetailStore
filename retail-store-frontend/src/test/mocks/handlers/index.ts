import { productHandlers } from './products.handlers';
import { orderHandlers } from './orders.handlers';
import { inventoryHandlers } from './inventory.handlers';
import { customerHandlers } from './customers.handlers';
import { userHandlers } from './users.handlers';

/**
 * All default MSW handlers combined.
 * Each handler returns the "happy path" response for that endpoint.
 * Override in individual tests with server.use(...yourOverrides).
 */
export const handlers = [
  ...userHandlers,      // login must come before product/order/etc.
  ...productHandlers,
  ...orderHandlers,
  ...inventoryHandlers,
  ...customerHandlers,
];
