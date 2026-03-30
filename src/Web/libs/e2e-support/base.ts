import { test as base, Page } from '@playwright/test';

export { expect } from '@playwright/test';

export interface TestFixtures {
  page: Page;
}

export const test = base;

export { base };

const isCI = process.env.CI === 'true' || 
  process.env.NEXT_PUBLIC_CI === 'true' || 
  process.env.KEYCLOAK_ADMIN_CLIENT_ID === 'ci-build-placeholder' ||
  process.env.KEYCLOAK_CLIENT_ID === 'ci-build-placeholder';

const mockSession = {
  user: {
    id: 'test-admin-id',
    name: 'Test Admin',
    email: 'admin@test.com',
    image: null,
    roles: ['admin'],
  },
  expires: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
};

async function setupAuthMocks(page: Page) {
  await page.route('**/api/auth/session', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockSession),
    });
  });

  await page.route('**/api/auth/csrf', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ csrfToken: 'mock-csrf-token' }),
    });
  });

  await page.route('**/api/auth/providers', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        keycloak: {
          id: 'keycloak',
          name: 'Keycloak',
          type: 'oauth',
          signinUrl: '/api/auth/signin/keycloak?callbackUrl=/dashboard',
          callbackUrl: '/api/auth/callback/keycloak',
        },
      }),
    });
  });

  await page.route('**/api/auth/signin**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ url: '/dashboard' }),
    });
  });

  const mockProviders = {
    items: [
      { id: '1', name: 'Provider 1', verificationStatus: 2, type: 0 },
      { id: '2', name: 'Provider 2', verificationStatus: 2, type: 1 },
      { id: '3', name: 'Provider 3', verificationStatus: 1, type: 2 },
      { id: '4', name: 'Provider 4', verificationStatus: 3, type: 0 },
      { id: '5', name: 'Provider 5', verificationStatus: 0, type: 3 },
      { id: '6', name: 'Provider 6', verificationStatus: 2, type: 1 },
      { id: '7', name: 'Provider 7', verificationStatus: 1, type: 2 },
      { id: '8', name: 'Provider 8', verificationStatus: 4, type: 0 },
    ],
    totalPages: 1,
    totalCount: 8,
  };

  await page.route('**/api/v1/providers**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockProviders),
    });
  });

  await page.route('**/api/v1/locations/allowed-cities**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          { id: '1', city: 'São Paulo', state: 'SP', serviceRadiusKm: 50 },
          { id: '2', city: 'Rio de Janeiro', state: 'RJ', serviceRadiusKm: 30 },
        ],
        totalPages: 1,
        totalCount: 2,
      }),
    });
  });

  await page.route('**/api/v1/service-catalogs/categories**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          { id: '1', name: 'Category 1', isActive: true, displayOrder: 1 },
          { id: '2', name: 'Category 2', isActive: true, displayOrder: 2 },
        ],
        totalPages: 1,
        totalCount: 2,
      }),
    });
  });

  await page.route('**/api/v1/service-catalogs/services**', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          { id: '1', name: 'Service 1', categoryId: '1', isActive: true, price: 100 },
        ],
        totalPages: 1,
        totalCount: 1,
      }),
    });
  });
    });
  });

  page.on('requestfailed', (request) => {
    console.log('Request failed:', request.url(), request.failure()?.errorText);
  });
}

async function handleLoginRedirect(page: Page): Promise<void> {
  if (isCI) {
    return;
  }

  try {
    await page.waitForURL(url => {
      const u = new URL(url);
      return u.pathname === '/' || u.pathname.match(/^\/(admin|provider|customer)(\/|$)/) !== null;
    }, { timeout: 15000 });
  } catch (error) {
    if (error instanceof Error && error.name === 'TimeoutError') {
      const logoutButton = page.locator('button:has-text("Sair"), [data-testid="logout-button"]');
      const hasSession = await logoutButton.count() > 0 || (await page.context().cookies()).some(c => c.name.includes('auth'));
      if (hasSession) {
        console.log('OAuth redirect intercepted - running in mock/test mode');
        return;
      }
      throw new Error('Login failed: no session detected after redirect timeout');
    } else {
      throw error;
    }
  }
}

async function loginWithPath(page: Page, path: string, buttonName: RegExp): Promise<void> {
  if (isCI) {
    await setupAuthMocks(page);
    await page.goto(path);
    await page.goto('/');
    return;
  }

  await page.goto(path);
  await page.waitForLoadState('domcontentloaded');
  const loginButton = page.getByRole('button', { name: buttonName });
  await loginButton.waitFor({ state: 'visible', timeout: 10000 });
  await loginButton.click();
  await handleLoginRedirect(page);
}

export async function loginAsAdmin(page: Page): Promise<void> {
  if (isCI) {
    await setupAuthMocks(page);
    await page.goto('/dashboard');
    return;
  }
  await loginWithPath(page, '/admin/login', /entrar com keycloak/i);
}

export async function loginAsProvider(page: Page): Promise<void> {
  if (isCI) {
    await setupAuthMocks(page);
    await page.goto('/provider/dashboard');
    return;
  }
  await loginWithPath(page, '/provider/login', /entrar/i);
}

export async function loginAsCustomer(page: Page): Promise<void> {
  if (isCI) {
    await setupAuthMocks(page);
    await page.goto('/customer/dashboard');
    return;
  }
  await loginWithPath(page, '/login', /entrar/i);
}

export async function logout(page: Page): Promise<void> {
  if (isCI) {
    return;
  }

  const logoutButton = page.getByRole('button', { name: /sair/i });
  const buttonCount = await logoutButton.count();
  
  if (buttonCount === 0) {
    console.log('User already logged out or logout button not found');
    return;
  }
  
  await logoutButton.waitFor({ state: 'visible', timeout: 5000 });
  await logoutButton.click();
  
  try {
    await page.waitForURL(/\/(login|admin\/login|provider\/login)/, { timeout: 10000 });
  } catch (error) {
    throw new Error(`Logout failed: could not navigate to login page. Error: ${error instanceof Error ? error.message : 'Unknown error'}`);
  }
}
