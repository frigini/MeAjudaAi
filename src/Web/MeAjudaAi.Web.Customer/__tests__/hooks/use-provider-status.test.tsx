import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useProviderStatus } from '@/hooks/use-provider-status';
import React from 'react';

vi.mock('@/lib/api/client', () => ({
  client: {
    getConfig: vi.fn(() => ({ baseUrl: 'http://localhost:7002' })),
  },
}));

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(() => ({
    data: { accessToken: 'mock-token', user: { id: 'user-1' } },
    status: 'authenticated',
  })),
}));

const originalFetch = global.fetch;

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

describe('useProviderStatus Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    global.fetch = originalFetch;
  });

  it('deve iniciar com estado correto', async () => {
    global.fetch = vi.fn().mockImplementation(() => new Promise(() => {}));

    const { result } = renderHook(() => useProviderStatus(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
  });

  it('deve fazer requisição autenticada', async () => {
    const mockResponse = {
      ok: true,
      status: 200,
      json: async () => ({ data: { status: 1 } }),
    };
    global.fetch = vi.fn().mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useProviderStatus(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith(
        'http://localhost:7002/api/v1/providers/me/status',
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer mock-token',
          }),
        })
      );
    });
  });

  it('deve retornar null quando não há token', async () => {
    const { useSession } = await import('next-auth/react');
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: 'unauthenticated',
    } as any);

    const queryClient = new QueryClient();
    const { result } = renderHook(() => useProviderStatus(), {
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
