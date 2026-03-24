import { test, expect } from '../base';

test.describe('Admin Portal - Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display admin login page', async ({ page }) => {
    await expect(page.getByRole('heading')).toContainText(/admin|administrador/i);
  });

  test('should navigate to login', async ({ page }) => {
    await page.click('text=Login Admin');
    await expect(page).toHaveURL(/.*\/admin\/login/);
  });

  test('should display login form', async ({ page }) => {
    await page.goto('/admin/login');
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.goto('/admin/login');
    await page.fill('input[type="email"]', 'admin@meajudaai.com');
    await page.fill('input[type="password"]', 'wrongpassword');
    await page.click('button[type="submit"]');
    await expect(page.getByRole('alert')).toContainText(/credenciais inválidas/i);
  });
});
