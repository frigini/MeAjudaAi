import { http, HttpResponse } from 'msw';
import { EProviderType, EVerificationStatus, type VerificationStatus, type ProviderType } from '@/lib/types';

const mockProvider = {
  id: 'provider-1',
  name: 'Prestador Teste',
  email: 'prestador@teste.com',
  verificationStatus: EVerificationStatus.Pending as VerificationStatus,
  type: EProviderType.Individual as ProviderType,
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

const providersMap = new Map<string, typeof mockProvider>();
providersMap.set(mockProvider.id, { ...mockProvider });

const categoriesMap = new Map<string, typeof mockCategory>();
categoriesMap.set(mockCategory.id, { ...mockCategory });

const citiesMap = new Map<string, typeof mockCity>();
citiesMap.set(mockCity.id, { ...mockCity });

const usersMap = new Map<string, typeof mockUser>();
usersMap.set(mockUser.id, { ...mockUser });

export function resetMockData() {
  providersMap.clear();
  providersMap.set(mockProvider.id, { ...mockProvider });
  
  categoriesMap.clear();
  categoriesMap.set(mockCategory.id, { ...mockCategory });
  
  citiesMap.clear();
  citiesMap.set(mockCity.id, { ...mockCity });
  
  usersMap.clear();
  usersMap.set(mockUser.id, { ...mockUser });
}

// Admin handlers should match the SDK paths (usually /api/v1/...)
export const handlers = [
  // Providers
  http.get('/api/v1/providers', () =>
    HttpResponse.json({ data: { items: Array.from(providersMap.values()), totalPages: 1, totalItems: providersMap.size } })
  ),
  http.get('/api/v1/providers/:id', ({ params }) => {
    const provider = providersMap.get(params.id as string);
    if (!provider) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    return HttpResponse.json({ data: provider });
  }),
  http.post('/api/v1/providers', async ({ request }) => {
    try {
      const body = await request.json() as Record<string, unknown>;
      const newProvider = { ...mockProvider, ...body, id: `provider-${providersMap.size + 1}` };
      providersMap.set(newProvider.id, newProvider);
      return HttpResponse.json({ data: newProvider }, { status: 201 });
    } catch {
      return HttpResponse.json({ error: 'Invalid JSON' }, { status: 400 });
    }
  }),
  http.put('/api/v1/providers/:id', async ({ params, request }) => {
    try {
      const body = await request.json() as Record<string, unknown>;
      const existing = providersMap.get(params.id as string);
      if (!existing) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
      const updated = { ...existing, ...body, id: params.id as string };
      providersMap.set(params.id as string, updated);
      return HttpResponse.json({ data: updated });
    } catch {
      return HttpResponse.json({ error: 'Bad Request', message: 'Invalid JSON' }, { status: 400 });
    }
  }),
  http.delete('/api/v1/providers/:id', ({ params }) => {
    if (!providersMap.has(params.id as string)) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    providersMap.delete(params.id as string);
    return new HttpResponse(null, { status: 204 });
  }),
  http.post('/api/v1/providers/:id/activate', ({ params }) => {
    const provider = providersMap.get(params.id as string);
    if (!provider) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    const updated = { ...provider, verificationStatus: EVerificationStatus.Verified };
    providersMap.set(params.id as string, updated);
    return HttpResponse.json({ data: updated });
  }),
  http.post('/api/v1/providers/:id/deactivate', ({ params }) => {
    const provider = providersMap.get(params.id as string);
    if (!provider) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    const updated = { ...provider, verificationStatus: EVerificationStatus.Suspended };
    providersMap.set(params.id as string, updated);
    return HttpResponse.json({ data: updated });
  }),

  // Categories
  http.get('/api/v1/service-catalogs/categories', () => HttpResponse.json({ data: Array.from(categoriesMap.values()) })),
  http.get('/api/v1/service-catalogs/categories/:id', ({ params }) => {
    const category = categoriesMap.get(params.id as string);
    if (!category) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    return HttpResponse.json({ data: category });
  }),
  http.post('/api/v1/service-catalogs/categories', async ({ request }) => {
    try {
      const body = await request.json() as Record<string, unknown>;
      const newCategory = { ...mockCategory, ...body, id: `category-${categoriesMap.size + 1}` };
      categoriesMap.set(newCategory.id, newCategory);
      return HttpResponse.json({ data: newCategory }, { status: 201 });
    } catch {
      return HttpResponse.json({ error: 'Invalid JSON' }, { status: 400 });
    }
  }),
  http.put('/api/v1/service-catalogs/categories/:id', async ({ params, request }) => {
    try {
      const category = categoriesMap.get(params.id as string);
      if (!category) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
      const body = await request.json() as Record<string, unknown>;
      const updated = { ...category, ...body, id: params.id as string };
      categoriesMap.set(params.id as string, updated);
      return HttpResponse.json({ data: updated });
    } catch {
      return HttpResponse.json({ error: 'Invalid JSON' }, { status: 400 });
    }
  }),
  http.delete('/api/v1/service-catalogs/categories/:id', ({ params }) => {
    if (!categoriesMap.has(params.id as string)) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    categoriesMap.delete(params.id as string);
    return new HttpResponse(null, { status: 204 });
  }),

  // Allowed Cities
  http.get('/api/v1/admin/allowed-cities', () => HttpResponse.json({ data: Array.from(citiesMap.values()) })),
  http.get('/api/v1/admin/allowed-cities/:id', ({ params }) => {
    const city = citiesMap.get(params.id as string);
    if (!city) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    return HttpResponse.json({ data: city });
  }),
  http.post('/api/v1/admin/allowed-cities', async ({ request }) => {
    try {
      const body = await request.json() as Record<string, unknown>;
      const newCity = { ...mockCity, ...body, id: `city-${citiesMap.size + 1}` };
      citiesMap.set(newCity.id, newCity);
      return HttpResponse.json({ data: newCity }, { status: 201 });
    } catch {
      return HttpResponse.json({ error: 'Invalid JSON' }, { status: 400 });
    }
  }),
  http.delete('/api/v1/admin/allowed-cities/:id', ({ params }) => {
    if (!citiesMap.has(params.id as string)) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    citiesMap.delete(params.id as string);
    return new HttpResponse(null, { status: 204 });
  }),

  // Users
  http.get('/api/v1/users', () => HttpResponse.json({ data: Array.from(usersMap.values()) })),
  http.get('/api/v1/users/:id', ({ params }) => {
    const user = usersMap.get(params.id as string);
    if (!user) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    return HttpResponse.json({ data: user });
  }),
  http.delete('/api/v1/users/:id', ({ params }) => {
    if (!usersMap.has(params.id as string)) return HttpResponse.json({ error: 'Not Found' }, { status: 404 });
    usersMap.delete(params.id as string);
    return new HttpResponse(null, { status: 204 });
  }),
];
