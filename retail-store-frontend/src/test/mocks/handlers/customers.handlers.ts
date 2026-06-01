import { http, HttpResponse } from 'msw';
import { mockCustomers, mockCustomerDetailDto } from '../data/fixtures';

const BASE = '/api/v1/customers';

export const customerHandlers = [
  // GET /api/v1/customers
  http.get(BASE, ({ request }) => {
    const url = new URL(request.url);
    const isActive = url.searchParams.get('isActive');
    const filtered =
      isActive !== null
        ? mockCustomers.filter((c) => c.isActive === (isActive === 'true'))
        : mockCustomers;
    return HttpResponse.json(filtered);
  }),

  // GET /api/v1/customers/:id
  http.get(`${BASE}/:id`, ({ params }) => {
    const { id } = params as { id: string };
    if (id === mockCustomerDetailDto.id) return HttpResponse.json(mockCustomerDetailDto);
    return HttpResponse.json(
      { errorCode: 'CUSTOMER_NOT_FOUND', detail: `Customer ${id} not found` },
      { status: 404 }
    );
  }),

  // GET /api/v1/customers/by-email/:email
  http.get(`${BASE}/by-email/:email`, ({ params }) => {
    const { email } = params as { email: string };
    const customer = mockCustomers.find((c) => c.email === decodeURIComponent(email));
    if (customer) return HttpResponse.json(customer);
    return HttpResponse.json(
      { errorCode: 'CUSTOMER_NOT_FOUND_BY_EMAIL', detail: 'Email not found' },
      { status: 404 }
    );
  }),

  // POST /api/v1/customers
  http.post(BASE, () =>
    HttpResponse.json('new-customer-id', { status: 201 })
  ),

  // PUT /api/v1/customers/:id (update name/phone)
  http.put(`${BASE}/:id`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/customers/:id/email
  http.put(`${BASE}/:id/email`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/customers/:id/address
  http.put(`${BASE}/:id/address`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/customers/:id/deactivate
  http.put(`${BASE}/:id/deactivate`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/customers/:id/reactivate
  http.put(`${BASE}/:id/reactivate`, () => new HttpResponse(null, { status: 204 })),
];
