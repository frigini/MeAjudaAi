import { test, expect } from '@meajudaai/web-e2e-support';

test.describe('Customer Web App - Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display landing page', async ({ page }) => {
    await expect(page.getByText(/Conectando quem precisa com/i)).toBeVisible();
    await expect(page.getByText(/quem sabe fazer/i)).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.click('text=Entrar');
    await expect(page).toHaveURL(/.*\/login/);
  });

  test('should display login form', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.goto('/login');
    await page.fill('input[type="email"]', 'invalid@test.com');
    await page.fill('input[type="password"]', 'wrongpassword');
    await page.click('button[type="submit"]');
    await expect(page.getByRole('alert')).toContainText(/credenciais inválidas/i);
  });
});
