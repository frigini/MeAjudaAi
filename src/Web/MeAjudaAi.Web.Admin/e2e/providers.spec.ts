import { test, expect, loginAsAdmin } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Providers Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/providers');
  });

  test.skip('should display providers table', async ({ page }) => {
    await expect(page.locator('[data-testid="providers-table"]')).toBeVisible();
  });

  test.skip('should search providers', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="buscar"], input[name="search"]');
    await searchInput.fill('João');
    
    // Wait for search results
    const providerRows = page.locator('[data-testid="provider-row"]');
    await expect(providerRows.first()).toBeVisible();
    
    // Verify search results contain the search term
    const firstRowText = await providerRows.first().textContent();
    expect(firstRowText).toContain('João');
  });

  test.skip('should filter by status', async ({ page }) => {
    await page.click('button:has-text("Filtrar")');
    await page.click('text=Ativos');
    
    // Wait for filter response
    await page.waitForResponse(response => response.url().includes('providers') && response.status() === 200);
    
    // Verify filtered results show only active providers
    const providerRows = page.locator('[data-testid="provider-row"]');
    await providerRows.first().waitFor({ state: 'visible' });
    const rowCount = await providerRows.count();
    expect(rowCount).toBeGreaterThan(0);
    
    // Verify each visible row has active status
    for (let i = 0; i < rowCount; i++) {
      const row = providerRows.nth(i);
      const statusCell = row.locator('[data-testid="provider-status"]');
      if (await statusCell.isVisible()) {
        const statusText = await statusCell.textContent();
        expect(statusText).toMatch(/ativo|active/i);
      }
    }
  });
});

test.describe('Admin Portal - Documents', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test.skip('should display documents pending review', async ({ page }) => {
    await page.goto('/documentos');
    await expect(page.locator('[data-testid="documents-list"]')).toBeVisible();
  });

  test.skip('should approve document', async ({ page }) => {
    await page.goto('/documentos');
    
    // Get the first provider row with pending documents
    const firstProviderRow = page.locator('[data-testid="provider-row"]').first();
    await expect(firstProviderRow).toBeVisible();
    
    // Click the eye icon to open provider detail view
    const detailButton = firstProviderRow.locator('button[aria-label*="visualizar"], button[aria-label*="view"], [data-testid="view-details"]').first();
    await detailButton.click();
    
    // Wait for detail view to load
    await expect(page.locator('[data-testid="provider-detail"]')).toBeVisible();
    
    // Click the approve button in the detail view
    const approveButton = page.locator('button:has-text("Aprovar"), [data-testid="approve-button"]');
    await expect(approveButton).toBeVisible();
    await approveButton.click();
    
    // Verify success alert
    await expect(page.getByRole('status')).toContainText(/aprova/i);
    
    // Return to the listing
    await page.click('button:has-text("Voltar")');
    await expect(page).toHaveURL(/.*\/documentos/);
    
    // Verify the provider no longer appears in pending list
    const providerRows = page.locator('[data-testid="provider-row"]');
    const firstRowText = await providerRows.first().textContent();
    expect(firstRowText).not.toContain('Pendente');
  });

  test.skip('should reject document', async ({ page }) => {
    await page.goto('/documentos');
    
    // Get the first provider row with pending documents
    const firstProviderRow = page.locator('[data-testid="provider-row"]').first();
    await expect(firstProviderRow).toBeVisible();
    
    // Click the eye icon to open provider detail view
    const detailButton = firstProviderRow.locator('button[aria-label*="visualizar"], button[aria-label*="view"], [data-testid="view-details"]').first();
    await detailButton.click();
    
    // Wait for detail view to load
    await expect(page.locator('[data-testid="provider-detail"]')).toBeVisible();
    
    // Click the reject button in the detail view
    const rejectButton = page.locator('button:has-text("Rejeitar"), [data-testid="reject-button"]');
    await expect(rejectButton).toBeVisible();
    await rejectButton.click();
    
    // Verify success alert
    await expect(page.getByRole('status')).toContainText(/rejeita/i);
  });
});
