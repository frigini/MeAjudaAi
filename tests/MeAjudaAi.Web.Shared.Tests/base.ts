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
  await page.fill('input[type="email"]', 'admin@meajudaai.com');
  await page.fill('input[type="password"]', 'adminpassword');
  await page.click('button[type="submit"]');
}

export async function loginAsProvider(page: Page): Promise<void> {
  await page.goto('/provider/login');
  await page.fill('input[type="email"]', 'provider@test.com');
  await page.fill('input[type="password"]', 'providerpassword');
  await page.click('button[type="submit"]');
}

export async function loginAsCustomer(page: Page): Promise<void> {
  await page.goto('/login');
  await page.fill('input[type="email"]', 'customer@test.com');
  await page.fill('input[type="password"]', 'customerpassword');
  await page.click('button[type="submit"]');
}

export async function logout(page: Page): Promise<void> {
  await page.click('text=Sair');
  await page.waitForURL(/\/(login|admin\/login|provider\/login)/);
}
