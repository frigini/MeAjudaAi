import { test, expect, loginAsAdmin } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Providers Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/providers');
  });

  test('should display providers table', async ({ page }) => {
    await expect(page.locator('[data-testid="providers-table"]')).toBeVisible();
  });

  test('should search providers', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="buscar"], input[name="search"]');
    await searchInput.fill('João');
    
    // Wait for search results
    const providerRows = page.locator('[data-testid="provider-row"]');
    await expect(providerRows.first()).toBeVisible();
    
    // Verify search results contain the search term
    const firstRowText = await providerRows.first().textContent();
    expect(firstRowText).toContain('João');
  });

  test('should filter by status', async ({ page }) => {
    await page.click('button:has-text("Filtrar")');
    await page.click('text=Ativos');
    
    // Verify filtered results are visible
    const providerRows = page.locator('[data-testid="provider-row"]');
    await expect(providerRows.first()).toBeVisible();
  });
});

test.describe('Admin Portal - Documents', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('should display documents pending review', async ({ page }) => {
    await page.goto('/admin/documentos');
    await expect(page.locator('[data-testid="documents-list"]')).toBeVisible();
  });

  test('should approve document', async ({ page }) => {
    await page.goto('/admin/documentos');
    
    // Get the first pending document's approve button
    const approveButton = page.locator('[data-testid="document-approve"]').first();
    await expect(approveButton).toBeVisible();
    await approveButton.click();
    
    await expect(page.getByRole('alert')).toContainText(/aprova/i);
  });

  test('should reject document', async ({ page }) => {
    await page.goto('/admin/documentos');
    
    // Get the first pending document's reject button
    const rejectButton = page.locator('[data-testid="document-reject"]').first();
    await expect(rejectButton).toBeVisible();
    await rejectButton.click();
    
    await expect(page.getByRole('alert')).toContainText(/rejeita/i);
  });
});
