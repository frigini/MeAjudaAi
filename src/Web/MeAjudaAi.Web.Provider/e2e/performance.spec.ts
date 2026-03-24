import { test, expect } from '@playwright/test';

const mobileViewport = { width: 375, height: 667 };

test.describe('Provider Web App - Mobile Responsiveness', () => {
  test('should render correctly on mobile viewport', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/provider/dashboard');
    
    await expect(page.locator('nav')).toBeVisible();
  });

  test('should have touch-friendly elements on mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/provider/dashboard');
    
    const actionButtons = page.locator('button');
    const count = await actionButtons.count();
    
    for (let i = 0; i < Math.min(count, 5); i++) {
      const button = actionButtons.nth(i);
      const box = await button.boundingBox();
      if (box) {
        expect(box.height).toBeGreaterThanOrEqual(44);
      }
    }
  });
});
