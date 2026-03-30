import { defineConfig, devices } from '@playwright/test';

const baseUrl = process.env.BASE_URL || 'http://localhost:3000';
const adminUrl = process.env.ADMIN_BASE_URL || 'http://localhost:3002';
const providerUrl = process.env.PROVIDER_BASE_URL || 'http://localhost:3001';

const isCI = process.env.CI === 'true';

export default defineConfig({
  testDir: '.',
  timeout: isCI ? 60 * 1000 : 30 * 1000,
  expect: {
    timeout: isCI ? 15 * 1000 : 5000,
  },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: isCI ? 2 : 0,
  workers: isCI ? 1 : undefined,
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list'],
  ],
  grep: /e2e/,
  testMatch: '**/e2e/**/*.spec.ts',
  testIgnore: ['**/api-client/**/*.spec.ts'],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], baseURL: baseUrl },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'], baseURL: baseUrl },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'], baseURL: baseUrl },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'], baseURL: baseUrl },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'], baseURL: baseUrl },
    },
    {
      name: 'ci:admin',
      use: { ...devices['Desktop Chrome'], baseURL: adminUrl },
      testMatch: '**/MeAjudaAi.Web.Admin/e2e/**/*.spec.ts',
    },
    {
      name: 'ci:customer',
      use: { ...devices['Desktop Chrome'], baseURL: baseUrl },
      testMatch: '**/MeAjudaAi.Web.Customer/e2e/**/*.spec.ts',
    },
    {
      name: 'ci:provider',
      use: { ...devices['Desktop Chrome'], baseURL: providerUrl },
      testMatch: '**/MeAjudaAi.Web.Provider/e2e/**/*.spec.ts',
    },
  ],
  use: {
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    baseURL: adminUrl,
    actionTimeout: isCI ? 15 * 1000 : 0,
    navigationTimeout: isCI ? 30 * 1000 : 0,
    extraHTTPHeaders: {
      'x-mock-auth': 'true',
    },
  },
  webServer: isCI ? undefined : {
    command: 'npm run dev:all',
    url: baseUrl,
    reuseExistingServer: !isCI,
    timeout: 120 * 1000,
  },
});
