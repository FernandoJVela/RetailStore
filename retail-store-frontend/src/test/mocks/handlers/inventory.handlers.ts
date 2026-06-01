import { http, HttpResponse } from 'msw';
import { mockInventoryItems, mockInventoryDetailDto } from '../data/fixtures';

const BASE = '/api/v1/inventory';

export const inventoryHandlers = [
  // GET /api/v1/inventory
  http.get(BASE, ({ request }) => {
    const url = new URL(request.url);
    const stockStatus = url.searchParams.get('stockStatus');
    const filtered = stockStatus
      ? mockInventoryItems.filter((i) => i.stockStatus === stockStatus)
      : mockInventoryItems;
    return HttpResponse.json(filtered);
  }),

  // GET /api/v1/inventory/low-stock — must be declared before the :productId route
  http.get(`${BASE}/low-stock`, () => {
    const lowStock = mockInventoryItems.filter(
      (i) => i.stockStatus === 'LowStock' || i.stockStatus === 'OutOfStock'
    );
    return HttpResponse.json(lowStock);
  }),

  // GET /api/v1/inventory/:productId
  http.get(`${BASE}/:productId`, ({ params }) => {
    const { productId } = params as { productId: string };
    if (productId === mockInventoryDetailDto.productId)
      return HttpResponse.json(mockInventoryDetailDto);
    return HttpResponse.json(
      { errorCode: 'INVENTORY_NOT_FOUND', detail: `No inventory for product ${productId}` },
      { status: 404 }
    );
  }),

  // POST /api/v1/inventory
  http.post(BASE, () =>
    HttpResponse.json('new-inventory-id', { status: 201 })
  ),

  // PUT /api/v1/inventory/:productId/add-stock
  http.put(`${BASE}/:productId/add-stock`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/inventory/:productId/remove-stock
  http.put(`${BASE}/:productId/remove-stock`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/inventory/:productId/reserve
  http.put(`${BASE}/:productId/reserve`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/inventory/:productId/release
  http.put(`${BASE}/:productId/release`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/inventory/:productId/fulfill
  http.put(`${BASE}/:productId/fulfill`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/inventory/:productId/adjust
  http.put(`${BASE}/:productId/adjust`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/inventory/:productId/reorder-threshold
  http.put(`${BASE}/:productId/reorder-threshold`, () =>
    new HttpResponse(null, { status: 204 })
  ),
];
