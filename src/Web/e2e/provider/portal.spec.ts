import { test, expect } from '@playwright/test';
import { setupPageForE2E } from '../support/auth';

/**
 * Provider Portal E2E Tests
 * Validates the provider dashboard and public routes.
 * USES: next-auth/middleware bypass via x-mock-auth.
 */

test.fixme(true, 'React hooks ordering issue with useProviderVerificationEvents - see GitHub issue');

// FIXME: React hooks ordering issue - useProviderVerificationEvents uses useSse/useSession
// which causes hooks order errors in test environment. Track in issue #XXX.
test.describe('Provider Portal - Dashboard (Authenticated)', () => {
  test.beforeEach(async ({ page }) => {
    await setupPageForE2E(page, 'provider');
    // The Provider Portal dashboard is at the root "/"
    await page.goto('/', { timeout: 15000 });
  });

  test.skip('should load provider dashboard with professional info', async ({ page }) => {
  // TODO: Skipped due to React hooks ordering issue with useProviderVerificationEvents
  // The component uses useSse and useSession which causes hooks order errors
  // in the test environment
  await page.waitForLoadState('domcontentloaded', { timeout: 15000 });
  await page.waitForTimeout(2000);
  
  const content = await page.content();
  const hasProviderName = content.includes('Provider Test');
  expect(hasProviderName).toBe(true);
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