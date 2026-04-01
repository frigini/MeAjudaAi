import { Page } from '@playwright/test';
import { setupAdminMocks, setupProviderMocks, setupCustomerMocks } from './mocks';

export async function loginAsAdmin(page: Page) {
  await page.goto('/dashboard');
}

export async function loginAsProvider(page: Page) {
  await page.goto('/provider/dashboard');
}

export async function loginAsCustomer(page: Page) {
  await page.goto('/customer/dashboard');
}

export async function setupPageForE2E(page: Page, portal: 'admin' | 'provider' | 'customer') {
  // Set cookie and extra headers to bypass middleware
  const context = page.context();
  await context.addCookies([
    {
      name: 'x-mock-auth',
      value: 'true',
      domain: 'localhost',
      path: '/',
    }
  ]);
  await page.setExtraHTTPHeaders({
    'x-mock-auth': 'true'
  });

  await page.addInitScript(() => {
    window.localStorage.setItem('nextauth.message', '[]');
    window.sessionStorage.setItem('nextauth.token', 'mock-token');
  });
  
  switch (portal) {
    case 'admin':
      setupAdminMocks(page);
      break;
    case 'provider':
      setupProviderMocks(page);
      break;
    case 'customer':
      setupCustomerMocks(page);
      break;
  }
}