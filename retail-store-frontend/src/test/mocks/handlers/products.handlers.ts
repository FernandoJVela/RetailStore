import { http, HttpResponse } from 'msw';
import {
  mockProducts,
  mockProductDetailDto,
  mockProductDto,
} from '../data/fixtures';

const BASE = '/api/v1/products';

export const productHandlers = [
  // GET /api/v1/products
  http.get(BASE, () => HttpResponse.json(mockProducts)),

  // GET /api/v1/products/:id
  http.get(`${BASE}/:id`, ({ params }) => {
    const { id } = params as { id: string };
    if (id === mockProductDto.id) return HttpResponse.json(mockProductDetailDto);
    return HttpResponse.json(
      { errorCode: 'PRODUCT_NOT_FOUND', detail: `Product ${id} not found` },
      { status: 404 }
    );
  }),

  // GET /api/v1/products/category/:category
  http.get(`${BASE}/category/:category`, ({ params }) => {
    const { category } = params as { category: string };
    const filtered = mockProducts.filter((p) => p.category === category);
    return HttpResponse.json(filtered);
  }),

  // POST /api/v1/products
  http.post(BASE, () =>
    HttpResponse.json('new-product-id', { status: 201 })
  ),

  // PUT /api/v1/products/:id (update details)
  http.put(`${BASE}/:id`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/products/:id/price
  http.put(`${BASE}/:id/price`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/products/:id/deactivate
  http.put(`${BASE}/:id/deactivate`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/products/:id/reactivate
  http.put(`${BASE}/:id/reactivate`, () => new HttpResponse(null, { status: 204 })),
];
