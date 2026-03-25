import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useRegisterProvider } from '@/hooks/use-register-provider';
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

const mockProviderData = {
  id: 'provider-1',
  name: 'João Silva',
  userId: 'user-1',
  email: 'joao@teste.com',
  type: EProviderType.Individual,
  status: 1,
  verificationStatus: 1,
  tier: 1,
  averageRating: 0,
  reviewCount: 0,
  city: 'Rio de Janeiro',
  state: 'RJ',
  avatarUrl: null,
  services: [],
  businessProfile: null,
  documents: [],
  qualifications: [],
  createdAt: new Date().toISOString(),
};

describe('useRegisterProvider Hook', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('deve iniciar sem erro', () => {
    const { result } = renderHook(() => useRegisterProvider(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isIdle).toBe(true);
  });

  it('deve registrar provider com sucesso', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    vi.mocked(authenticatedFetch).mockResolvedValueOnce(mockProviderData);

    const { result } = renderHook(() => useRegisterProvider(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      name: 'João Silva',
      type: EProviderType.Individual,
      documentNumber: '11144477735',
      phoneNumber: '21999999999',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual(mockProviderData);
  });

  it('deve falhar quando não há token', async () => {
    const { useSession } = await import('next-auth/react');
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: 'unauthenticated',
    } as any);

    const queryClient = new QueryClient();
    const { result } = renderHook(() => useRegisterProvider(), {
      wrapper: ({ children }: { children: React.ReactNode }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      ),
    });

    result.current.mutate({
      name: 'João Silva',
      type: EProviderType.Individual,
      documentNumber: '11144477735',
      phoneNumber: '21999999999',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeDefined();
  });
});
