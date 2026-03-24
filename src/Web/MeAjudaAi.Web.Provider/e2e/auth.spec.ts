import { test, expect, loginAsProvider, logout } from '@meajudaai/web-e2e-support';

test.describe('Provider Web App - Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display provider landing page', async ({ page }) => {
    await expect(page.locator('h1')).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.click('text=Login Prestador');
    await expect(page).toHaveURL(/.*\/provider\/login/);
  });

  test('should display login form with provider fields', async ({ page }) => {
    await page.goto('/provider/login');
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.goto('/provider/login');
    await page.fill('input[type="email"]', 'invalid@provider.com');
    await page.fill('input[type="password"]', 'wrongpassword');
    await page.click('button[type="submit"]');
    await expect(page.getByRole('alert')).toContainText(/credenciais inválidas/i);
  });
});
