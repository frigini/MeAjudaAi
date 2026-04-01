/// <reference types="node" />
import { defineConfig, devices } from '@playwright/test';

const isServerRunning = (port: number) => {
  try {
    const net = require('net');
    const socket = new net.Socket();
    return new Promise((resolve) => {
      socket.once('error', () => {
        socket.destroy();
        resolve(false);
      });
      socket.once('connect', () => {
        socket.destroy();
        resolve(true);
      });
      socket.connect(port, 'localhost');
    });
  } catch {
    return Promise.resolve(false);
  }
};

const serversAlreadyRunning = async () => {
  const ports = [3000, 3001, 3002];
  const results = await Promise.all(ports.map(p => isServerRunning(p)));
  return results.every(r => r);
};

const baseUrl = process.env.BASE_URL || 'http://localhost:3000';
const adminUrl = process.env.ADMIN_BASE_URL || 'http://localhost:3002';
const providerUrl = process.env.PROVIDER_BASE_URL || 'http://localhost:3001';
const isCI = process.env.CI === 'true';
const testEnv = process.env.TEST_ENV;

export default defineConfig({
  testDir: './e2e',
  timeout: 120 * 1000,
  expect: {
    timeout: 30 * 1000,
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
    actionTimeout: 45 * 1000,
    navigationTimeout: 45 * 1000,
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
  webServer: testEnv === 'external' ? undefined : [
    {
      command: 'npm run dev:admin',
      port: 3002,
      cwd: './',
      reuseExistingServer: true,
      env: { 
        MOCK_AUTH: 'true', 
        CI: 'true',
        NEXT_PUBLIC_API_URL: 'http://localhost:3002'
      },
      timeout: 180 * 1000,
    },
    {
      command: 'npm run dev:provider',
      port: 3001,
      cwd: './',
      reuseExistingServer: true,
      env: { 
        MOCK_AUTH: 'true', 
        CI: 'true',
        NEXT_PUBLIC_API_URL: 'http://localhost:3001'
      },
      timeout: 180 * 1000,
    },
    {
      command: 'npm run dev:customer',
      port: 3000,
      cwd: './',
      reuseExistingServer: true,
      env: { 
        MOCK_AUTH: 'true', 
        CI: 'true',
        NEXT_PUBLIC_API_URL: 'http://localhost:3000'
      },
      timeout: 180 * 1000,
    },
  ],
});