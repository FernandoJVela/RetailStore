import { http, HttpResponse } from 'msw';
import { mockLoginResponse, mockUserDto, mockRoleDto } from '../data/fixtures';

const BASE = '/api/v1/users';

export const userHandlers = [
  // POST /api/v1/users/login
  http.post(`${BASE}/login`, async ({ request }) => {
    const body = await request.json() as { email: string; password: string };
    // Simulate auth: only the fixture user gets a token
    if (body.email === 'testuser@example.com' && body.password === 'Password123!') {
      return HttpResponse.json(mockLoginResponse);
    }
    return HttpResponse.json(
      { errorCode: 'USER_INVALID_CREDENTIALS', detail: 'Email or password is incorrect.' },
      { status: 401 }
    );
  }),

  // POST /api/v1/users/register
  http.post(`${BASE}/register`, async ({ request }) => {
    const body = await request.json() as { email: string };
    // Simulate duplicate email conflict
    if (body.email === 'taken@example.com') {
      return HttpResponse.json(
        { errorCode: 'USER_DUPLICATE_EMAIL', detail: 'Email already in use.' },
        { status: 409 }
      );
    }
    return HttpResponse.json('new-user-id', { status: 201 });
  }),

  // GET /api/v1/users
  http.get(BASE, () => HttpResponse.json([mockUserDto])),

  // GET /api/v1/users/:id
  http.get(`${BASE}/:id`, ({ params }) => {
    const { id } = params as { id: string };
    if (id === mockUserDto.id) return HttpResponse.json(mockUserDto);
    return HttpResponse.json(
      { errorCode: 'USER_NOT_FOUND', detail: `User ${id} not found` },
      { status: 404 }
    );
  }),

  // GET /api/v1/users/roles
  http.get(`${BASE}/roles`, () => HttpResponse.json([mockRoleDto])),

  // POST /api/v1/users/:userId/roles
  http.post(`${BASE}/:userId/roles`, () => new HttpResponse(null, { status: 204 })),

  // DELETE /api/v1/users/:userId/roles/:roleId
  http.delete(`${BASE}/:userId/roles/:roleId`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/users/:id/deactivate
  http.put(`${BASE}/:id/deactivate`, () => new HttpResponse(null, { status: 204 })),

  // PUT /api/v1/users/:id/reactivate
  http.put(`${BASE}/:id/reactivate`, () => new HttpResponse(null, { status: 204 })),
];
