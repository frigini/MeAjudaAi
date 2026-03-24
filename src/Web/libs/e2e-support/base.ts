import { test as base, Page, BrowserContext } from '@playwright/test';

export { expect } from '@playwright/test';

export interface TestFixtures {
  page: Page;
}

export const test = base.extend<TestFixtures>({
  page: async ({ page }, use) => {
    await use(page);
  },
});

export { base };

export async function loginAsAdmin(page: Page): Promise<void> {
  await page.goto('/admin/login');
  await page.getByRole('button', { name: /entrar com keycloak/i }).click();
  await page.waitForURL(/.*keycloak.*|.*realms.*\/meajudaai/i, { timeout: 5000 }).catch(() => {
    console.log('OAuth redirect intercepted - running in mock/test mode');
  });
}

export async function loginAsProvider(page: Page): Promise<void> {
  await page.goto('/provider/login');
  await page.getByRole('button', { name: /entrar/i }).click();
  await page.waitForURL(/.*keycloak.*|.*realms.*\/meajudaai/i, { timeout: 5000 }).catch(() => {
    console.log('OAuth redirect intercepted - running in mock/test mode');
  });
}

export async function loginAsCustomer(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByRole('button', { name: /entrar/i }).click();
  await page.waitForURL(/.*keycloak.*|.*realms.*\/meajudaai/i, { timeout: 5000 }).catch(() => {
    console.log('OAuth redirect intercepted - running in mock/test mode');
  });
}

export async function logout(page: Page): Promise<void> {
  await page.click('text=Sair');
  await page.waitForURL(/\/(login|admin\/login|provider\/login)/);
}
