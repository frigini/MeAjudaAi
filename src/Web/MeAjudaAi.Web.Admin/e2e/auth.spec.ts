import { test, expect } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Authentication', () => {
  test('should display admin login page when accessing root', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/.*\/login/);
  });

  test('should display login page at /login', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('heading', { name: /login|entre|autentica/i })).toBeVisible({ timeout: 10000 });
  });

  test('should display Keycloak OAuth login button', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('button', { name: /entrar com keycloak/i })).toBeVisible({ timeout: 10000 });
  });
});
