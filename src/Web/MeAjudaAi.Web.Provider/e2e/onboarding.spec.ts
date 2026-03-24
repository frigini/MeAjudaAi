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

test.describe('Provider Web App - Document Upload', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/provider/onboarding/documentos');
  });

  test('should display document upload section', async ({ page }) => {
    await expect(page.locator('[data-testid="document-upload"]')).toBeVisible();
    await expect(page.locator('input[type="file"]')).toBeVisible();
  });

  test('should upload document successfully', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]').first();
    
    await fileInput.setInputFiles({
      name: 'documento-teste.pdf',
      mimeType: 'application/pdf',
      buffer: Buffer.from('dummy pdf content')
    });
    
    await expect(page.locator('text=documento-teste.pdf')).toBeVisible();
    await expect(page.locator('text=Upload concluído')).toBeVisible();
  });

  test('should validate file type', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]').first();
    
    await fileInput.setInputFiles({
      name: 'imagem-teste.txt',
      mimeType: 'text/plain',
      buffer: Buffer.from('not an image')
    });
    
    await expect(page.locator(/tipo de arquivo inválido|formato não permitido/i)).toBeVisible();
  });

  test('should validate file size', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]').first();
    
    const largeBuffer = Buffer.alloc(11 * 1024 * 1024);
    await fileInput.setInputFiles({
      name: 'arquivo-grande.pdf',
      mimeType: 'application/pdf',
      buffer: largeBuffer
    });
    
    await expect(page.locator(/arquivo muito grande|tamanho máximo/i)).toBeVisible();
  });

  test('should proceed after document upload', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]').first();
    
    await fileInput.setInputFiles({
      name: 'documento-teste.pdf',
      mimeType: 'application/pdf',
      buffer: Buffer.from('dummy pdf content')
    });
    
    await page.waitForSelector('text=Upload concluído');
    await page.click('button:has-text("Próximo")');
    await expect(page).toHaveURL(/.*\/onboarding\/servicos/);
  });
});

test.describe('Provider Web App - Onboarding Services', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/provider/onboarding/servicos');
  });

  test('should display service selection', async ({ page }) => {
    await expect(page.locator('[data-testid="service-selection"]')).toBeVisible();
  });

  test('should select service categories', async ({ page }) => {
    const serviceCheckbox = page.locator('input[type="checkbox"]').first();
    await serviceCheckbox.check();
    await expect(serviceCheckbox).toBeChecked();
  });

  test('should complete onboarding flow', async ({ page }) => {
    await page.click('button:has-text("Próximo")');
    await expect(page).toHaveURL(/.*\/provider\/dashboard/);
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
