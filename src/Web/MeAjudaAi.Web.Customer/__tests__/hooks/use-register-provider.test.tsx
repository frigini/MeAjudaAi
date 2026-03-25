import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useRegisterProvider } from '@/hooks/use-register-provider';
import React from 'react';
import { EProviderType } from '@/types/api/provider';

vi.mock('@/lib/api/fetch-client', () => ({
  authenticatedFetch: vi.fn(() => Promise.resolve({ id: 'test' })),
}));

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(() => ({
    data: { accessToken: 'mock-token' },
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

describe('useRegisterProvider Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve iniciar com estado correto', () => {
    const { result } = renderHook(() => useRegisterProvider(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(false);
    expect(result.current.isError).toBe(false);
    expect(result.current.isSuccess).toBe(false);
  });
});
