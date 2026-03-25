import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./__tests__/setup.ts'],
    css: true,
    include: ['__tests__/**/*.test.{ts,tsx}'],
    exclude: ['node_modules/', '.next/', 'e2e/'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      reportsDirectory: './coverage',
      include: [
        'components/**/*.{ts,tsx}',
        'hooks/**/*.{ts,tsx}',
        'lib/**/*.{ts,tsx}',
      ],
      exclude: [
        'node_modules/',
        '__tests__/',
        'e2e/',
        '.next/',
        '**/*.d.ts',
        '**/*.config.*',
        'lib/api/generated/**',
        'app/**',
        'types/**',
      ],
      thresholds: {
        lines: 25,
        functions: 25,
        branches: 30,
        statements: 25,
      },
    },
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, './'),
      'test-support': resolve(__dirname, '../../libs/test-support/src'),
    },
  },
});
