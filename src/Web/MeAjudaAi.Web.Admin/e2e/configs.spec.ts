import { test, expect, loginAsAdmin } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Allowed Cities Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/allowed-cities', { waitUntil: 'networkidle' });
  });

  test('should display allowed cities list', async ({ page }) => {
    await page.waitForTimeout(1000);
    await expect(page.locator('[data-testid="cities-list"]')).toBeVisible({ timeout: 10000 });
  });

  test('should add new allowed city', async ({ page }) => {
    await page.waitForTimeout(500);
    await page.click('button:has-text("Adicionar Cidade")');
    await expect(page.locator('[data-testid="city-form"]')).toBeVisible();
  });

  test('should edit existing city', async ({ page }) => {
    await page.waitForTimeout(500);
    const cityRow = page.locator('[data-testid="city-row"]').first();
    await cityRow.locator('button[aria-label="Editar"]').click();
    
    await page.fill('input[name="serviceRadiusKm"]', '150');
    await page.click('button:has-text("Salvar")');
    
    await expect(page.locator('text=Cidade atualizada com sucesso')).toBeVisible();
  });

  test('should remove allowed city', async ({ page }) => {
    await page.waitForTimeout(500);
    const cityRow = page.locator('[data-testid="city-row"]').first();
    await cityRow.locator('button[aria-label="Remover"]').click();
    
    await page.click('button:has-text("Confirmar")');
    
    await expect(page.locator('text=Cidade removida com sucesso')).toBeVisible();
  });

  test('should search cities', async ({ page }) => {
    await page.waitForTimeout(500);
    const searchInput = page.locator('input[name="search"]');
    await searchInput.fill('Rio');
    
    await expect(page.locator('[data-testid="city-row"]')).toBeVisible();
  });

  test('should filter by state', async ({ page }) => {
    await page.waitForTimeout(500);
    await page.click('button:has-text("Filtrar por Estado")');
    await page.click('button:has-text("RJ")');
    
    await expect(page.locator('[data-testid="city-row"]')).toBeVisible();
  });
});

test.describe('Admin Portal - Service Catalog Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/config/servicos', { waitUntil: 'networkidle' });
  });

  test('should display service catalog', async ({ page }) => {
    await page.waitForTimeout(1000);
    await expect(page.locator('[data-testid="services-list"]')).toBeVisible({ timeout: 10000 });
  });

  test('should add new service category', async ({ page }) => {
    await page.waitForTimeout(500);
    await page.click('button:has-text("Adicionar Serviço")');
    await expect(page.locator('[data-testid="service-form"]')).toBeVisible();
  });

  test('should edit service category', async ({ page }) => {
    await page.waitForTimeout(500);
    const serviceRow = page.locator('[data-testid="service-row"]').first();
    await serviceRow.locator('button[aria-label="Editar"]').click();
    
    await page.fill('input[name="name"]', 'Eletricista Atualizado');
    await page.click('button:has-text("Salvar")');
    
    await expect(page.locator('text=Serviço atualizado com sucesso')).toBeVisible();
  });

  test('should toggle service visibility', async ({ page }) => {
    await page.waitForTimeout(500);
    const serviceRow = page.locator('[data-testid="service-row"]').first();
    const toggle = serviceRow.locator('input[type="checkbox"]');
    
    const isChecked = await toggle.isChecked();
    await toggle.click();
    
    if (isChecked) {
      await expect(page.locator('text=Serviço desabilitado')).toBeVisible();
    } else {
      await expect(page.locator('text=Serviço habilitado')).toBeVisible();
    }
  });

  test('should delete service category', async ({ page }) => {
    await page.waitForTimeout(500);
    const serviceRow = page.locator('[data-testid="service-row"]').first();
    await serviceRow.locator('button[aria-label="Excluir"]').click();
    
    await page.click('button:has-text("Confirmar")');
    
    await expect(page.locator('text=Serviço excluído com sucesso')).toBeVisible();
  });

  test('should search services', async ({ page }) => {
    await page.waitForTimeout(500);
    const searchInput = page.locator('input[name="search"]');
    await searchInput.fill('Eletricista');
    
    await expect(page.locator('[data-testid="service-row"]')).toBeVisible();
  });

  test('should filter services by status', async ({ page }) => {
    await page.waitForTimeout(500);
    await page.click('button:has-text("Filtrar")');
    await page.click('button:has-text("Ativos")');
    
    await expect(page.locator('[data-testid="service-row"]')).toBeVisible();
  });
});

test.describe('Admin Portal - Profile Settings', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/configuracoes', { waitUntil: 'networkidle' });
  });

  test('should display profile settings', async ({ page }) => {
    await page.waitForTimeout(1000);
    await expect(page.locator('text=Perfil')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('input[name="adminName"]')).toBeVisible();
    await expect(page.locator('input[name="adminEmail"]')).toBeVisible();
  });

  test('should update profile settings', async ({ page }) => {
    await page.waitForTimeout(500);
    await page.fill('input[name="adminName"]', 'Admin Atualizado');
    await page.fill('input[name="adminEmail"]', 'admin.atualizado@meajudaai.com');
    await page.click('button:has-text("Salvar")');
    
    await expect(page.locator('text=Configurações salvas com sucesso')).toBeVisible();
  });
});
