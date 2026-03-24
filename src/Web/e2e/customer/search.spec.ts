import { test, expect } from '../base';

test.describe('Customer Web App - Search', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display search bar', async ({ page }) => {
    await expect(page.locator('input[placeholder*="buscar"], input[name="search"]')).toBeVisible();
  });

  test('should search for services', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="serviço"], input[name="search"]');
    await searchInput.fill('eletricista');
    await searchInput.press('Enter');
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible();
  });

  test('should filter by location', async ({ page }) => {
    await page.goto('/busca');
    const locationInput = page.locator('input[placeholder*="CEP"], input[placeholder*="endereço"]');
    await locationInput.fill(' Rio de Janeiro');
    await expect(page.locator('[data-testid="location-filter"]')).toBeVisible();
  });

  test('should display provider cards', async ({ page }) => {
    await page.goto('/busca?servico=eletricista');
    await expect(page.locator('[data-testid="provider-card"]').first()).toBeVisible();
  });
});
