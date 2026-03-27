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
    await page.waitForURL(/\/(admin|provider)?(\/|$)/, { timeout: 15000 });
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
  await page.goto(path);
  await page.waitForLoadState('domcontentloaded');
  const loginButton = page.getByRole('button', { name: buttonName });
  await loginButton.waitFor({ state: 'visible', timeout: 10000 });
  await loginButton.click();
  await handleLoginRedirect(page);
}

export async function loginAsAdmin(page: Page): Promise<void> {
  await loginWithPath(page, '/admin/login', /entrar com keycloak/i);
}

export async function loginAsProvider(page: Page): Promise<void> {
  await loginWithPath(page, '/provider/login', /entrar/i);
}

export async function loginAsCustomer(page: Page): Promise<void> {
  await loginWithPath(page, '/login', /entrar/i);
}

export async function logout(page: Page): Promise<void> {
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
