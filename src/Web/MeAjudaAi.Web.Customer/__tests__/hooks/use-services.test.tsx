import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useServices } from '@/hooks/use-services';
import React from 'react';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiServicesGet: vi.fn(),
}));

import { apiServicesGet } from '@/lib/api/generated/sdk.gen';

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

describe('useServices Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar serviços com sucesso', async () => {
    const mockServices = [
      { id: '1', name: 'Elétrica' },
      { id: '2', name: 'Hidráulica' },
    ];

    (apiServicesGet as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      data: { data: mockServices },
    });

    const { result } = renderHook(() => useServices(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockServices);
  });

  it('deve retornar erro ao falhar', async () => {
    (apiServicesGet as ReturnType<typeof vi.fn>).mockRejectedValueOnce(new Error('API Error'));

    const { result } = renderHook(() => useServices(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeDefined();
  });

  it('deve estar em estado de loading inicialmente', () => {
    (apiServicesGet as ReturnType<typeof vi.fn>).mockImplementation(
      () => new Promise(() => {})
    );

    const { result } = renderHook(() => useServices(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
  });
});
