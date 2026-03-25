import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useMyProviderProfile } from '@/hooks/use-my-provider-profile';
import React from 'react';

vi.mock('@/lib/api/fetch-client', () => ({
  authenticatedFetch: vi.fn(),
}));

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(() => ({
    data: { accessToken: 'mock-token', user: { id: 'user-1' } },
    status: 'authenticated',
  })),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useMyProviderProfile Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve iniciar com estado correto', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    vi.mocked(authenticatedFetch).mockImplementation(() => new Promise(() => {}));

    const { result } = renderHook(() => useMyProviderProfile(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
  });
});
