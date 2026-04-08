import { test, expect } from '@playwright/test';
import { setupPageForE2E } from '../support/auth';

/**
 * Provider Portal E2E Tests
 * Validates the provider dashboard and public routes.
 * USES: next-auth/middleware bypass via x-mock-auth.
 */

test.describe('Provider Portal - Dashboard (Authenticated)', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'provider');
    // The Provider Portal dashboard is at the root "/"
    await page.goto('/', { timeout: 15000 });
  });

  test('should load provider dashboard with professional info', async ({ page }) => {
    // Wait for the name from the mockProviderMe to appear
    await expect(page.getByText('Provider Test')).toBeVisible({ timeout: 10000 });
    
    // The UI shows "online!" when isActive is true in ProfileHeader
    await expect(page.getByText('online!')).toBeVisible();
    
    // The dashboard also renders services from the mock
    await expect(page.getByText('Limpeza Residencial')).toBeVisible();
    await expect(page.getByText('Reparo Elétrico')).toBeVisible();
  });
});

test.describe('Provider Portal - Public Routes', () => {
  test.beforeEach(async ({ page }) => {
    // We don't need auth bypass for public routes, but setupProviderMocks is still useful
    await setupPageForE2E(page, 'provider');
  });

  test('should display login page', async ({ page }) => {
    await page.goto('/login');
    // Check for the "MeAjudaAí" text in the login header
    await expect(page.getByText('Portal do Prestador')).toBeVisible();
    await expect(page.getByRole('button', { name: /Entrar com Keycloak/i })).toBeVisible();
  });
});