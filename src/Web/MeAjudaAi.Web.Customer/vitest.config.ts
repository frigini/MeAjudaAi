import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./__tests__/setup.ts'],
    css: false,
    include: ['__tests__/**/*.test.{ts,tsx}'],
    exclude: ['node_modules/', '.next/', 'e2e/'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov', 'cobertura'],
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
      // Note: Coverage thresholds are managed at global level via merge-coverage.mjs
      // Current global target: 70%
    },
    alias: {
      '@': resolve(__dirname, './'),
      'test-support': resolve(__dirname, '../libs/test-support/src'),
    },
    server: {
      deps: {
        inline: [/lucide-react/, /@radix-ui/, /tailwind-variants/, /tailwind-merge/],
      },
    },
  },
});

