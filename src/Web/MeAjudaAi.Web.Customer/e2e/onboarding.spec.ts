import { test, expect, loginAsCustomer, logout } from '@meajudaai/web-e2e-support';

test.describe('Customer Web App - Onboarding', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/cadastro');
  });

  test('should display registration form', async ({ page }) => {
    await expect(page.locator('input[name="name"]')).toBeVisible();
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('should complete customer registration', async ({ page }) => {
    await page.fill('input[name="name"]', 'Cliente Teste');
    await page.fill('input[type="email"]', `cliente${Date.now()}@teste.com`);
    await page.fill('input[type="password"]', 'Senha@123456');
    await page.fill('input[name="phone"]', '21999999999');
    await page.click('button:has-text("Cadastrar")');
    await expect(page).toHaveURL(/.*\/cadastro\/endereco|.*\/inicio/);
  });

  test('should validate required fields', async ({ page }) => {
    await page.click('button:has-text("Cadastrar")');
    await expect(page.locator('text=Campo obrigatório')).toBeVisible();
  });

  test('should validate email format', async ({ page }) => {
    await page.fill('input[name="name"]', 'Cliente Teste');
    await page.fill('input[type="email"]', 'email-invalido');
    await page.fill('input[type="password"]', 'Senha@123');
    await page.fill('input[name="phone"]', '21999999999');
    await page.click('button:has-text("Cadastrar")');
    await expect(page.locator('text=email inválido')).toBeVisible();
  });

  test('should validate password strength', async ({ page }) => {
    await page.fill('input[name="name"]', 'Cliente Teste');
    await page.fill('input[type="email"]', 'teste@teste.com');
    await page.fill('input[type="password"]', 'fraca');
    await page.fill('input[name="phone"]', '21999999999');
    await page.click('button:has-text("Cadastrar")');
    await expect(page.locator(/senha fraca|caracteres mínimos/i)).toBeVisible();
  });
});

test.describe('Customer Web App - Onboarding Address', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/cadastro/endereco');
  });

  test('should display address form', async ({ page }) => {
    await expect(page.locator('input[name="cep"]')).toBeVisible();
    await expect(page.locator('input[name="street"]')).toBeVisible();
    await expect(page.locator('input[name="number"]')).toBeVisible();
  });

  test('should complete address step', async ({ page }) => {
    await page.fill('input[name="cep"]', '20550160');
    await page.fill('input[name="street"]', 'Rua Teste');
    await page.fill('input[name="number"]', '123');
    await page.fill('input[name="neighborhood"]', 'Bairro Teste');
    await page.fill('input[name="city"]', 'Rio de Janeiro');
    await page.fill('input[name="state"]', 'RJ');
    await page.click('button:has-text("Próximo")');
    await expect(page).toHaveURL(/.*\/inicio/);
  });
});

test.describe('Customer Web App - Complete Onboarding Flow', () => {
  test('should complete full onboarding journey', async ({ page }) => {
    await page.goto('/cadastro');
    
    await page.fill('input[name="name"]', 'Cliente Completo');
    await page.fill('input[type="email"]', `comple${Date.now()}@teste.com`);
    await page.fill('input[type="password"]', 'Senha@123456');
    await page.fill('input[name="phone"]', '21999999999');
    await page.click('button:has-text("Cadastrar")');
    
    await page.waitForURL(/.*\/cadastro\/endereco|.*\/inicio/);
    
    if (await page.locator('input[name="cep"]').isVisible()) {
      await page.fill('input[name="cep"]', '20550160');
      await page.fill('input[name="street"]', 'Rua Teste');
      await page.fill('input[name="number"]', '123');
      await page.fill('input[name="neighborhood"]', 'Bairro Teste');
      await page.click('button:has-text("Próximo")');
    }
    
    await expect(page).toHaveURL(/.*\/inicio/);
  });
});
