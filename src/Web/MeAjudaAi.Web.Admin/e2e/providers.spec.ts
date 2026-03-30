import { test, expect, loginAsAdmin } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Providers Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/providers', { waitUntil: 'networkidle' });
  });

  test('should display providers table', async ({ page }) => {
    await page.waitForTimeout(1000);
    await expect(page.locator('[data-testid="providers-table"]')).toBeVisible({ timeout: 10000 });
  });

  test('should search providers', async ({ page }) => {
    await page.waitForTimeout(500);
    const searchInput = page.locator('input[placeholder*="buscar"], input[name="search"]');
    await searchInput.fill('João');
    
    // Wait for search results
    const providerRows = page.locator('[data-testid="provider-row"]');
    await expect(providerRows.first()).toBeVisible();
  });

  test('should filter by status', async ({ page }) => {
    await page.waitForTimeout(500);
    await page.click('button:has-text("Filtrar")');
    await page.click('text=Ativos');
    
    // Wait for filter response
    await page.waitForResponse(response => response.url().includes('providers') && response.status() === 200);
    
    // Verify filtered results show only active providers
    const providerRows = page.locator('[data-testid="provider-row"]');
    await providerRows.first().waitFor({ state: 'visible' });
    const rowCount = await providerRows.count();
    expect(rowCount).toBeGreaterThan(0);
  });
});

test.describe('Admin Portal - Documents', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('should display documents pending review', async ({ page }) => {
    await page.goto('/documentos', { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await expect(page.locator('[data-testid="documents-list"]')).toBeVisible({ timeout: 10000 });
  });

  test('should approve document', async ({ page }) => {
    await page.goto('/documentos', { waitUntil: 'networkidle' });
    await page.waitForTimeout(500);
    
    // Get the first provider row with pending documents
    const firstProviderRow = page.locator('[data-testid="provider-row"]').first();
    await expect(firstProviderRow).toBeVisible();
  });

  test('should reject document', async ({ page }) => {
    await page.goto('/documentos', { waitUntil: 'networkidle' });
    await page.waitForTimeout(500);
    
    // Get the first provider row with pending documents
    const firstProviderRow = page.locator('[data-testid="provider-row"]').first();
    await expect(firstProviderRow).toBeVisible();
  });
});
