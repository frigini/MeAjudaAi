import { test, expect } from '@playwright/test';
import { setupPageForE2E } from '../support/auth';

test.describe('Admin Portal - Public Routes', () => {
  test('should display login page', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('heading', { name: /MeAjudaAí/i })).toBeVisible({ timeout: 10000 });
  });

  test('should display Keycloak login button', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('button', { name: /Entrar com Keycloak/i })).toBeVisible();
  });

  test('should redirect root to login when unauthenticated', async ({ page }) => {
    // We don't call setupPageForE2E here, so middleware should redirect
    await page.goto('/');
    await expect(page).toHaveURL(/.*\/login/);
  });
});

test.describe('Admin Portal - Dashboard (Authenticated)', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'admin');
    await page.goto('/dashboard', { timeout: 15000 });
  });

  test('should load dashboard page with admin info', async ({ page }) => {
    await expect(page.locator('p.text-sm.font-medium', { hasText: 'Admin' })).toBeVisible();
  });
});

test.describe('Admin Portal - Management Pages', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'admin');
  });

  test('should load providers page and show list', async ({ page }) => {
    await page.goto('/providers', { timeout: 15000 });
    await expect(page.getByText('João Silva')).toBeVisible();
    await expect(page.getByText('Maria Santos')).toBeVisible();
  });

  test('should load allowed cities page and show list', async ({ page }) => {
    await page.goto('/allowed-cities', { timeout: 15000 });
    await expect(page.getByText('São Paulo')).toBeVisible();
    await expect(page.getByText('Rio de Janeiro')).toBeVisible();
  });

  test('should load categories page and show list', async ({ page }) => {
    await page.goto('/categories', { timeout: 15000 });
    await expect(page.getByText('Serviços Domésticos')).toBeVisible();
    await expect(page.getByText('Manutenção e Reparos')).toBeVisible();
  });

  test('should load services page and show list', async ({ page }) => {
    await page.goto('/services', { timeout: 15000 });
    await expect(page.getByText('Limpeza Residencial')).toBeVisible();
    await expect(page.getByText('Reparo Elétrico')).toBeVisible();
  });
});