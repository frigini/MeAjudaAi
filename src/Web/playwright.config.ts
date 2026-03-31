import { defineConfig, devices } from '@playwright/test';

const baseUrl = process.env.BASE_URL || 'http://localhost:3000';
const adminUrl = process.env.ADMIN_BASE_URL || 'http://localhost:3002';
const providerUrl = process.env.PROVIDER_BASE_URL || 'http://localhost:3001';

export default defineConfig({
  testDir: './e2e',
  timeout: 60 * 1000,
  expect: {
    timeout: 10 * 1000,
  },
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,
  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report' }],
  ],
  use: {
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    baseURL: baseUrl,
    actionTimeout: 30 * 1000,
    navigationTimeout: 30 * 1000,
  },
  projects: [
    {
      name: 'admin',
      use: { ...devices['Desktop Chrome'], baseURL: adminUrl },
      testMatch: '**/admin/**/*.spec.ts',
    },
    {
      name: 'provider',
      use: { ...devices['Desktop Chrome'], baseURL: providerUrl },
      testMatch: '**/provider/**/*.spec.ts',
    },
    {
      name: 'customer',
      use: { ...devices['Desktop Chrome'], baseURL: baseUrl },
      testMatch: '**/customer/**/*.spec.ts',
    },
  ],
});