import { test, expect } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display admin login page', async ({ page }) => {
    await expect(page.getByRole('heading')).toContainText(/admin|administrador|meajudaai/i);
  });

  test('should navigate to login', async ({ page }) => {
    await page.click('text=Login');
    await expect(page).toHaveURL(/.*\/admin\/login/);
  });

  test('should display Keycloak OAuth login button', async ({ page }) => {
    await page.goto('/admin/login');
    await expect(page.getByRole('button', { name: /entrar com keycloak/i })).toBeVisible();
  });

  test('should trigger Keycloak OAuth flow when clicking login button', async ({ page }) => {
    await page.goto('/admin/login');
    
    await page.getByRole('button', { name: /entrar com keycloak/i }).click();
    
    await expect(page).toHaveURL(/.*keycloak.*|.*realms.*\/meajudaai/i);
  });
});
