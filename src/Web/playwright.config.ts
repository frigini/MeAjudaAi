import { defineConfig, devices } from '@playwright/test';

const baseUrl = process.env.BASE_URL || 'http://localhost:3000';
const adminUrl = process.env.ADMIN_BASE_URL || 'http://localhost:3002';
const providerUrl = process.env.PROVIDER_BASE_URL || 'http://localhost:3001';

export default defineConfig({
  testDir: '.',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
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
      name: 'ci',
      use: { ...devices['Desktop Chrome'], baseURL: adminUrl },
    },
  ],
  use: {
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    extraHTTPHeaders: {
      'x-mock-auth': 'true',
    },
  },
  webServer: process.env.BASE_URL ? undefined : {
    command: 'npm run dev:all',
    url: baseUrl,
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
