import { test, expect, loginAsProvider } from '@meajudaai/web-e2e-support';

test.describe('Provider Web App - Onboarding', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/provider/onboarding');
  });

  test('should display onboarding steps', async ({ page }) => {
    await expect(page.locator('[data-testid="onboarding-step"]')).toBeVisible();
  });

  test('should complete basic info step', async ({ page }) => {
    await page.fill('input[name="name"]', 'João Silva');
    await page.fill('input[name="phone"]', '21999999999');
    await page.fill('input[name="cpf"]', '52998224725');
    await page.click('button:has-text("Próximo")');
    await expect(page).toHaveURL(/.*\/onboarding\/documentos/);
  });

  test('should validate required fields', async ({ page }) => {
    await page.click('button:has-text("Próximo")');
    await expect(page.locator('text=Campo obrigatório')).toBeVisible();
  });
});

test.describe('Provider Web App - Dashboard', () => {
  test('should display dashboard metrics', async ({ page }) => {
    await page.goto('/provider/dashboard');
    await expect(page.locator('[data-testid="dashboard-metrics"]')).toBeVisible();
  });

  test('should display recent bookings', async ({ page }) => {
    await page.goto('/provider/dashboard');
    await expect(page.locator('[data-testid="recent-bookings"]')).toBeVisible();
  });
});
