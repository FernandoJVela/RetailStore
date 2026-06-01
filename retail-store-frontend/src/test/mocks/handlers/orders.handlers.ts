import { http, HttpResponse } from 'msw';
import { mockOrders, mockOrderDetailDto } from '../data/fixtures';

const BASE = '/api/v1/orders';

export const orderHandlers = [
  // GET /api/v1/orders
  http.get(BASE, ({ request }) => {
    const url = new URL(request.url);
    const status = url.searchParams.get('status');
    const filtered = status
      ? mockOrders.filter((o) => o.status === status)
      : mockOrders;
    return HttpResponse.json(filtered);
  }),

  // GET /api/v1/orders/:id
  http.get(`${BASE}/:id`, ({ params }) => {
    const { id } = params as { id: string };
    if (id === mockOrderDetailDto.id) return HttpResponse.json(mockOrderDetailDto);
    return HttpResponse.json(
      { errorCode: 'ORDER_NOT_FOUND', detail: `Order ${id} not found` },
      { status: 404 }
    );
  }),

  // POST /api/v1/orders
  http.post(BASE, () =>
    HttpResponse.json('new-order-id', { status: 201 })
  ),

  // POST /api/v1/orders/:orderId/items
  http.post(`${BASE}/:orderId/items`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // DELETE /api/v1/orders/:orderId/items/:productId
  http.delete(`${BASE}/:orderId/items/:productId`, () =>
    new HttpResponse(null, { status: 204 })
  ),

  // PUT /api/v1/orders/:id/confirm
  http.put(`${BASE}/:id/confirm`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/orders/:id/complete
  http.put(`${BASE}/:id/complete`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/orders/:id/cancel
  http.put(`${BASE}/:id/cancel`, () => new HttpResponse(null, { status: 204 })),
];
