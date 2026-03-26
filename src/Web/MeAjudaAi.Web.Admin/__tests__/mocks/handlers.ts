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

// Admin handlers should match the SDK paths (usually /api/v1/...)
export const handlers = [
  // Providers
  http.get('/api/v1/providers', () =>
    HttpResponse.json({ data: { items: [mockProvider], totalPages: 1, totalItems: 1 } })
  ),
  http.get('/api/v1/providers/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockProvider, id: params.id as string } })
  ),
  http.post('/api/v1/providers', () => HttpResponse.json({ data: mockProvider }, { status: 201 })),
  http.put('/api/v1/providers/:id', async ({ params, request }) => {
    const body = await request.json() as Record<string, unknown>;
    // Removed strict mockProvider.id check to allow testing with different IDs
    if (!params.id) return new HttpResponse(null, { status: 404 });
    return HttpResponse.json({ data: { ...mockProvider, ...body, id: params.id as string } });
  }),
  http.delete('/api/v1/providers/:id', ({ params }) => {
    if (!params.id) return new HttpResponse(null, { status: 404 });
    return new HttpResponse(null, { status: 204 });
  }),
  http.post('/api/v1/providers/:id/activate', ({ params }) => {
    return HttpResponse.json({ data: { ...mockProvider, id: params.id as string, verificationStatus: EVerificationStatus.Verified } });
  }),
  http.post('/api/v1/providers/:id/deactivate', ({ params }) => {
    return HttpResponse.json({ data: { ...mockProvider, id: params.id as string, verificationStatus: EVerificationStatus.Suspended } });
  }),

  // Categories
  http.get('/api/v1/service-catalogs/categories', () => HttpResponse.json({ data: [mockCategory] })),
  http.get('/api/v1/service-catalogs/categories/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockCategory, id: params.id as string } })
  ),
  http.post('/api/v1/service-catalogs/categories', () => HttpResponse.json({ data: mockCategory }, { status: 201 })),
  http.put('/api/v1/service-catalogs/categories/:id', ({ params }) => 
    HttpResponse.json({ data: { ...mockCategory, id: params.id as string } })
  ),
  http.delete('/api/v1/service-catalogs/categories/:id', () => new HttpResponse(null, { status: 204 })),

  // Allowed Cities
  http.get('/api/v1/admin/allowed-cities', () => HttpResponse.json({ data: [mockCity] })),
  http.get('/api/v1/admin/allowed-cities/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockCity, id: params.id as string } })
  ),
  http.post('/api/v1/admin/allowed-cities', () => HttpResponse.json({ data: mockCity }, { status: 201 })),
  http.delete('/api/v1/admin/allowed-cities/:id', () => new HttpResponse(null, { status: 204 })),

  // Users
  http.get('/api/v1/users', () => HttpResponse.json({ data: [mockUser] })),
  http.get('/api/v1/users/:id', ({ params }) =>
    HttpResponse.json({ data: { ...mockUser, id: params.id as string } })
  ),
  http.delete('/api/v1/users/:id', () => new HttpResponse(null, { status: 204 })),
];
