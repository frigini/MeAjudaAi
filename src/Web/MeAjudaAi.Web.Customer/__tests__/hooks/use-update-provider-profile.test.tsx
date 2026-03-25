import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useUpdateProviderProfile } from '@/hooks/use-update-provider-profile';
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
      mutations: { retry: false },
    },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useUpdateProviderProfile Hook', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('deve iniciar corretamente', () => {
    const { result } = renderHook(() => useUpdateProviderProfile(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(false);
  });
});
