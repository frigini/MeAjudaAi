import { Page } from '@playwright/test';

export const mockProviders = {
  items: [
    { id: '550e8400-e29b-41d4-a716-446655440001', name: 'João Silva', verificationStatus: 3, type: 0 },
    { id: '550e8400-e29b-41d4-a716-446655440002', name: 'Maria Santos', verificationStatus: 2, type: 1 },
    { id: '550e8400-e29b-41d4-a716-446655440003', name: 'Pedro Costa', verificationStatus: 1, type: 0 },
    { id: '550e8400-e29b-41d4-a716-446655440004', name: 'Ana Oliveira', verificationStatus: 3, type: 2 },
    { id: '550e8400-e29b-41d4-a716-446655440005', name: 'Carlos Souza', verificationStatus: 0, type: 3 },
  ],
  totalPages: 1,
  totalCount: 5,
};

export const mockAllowedCities = {
  items: [
    { id: '1', cityName: 'São Paulo', stateSigla: 'SP', serviceRadiusKm: 50, isActive: true },
    { id: '2', cityName: 'Rio de Janeiro', stateSigla: 'RJ', serviceRadiusKm: 30, isActive: true },
    { id: '3', cityName: 'Belo Horizonte', stateSigla: 'MG', serviceRadiusKm: 40, isActive: true },
  ],
  totalPages: 1,
  totalCount: 3,
};

export const mockCategories = {
  items: [
    { id: '1', name: 'Serviços Domésticos', isActive: true, displayOrder: 1 },
    { id: '2', name: 'Manutenção e Reparos', isActive: true, displayOrder: 2 },
    { id: '3', name: 'Aulas e Treinamentos', isActive: true, displayOrder: 3 },
  ],
  totalPages: 1,
  totalCount: 3,
};

export const mockServices = {
  items: [
    { id: '1', name: 'Limpeza Residencial', categoryId: '1', isActive: true, price: 150 },
    { id: '2', name: 'Reparo Elétrico', categoryId: '2', isActive: true, price: 200 },
    { id: '3', name: 'Aula de Inglês', categoryId: '3', isActive: true, price: 100 },
  ],
  totalPages: 1,
  totalCount: 3,
};

export const mockCustomerProviders = {
  items: [
    { 
      providerId: '550e8400-e29b-41d4-a716-446655440001', 
      name: 'João Silva', 
      category: 'Serviços Domésticos', 
      averageRating: 4.5, 
      totalReviews: 10,
      city: 'São Paulo',
      state: 'SP',
      serviceIds: ['550e8400-e29b-41d4-a716-446655440101']
    },
    { 
      providerId: '550e8400-e29b-41d4-a716-446655440002', 
      name: 'Maria Santos', 
      category: 'Manutenção', 
      averageRating: 4.8, 
      totalReviews: 5,
      city: 'Rio de Janeiro',
      state: 'RJ',
      serviceIds: ['550e8400-e29b-41d4-a716-446655440102']
    },
  ],
  totalPages: 1,
  totalCount: 2,
};

export function setupAuthMocks(page: Page) {
  page.addListener('request', (request) => {
    const url = request.url();
    if (url.includes('x-mock-auth')) return;
  });
}

export function setupAdminMocks(page: Page) {
  page.route('**/api/auth/session', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        user: { id: 'test-admin', name: 'Admin Test', email: 'admin@test.com', roles: ['admin'] },
        accessToken: 'mock-token',
        expires: new Date(Date.now() + 3600000).toISOString(),
      }),
    });
  });

  // Providers list
  page.route('**/api/v1/providers**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockProviders), // ProvidersPage uses .items which is in mockProviders
    });
  });

  // Allowed Cities list
  page.route('**/api/v1/admin/allowed-cities**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ value: mockAllowedCities.items }), // AllowedCitiesPage uses .value
    });
  });

  // Categories list
  page.route('**/api/v1/service-catalogs/categories**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockCategories.items), // CategoriesPage uses direct array
    });
  });

  // Services list
  page.route('**/api/v1/service-catalogs/services**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ value: mockServices.items }), // ServicesPage uses .value
    });
  });

  page.route('**/api/v1/admin/documents**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [], totalPages: 1, totalCount: 0 }),
    });
  });

  page.route('**/api/v1/admin/settings**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [], totalPages: 1, totalCount: 0 }),
    });
  });
}

