import { test, expect } from '../base';

test.describe('Admin Portal - Providers Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/providers');
  });

  test('should display providers table', async ({ page }) => {
    await expect(page.locator('[data-testid="providers-table"]')).toBeVisible();
  });

  test('should search providers', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="buscar"], input[name="search"]');
    await searchInput.fill('João');
    await expect(page.locator('[data-testid="provider-row"]')).toBeVisible();
  });

  test('should filter by status', async ({ page }) => {
    await page.click('button:has-text("Filtrar")');
    await page.click('text=Ativos');
    await expect(page.locator('[data-testid="provider-row"]')).toBeVisible();
  });
});

test.describe('Admin Portal - Documents', () => {
  test('should display documents pending review', async ({ page }) => {
    await page.goto('/admin/documentos');
    await expect(page.locator('[data-testid="documents-list"]')).toBeVisible();
  });

  test('should approve document', async ({ page }) => {
    await page.goto('/admin/documentos');
    await page.click('button:has-text("Aprovar")');
    await expect(page.locator('text=Documento aprovado')).toBeVisible();
  });

  test('should reject document', async ({ page }) => {
    await page.goto('/admin/documentos');
    await page.click('button:has-text("Rejeitar")');
    await expect(page.locator('text=Documento rejeitado')).toBeVisible();
  });
});
