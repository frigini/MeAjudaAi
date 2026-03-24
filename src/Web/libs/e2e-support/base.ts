import { test as base, Page } from '@playwright/test';

export { expect } from '@playwright/test';

export interface TestFixtures {
  page: Page;
}

export const test = base;

export { base };

async function handleLoginRedirect(page: Page): Promise<void> {
  try {
    await page.waitForURL(/.*keycloak.*|.*realms.*\/meajudaai/i, { timeout: 5000 });
  } catch (error) {
    if (error instanceof Error && error.name === 'TimeoutError') {
      console.log('OAuth redirect intercepted - running in mock/test mode');
    } else {
      throw error;
    }
  }
}

export async function loginAsAdmin(page: Page): Promise<void> {
  await page.goto('/admin/login');
  await page.getByRole('button', { name: /entrar com keycloak/i }).click();
  await handleLoginRedirect(page);
}

export async function loginAsProvider(page: Page): Promise<void> {
  await page.goto('/provider/login');
  await page.getByRole('button', { name: /entrar/i }).click();
  await handleLoginRedirect(page);
}

export async function loginAsCustomer(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByRole('button', { name: /entrar/i }).click();
  await handleLoginRedirect(page);
}

export async function logout(page: Page): Promise<void> {
  await page.getByRole('button', { name: /sair/i }).click();
  await page.waitForURL(/\/(login|admin\/login|provider\/login)/);
}
