import { test, expect } from '@playwright/test';
import { setupPageForE2E, loginAsAdmin } from '../support/auth';

test.describe('Admin Portal - Public Routes', () => {
  test('should display login page', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('heading', { name: /MeAjudaAí/i })).toBeVisible({ timeout: 10000 });
  });

  test('should display Keycloak login button', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('button', { name: /Entrar com Keycloak/i })).toBeVisible({ timeout: 10000 });
  });

  test('should redirect root to login', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/.*\/login/, { timeout: 10000 });
  });
});

test.describe('Admin Portal - Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.localStorage.setItem('nextauth.message', '[]');
      window.sessionStorage.setItem('nextauth.token', 'mock-jwt-token');
    });
    await page.goto('/dashboard', { timeout: 15000 });
  });

  test('should load dashboard page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Admin Portal - Providers', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.localStorage.setItem('nextauth.message', '[]');
      window.sessionStorage.setItem('nextauth.token', 'mock-jwt-token');
    });
    await page.goto('/providers', { timeout: 15000 });
  });

  test('should load providers page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Admin Portal - Allowed Cities', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.localStorage.setItem('nextauth.message', '[]');
      window.sessionStorage.setItem('nextauth.token', 'mock-jwt-token');
    });
    await page.goto('/allowed-cities', { timeout: 15000 });
  });

  test('should load cities page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Admin Portal - Categories', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.localStorage.setItem('nextauth.message', '[]');
      window.sessionStorage.setItem('nextauth.token', 'mock-jwt-token');
    });
    await page.goto('/categories', { timeout: 15000 });
  });

  test('should load categories page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Admin Portal - Services', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.localStorage.setItem('nextauth.message', '[]');
      window.sessionStorage.setItem('nextauth.token', 'mock-jwt-token');
    });
    await page.goto('/services', { timeout: 15000 });
  });

  test('should load services page', async ({ page }) => {
    await expect(page.locator('body')).toBeVisible({ timeout: 10000 });
  });
});