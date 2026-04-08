import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@/components/providers/app-providers';

vi.mock('next-auth/react', () => ({
  SessionProvider: ({ children }: { children: React.ReactNode }) => children,
  useSession: () => ({ data: null, status: 'unauthenticated' }),
}));

vi.mock('next-auth', () => ({
  default: vi.fn(),
}));

describe('AppProviders', () => {
  it('deve renderizar children', () => {
    render(
      <AppProviders>
        <div data-testid="child">Child Content</div>
      </AppProviders>
    );
    expect(screen.getByTestId('child')).toHaveTextContent('Child Content');
  });
});
