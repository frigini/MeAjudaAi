import { test, expect } from '@playwright/test';
import { setupPageForE2E } from '../support/auth';

/**
 * Customer Portal E2E Tests
 * Validates the customer home, search, and profile pages.
 */

test.describe('Customer Portal - Public Routes', () => {
  test('should display home page with main heading', async ({ page }) => {
    await page.goto('/');
    // Heading: "Conectando quem precisa com quem sabe fazer."
    await expect(page.getByText('Conectando quem precisa com')).toBeVisible({ timeout: 10000 });
    await expect(page.getByText('quem sabe fazer.')).toBeVisible();
  });

  test('should display city search on home', async ({ page }) => {
    await page.goto('/');
    // CitySearch uses a select with "Selecionar cidade..." option
    await expect(page.getByLabel('Selecionar cidade')).toBeVisible();
  });

  test('should display login page', async ({ page }) => {
    await page.goto('/auth/signin');
    // Heading in (auth)/auth/signin/page.tsx is likely "Entrar" or similar
    await expect(page.getByRole('heading', { name: /Entrar/i })).toBeVisible();
  });
});

test.describe('Customer Portal - Search', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'customer');
    await page.goto('/buscar', { timeout: 15000 });
  });

  test('should load search page and display results', async ({ page }) => {
    // The mock data setupCustomerMocks uses mockCategories which includes "Serviços Domésticos"
    // And setupProviderMocks (shared) has "João Silva"
    await expect(page.getByText('João Silva')).toBeVisible({ timeout: 10000 });
    await expect(page.getByText('Muriaé - MG').first()).toBeVisible();
  });

  test('should filter by query', async ({ page }) => {
    // Search input placeholder on /buscar is "Buscar serviço"
    const searchInput = page.getByPlaceholder(/Buscar serviço/i);
    await searchInput.fill('Limpeza');
    await searchInput.press('Enter');
    await expect(page).toHaveURL(/.*q=Limpeza/);
  });
});

test.describe('Customer Portal - Provider Details', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'customer');
    // Using a valid UUID pattern for the [id] route
    await page.goto('/prestador/550e8400-e29b-41d4-a716-446655440001', { timeout: 15000 });
  });

  test('should load provider detail page with services', async ({ page }) => {
    // Both heading and fantasyName are "João Silva" in our mock
    await expect(page.getByRole('heading', { name: 'João Silva' })).toBeVisible({ timeout: 10000 });
    await expect(page.getByText('Muriaé - MG').first()).toBeVisible();
  });
});

test.describe('Customer Portal - Dashboard (Authenticated)', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'customer');
    await page.goto('/perfil', { timeout: 15000 });
  });

  test('should load customer profile page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Meu Perfil' })).toBeVisible({ timeout: 10000 });
    await expect(page.getByText('Customer Test')).toBeVisible();
    await expect(page.getByText('customer@test.com')).toBeVisible();
    await expect(page.getByRole('button', { name: /Customer/i })).toBeVisible();
  });
});