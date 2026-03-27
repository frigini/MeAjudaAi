import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@/components/providers/app-providers';
import { vi } from 'vitest';

vi.mock('next-auth/react', () => ({
  SessionProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

vi.mock('@tanstack/react-query-devtools', () => ({
  ReactQueryDevtools: () => null,
}));

describe('AppProviders', () => {
  it('should render children', () => {
    render(<AppProviders>Test</AppProviders>);
    expect(screen.getByText('Test')).toBeInTheDocument();
  });
});
