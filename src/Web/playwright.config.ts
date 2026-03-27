import { defineConfig, devices } from '@playwright/test';

const baseUrl = process.env.BASE_URL || 'http://localhost:3000';

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
  use: {
    baseURL: baseUrl,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  grep: /e2e/,
  testMatch: '**/e2e/**/*.spec.ts',
  testIgnore: ['**/api-client/**/*.spec.ts'],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },
    {
      name: 'ci',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: process.env.BASE_URL ? undefined : {
    command: 'npm run dev:all',
    url: baseUrl,
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