export function setupProviderMocks(page: Page) {
  page.route('**/api/auth/session', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        user: { id: 'test-provider', name: 'Provider Test', email: 'provider@test.com', roles: ['provider'] },
        accessToken: 'mock-token',
        expires: new Date(Date.now() + 3600000).toISOString(),
      }),
    });
  });

  page.route('**/api/v1/providers/me**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        data: {
          data: {
            id: '550e8400-e29b-41d4-a716-446655440000',
            name: 'Provider Test',
            isActive: true,
            verificationStatus: 3,
            type: 0,
            businessProfile: {
              description: 'Prestador de serviços de teste para E2E.',
              contactInfo: { 
                email: 'provider@test.com', 
                phoneNumber: '11999999999' 
              }
            },
            services: [
              { serviceId: '550e8400-e29b-41d4-a716-446655440101', serviceName: 'Limpeza Residencial' },
              { serviceId: '550e8400-e29b-41d4-a716-446655440102', serviceName: 'Reparo Elétrico' }
            ]
          },
          isSuccess: true
        },
        success: true,
        error: null
      }),
    });
  });

  // Also intercept requests to port 7002 (default API port for Provider app)
  page.route('http://localhost:7002/api/v1/providers/me', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        data: {
          data: {
            id: '550e8400-e29b-41d4-a716-446655440000',
            name: 'Provider Test',
            isActive: true,
            verificationStatus: 3,
            type: 0,
            businessProfile: {
              description: 'Prestador de serviços de teste para E2E.',
              contactInfo: { 
                email: 'provider@test.com', 
                phoneNumber: '11999999999' 
              }
            },
            services: [
              { serviceId: '550e8400-e29b-41d4-a716-446655440101', serviceName: 'Limpeza Residencial' },
              { serviceId: '550e8400-e29b-41d4-a716-446655440102', serviceName: 'Reparo Elétrico' }
            ]
          },
          isSuccess: true
        },
        success: true,
        error: null
      }),
    });
  });

  page.route('**/api/v1/providers/me/services**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockServices),
    });
  });

  page.route('**/api/v1/providers/me/reviews**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [], totalPages: 1, totalCount: 0 }),
    });
  });
}

export function setupCustomerMocks(page: Page) {
  page.route('**/api/auth/session', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        user: { 
          id: '550e8400-e29b-41d4-a716-446655440003', // Use a GUID
          name: 'Customer Test', 
          email: 'customer@test.com', 
          roles: ['user'] 
        },
        accessToken: 'mock-token',
        expires: new Date(Date.now() + 3600000).toISOString(),
      }),
    });
  });

  // Mock for Profile Page (authenticatedFetch expecting .value)
  page.route('**/api/v1/users/550e8400-e29b-41d4-a716-446655440003', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        value: {
          id: '550e8400-e29b-41d4-a716-446655440003',
          firstName: 'Customer',
          lastName: 'Test',
          fullName: 'Customer Test',
          email: 'customer@test.com',
          userType: 0
        }
      }),
    });
  });

  // SDK apiProvidersGet4 DOES NOT unwrap .value automatically unless configured
  // Based on current SearchPage.tsx logic, it expects items at root of data
  page.route('**/api/v1/search/providers**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          { 
            providerId: '550e8400-e29b-41d4-a716-446655440001', 
            name: 'João Silva', 
            category: 'Serviços Domésticos', 
            averageRating: 4.5, 
            totalReviews: 10,
            city: 'Muriaé',
            state: 'MG',
            serviceIds: ['550e8400-e29b-41d4-a716-446655440101']
          },
          { 
            providerId: '550e8400-e29b-41d4-a716-446655440002', 
            name: 'Maria Santos', 
            category: 'Manutenção', 
            averageRating: 4.8, 
            totalReviews: 5,
            city: 'Itaperuna',
            state: 'RJ',
            serviceIds: ['550e8400-e29b-41d4-a716-446655440102']
          },
        ],
        totalPages: 1,
        totalCount: 2,
      }),
    });
  });

  page.route('**/api/v1/service-catalogs/categories**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockCategories.items),
    });
  });

  page.route('**/api/v1/providers/**/public', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        data: {
          id: '550e8400-e29b-41d4-a716-446655440001',
          name: 'João Silva',
          verificationStatus: 3,
          type: 0,
          fantasyName: 'João Silva',
          description: 'Prestador de serviços de teste para E2E.',
          city: 'Muriaé',
          state: 'MG',
          rating: 4.5,
          reviewCount: 10,
          phoneNumbers: ['11999999999'],
          services: [
            { id: '550e8400-e29b-41d4-a716-446655440101', name: 'Limpeza Residencial' },
            { id: '550e8400-e29b-41d4-a716-446655440102', name: 'Reparo Elétrico' }
          ],
          email: 'provider@test.com'
        }
      }),
    });
  });

  page.route('**/api/v1/providers/*/services**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: mockServices.items }),
    });
  });
}