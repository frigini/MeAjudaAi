import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useUpdateProviderProfile } from '@/hooks/use-update-provider-profile';
import React from 'react';
import { EProviderType } from '@/types/provider';

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

  it('deve atualizar perfil com sucesso', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    vi.mocked(authenticatedFetch).mockResolvedValueOnce({ success: true });

    const { result } = renderHook(() => useUpdateProviderProfile(), {
      wrapper: createWrapper(),
    });

    const payload = {
      name: 'João Silva',
      phoneNumber: '21999999999',
      city: 'Rio de Janeiro',
      state: 'RJ',
    };

    await act(async () => {
      await result.current.mutateAsync(payload);
    });

    expect(authenticatedFetch).toHaveBeenCalled();
    expect(result.current.isPending).toBe(false);
  });

  it('deve tratar erro na atualização', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    vi.mocked(authenticatedFetch).mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useUpdateProviderProfile(), {
      wrapper: createWrapper(),
    });

    const payload = {
      name: 'João Silva',
      phoneNumber: '21999999999',
      city: 'Rio de Janeiro',
      state: 'RJ',
    };

    await act(async () => {
      await expect(result.current.mutateAsync(payload)).rejects.toThrow('Network error');
    });

    expect(result.current.isError).toBe(true);
    expect(result.current.isPending).toBe(false);
  });
});
