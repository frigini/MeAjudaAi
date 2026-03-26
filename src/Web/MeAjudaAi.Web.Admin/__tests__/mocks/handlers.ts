import { http, HttpResponse } from 'msw';
import { EProviderType, EVerificationStatus } from '@/lib/types';

const mockProvider = {
  id: 'provider-1',
  name: 'Prestador Teste',
  email: 'prestador@teste.com',
  verificationStatus: EVerificationStatus.Pending,
  type: EProviderType.Individual,
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
  http.put('/api/providers/:id', async ({ params, request }) => {
    const body = await request.json() as any;
    if (params.id !== mockProvider.id) return new HttpResponse(null, { status: 404 });
    if (!body.name) return new HttpResponse(null, { status: 400 });
    return HttpResponse.json({ data: { ...mockProvider, ...body } });
  }),
  http.delete('/api/providers/:id', ({ params }) => {
    if (params.id !== mockProvider.id) return new HttpResponse(null, { status: 404 });
    return new HttpResponse(null, { status: 204 });
  }),
  http.post('/api/providers/:id/activate', ({ params }) => {
    if (params.id !== mockProvider.id) return new HttpResponse(null, { status: 404 });
    return HttpResponse.json({ data: { ...mockProvider, verificationStatus: EVerificationStatus.Verified } });
  }),
  http.post('/api/providers/:id/deactivate', ({ params }) => {
    if (params.id !== mockProvider.id) return new HttpResponse(null, { status: 404 });
    return HttpResponse.json({ data: { ...mockProvider, verificationStatus: EVerificationStatus.Suspended } });
  }),

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
