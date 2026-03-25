import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useMyProviderProfile } from '@/hooks/use-my-provider-profile';
import React from 'react';
import { EProviderType, EProviderStatus, EVerificationStatus, EProviderTier } from '@/types/api/provider';

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

const mockProviderData = {
  id: 'provider-1',
  name: 'João Silva',
  userId: 'user-1',
  email: 'joao@teste.com',
  type: EProviderType.Individual,
  status: EProviderStatus.Active,
  verificationStatus: EVerificationStatus.Verified,
  tier: EProviderTier.Standard,
  averageRating: 4.5,
  reviewCount: 10,
  city: 'Rio de Janeiro',
  state: 'RJ',
  avatarUrl: null,
  services: [],
  businessProfile: null,
  documents: [],
  qualifications: [],
  createdAt: new Date().toISOString(),
};

describe('useMyProviderProfile Hook', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('deve iniciar com estado correto', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    vi.mocked(authenticatedFetch).mockImplementation(() => new Promise(() => {}));

    const { result } = renderHook(() => useMyProviderProfile(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
  });

  it('deve buscar dados do perfil com sucesso', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    vi.mocked(authenticatedFetch).mockResolvedValueOnce(mockProviderData);

    const { result } = renderHook(() => useMyProviderProfile(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual(mockProviderData);
  });

  it('deve retornar null quando não há token', async () => {
    const { useSession } = await import('next-auth/react');
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: 'unauthenticated',
    } as any);

    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    });
    const { result } = renderHook(() => useMyProviderProfile(), {
      wrapper: ({ children }: { children: React.ReactNode }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      ),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toBeFalsy();
  });
});
