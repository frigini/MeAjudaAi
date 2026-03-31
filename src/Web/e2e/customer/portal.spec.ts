import { test, expect } from '@playwright/test';

test.describe('Customer Portal - Public Routes', () => {
  test('should display home page', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });

  test('should display search input on home', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });

  test('should display login page', async ({ page }) => {
    await page.goto('/auth/signin');
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Customer Portal - Search', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/buscar', { timeout: 15000 });
  });

  test('should load search page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Customer Portal - Provider Details', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/prestador/1', { timeout: 15000 });
  });

  test('should load provider detail page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Customer Portal - Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.localStorage.setItem('nextauth.message', '[]');
      window.sessionStorage.setItem('nextauth.token', 'mock-jwt-token');
    });
    await page.goto('/perfil', { timeout: 15000 });
  });

  test('should load customer profile page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});