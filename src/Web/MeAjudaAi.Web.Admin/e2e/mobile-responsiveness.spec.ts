import { test, expect, loginAsAdmin } from '@meajudaai/web-e2e-support';

const mobileViewport = { width: 375, height: 667 };

test.describe('Admin Portal - Mobile Responsiveness', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  test('should render correctly on mobile viewport', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/dashboard');
    
    const hamburgerButton = page.locator('[data-testid="mobile-menu-toggle"]');
    await hamburgerButton.click();
    
    await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible();
  });

  test('should collapse sidebar on mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/dashboard', { waitUntil: 'networkidle' });
    await page.waitForTimeout(500);
    
    const sidebar = page.locator('[data-testid="sidebar"]');
    await expect(sidebar).not.toBeVisible();
  });

  test('should display hamburger menu on mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/dashboard');
    
    await expect(page.locator('[data-testid="mobile-menu-toggle"]')).toBeVisible();
  });
});
