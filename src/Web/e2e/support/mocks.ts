import { Page } from '@playwright/test';

export const mockProviders = {
  items: [
    { id: '1', name: 'João Silva', verificationStatus: 3, type: 0 },
    { id: '2', name: 'Maria Santos', verificationStatus: 2, type: 1 },
    { id: '3', name: 'Pedro Costa', verificationStatus: 1, type: 0 },
    { id: '4', name: 'Ana Oliveira', verificationStatus: 3, type: 2 },
    { id: '5', name: 'Carlos Souza', verificationStatus: 0, type: 3 },
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
    { id: '1', name: 'João Silva', category: 'Serviços Domésticos', rating: 4.5, city: 'São Paulo' },
    { id: '2', name: 'Maria Santos', category: 'Manutenção', rating: 4.8, city: 'Rio de Janeiro' },
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
        expires: new Date(Date.now() + 3600000).toISOString(),
      }),
    });
  });

  page.route('**/api/v1/admin/providers**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockProviders),
    });
  });

  page.route('**/api/v1/admin/allowed-cities**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockAllowedCities),
    });
  });

  page.route('**/api/v1/admin/categories**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockCategories),
    });
  });

  page.route('**/api/v1/admin/services**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockServices),
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
        expires: new Date(Date.now() + 3600000).toISOString(),
      }),
    });
  });

  page.route('**/api/v1/providers/me**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: 'test-provider',
        name: 'Provider Test',
        verificationStatus: 3,
        type: 0,
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
        user: { id: 'test-customer', name: 'Customer Test', email: 'customer@test.com', roles: ['user'] },
        expires: new Date(Date.now() + 3600000).toISOString(),
      }),
    });
  });

  page.route('**/api/v1/search**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockCustomerProviders),
    });
  });

  page.route('**/api/v1/categories**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockCategories),
    });
  });

  page.route('**/api/v1/providers/*/services**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockServices),
    });
  });
}