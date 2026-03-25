import { http, HttpResponse } from 'msw';

const mockProvider = {
  id: 'provider-1',
  name: 'Prestador Teste',
  email: 'prestador@teste.com',
  verificationStatus: 1, // Pending
  type: 0, // Individual
};

const mockCategory = {
  id: 'category-1',
  name: 'Elétrica',
  description: 'Serviços elétricos',
  displayOrder: 1,
  isActive: true,
};

const mockCity = {
  id: 'city-1',
  cityName: 'Muriaé',
  stateCode: 'MG',
  isActive: true,
};

const mockUser = {
  id: 'user-1',
  name: 'Usuário Teste',
  email: 'usuario@teste.com',
};

export const handlers = [
  // Providers
  http.get('/api/providers', () =>
    HttpResponse.json({ data: { items: [mockProvider], totalPages: 1, totalItems: 1 } })
  ),
  http.get('/api/providers/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockProvider, id: params.id as string } })
  ),
  http.post('/api/providers', () => HttpResponse.json({ data: mockProvider }, { status: 201 })),
  http.put('/api/providers/:id', () => HttpResponse.json({ data: mockProvider })),
  http.delete('/api/providers/:id', () => new HttpResponse(null, { status: 204 })),
  http.post('/api/providers/:id/activate', () => HttpResponse.json({ data: mockProvider })),
  http.post('/api/providers/:id/deactivate', () => HttpResponse.json({ data: mockProvider })),

  // Categories
  http.get('/api/categories', () => HttpResponse.json({ data: [mockCategory] })),
  http.get('/api/categories/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockCategory, id: params.id as string } })
  ),
  http.post('/api/categories', () => HttpResponse.json({ data: mockCategory }, { status: 201 })),
  http.put('/api/categories/:id', () => HttpResponse.json({ data: mockCategory })),
  http.delete('/api/categories/:id', () => new HttpResponse(null, { status: 204 })),

  // Allowed Cities
  http.get('/api/allowed-cities', () => HttpResponse.json({ data: [mockCity] })),
  http.get('/api/allowed-cities/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockCity, id: params.id as string } })
  ),
  http.post('/api/allowed-cities', () => HttpResponse.json({ data: mockCity }, { status: 201 })),
  http.delete('/api/allowed-cities/:id', () => new HttpResponse(null, { status: 204 })),

  // Users
  http.get('/api/users', () => HttpResponse.json({ data: [mockUser] })),
  http.get('/api/users/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockUser, id: params.id as string } })
  ),
  http.delete('/api/users/:id', () => new HttpResponse(null, { status: 204 })),
];
