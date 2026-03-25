import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { AppProviders } from '@/components/providers/app-providers';

vi.mock('next-auth/react', () => ({
  SessionProvider: ({ children }: { children: React.ReactNode }) => children,
  useSession: () => ({ data: null, status: 'unauthenticated' }),
}));

vi.mock('next-auth', () => ({
  default: vi.fn(),
}));

describe('AppProviders', () => {
  it('deve renderizar children', async () => {
    render(
      <AppProviders>
        <div data-testid="child">Child Content</div>
      </AppProviders>
    );
    await waitFor(() => {
      expect(screen.getByTestId('child')).toHaveTextContent('Child Content');
    });
  });
});
