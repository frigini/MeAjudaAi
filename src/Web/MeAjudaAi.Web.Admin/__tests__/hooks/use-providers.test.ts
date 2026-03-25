import { describe, it, expect, beforeAll, afterEach, afterAll, vi } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { server } from '../mocks/server';

// Mock the generated API client module — hooks call these functions
vi.mock('@/lib/api/generated', () => ({
  apiProvidersGet2: vi.fn().mockResolvedValue({
    data: { items: [{ id: 'provider-1', name: 'Prestador Teste', verificationStatus: 1, type: 0 }], totalPages: 1 },
  }),
  apiProvidersGet3: vi.fn().mockResolvedValue({
    data: { id: 'provider-1', name: 'Prestador Teste' },
  }),
  apiProvidersGet: vi.fn().mockResolvedValue({ data: [] }),
  apiProvidersPost: vi.fn().mockResolvedValue({ data: {} }),
  apiProvidersPut: vi.fn().mockResolvedValue({ data: {} }),
  apiProvidersDelete: vi.fn().mockResolvedValue({}),
  apiActivatePost: vi.fn().mockResolvedValue({ data: {} }),
  apiDeactivatePost: vi.fn().mockResolvedValue({ data: {} }),
}));

import { useProviders, useProviderById, providerKeys } from '@/hooks/admin/use-providers';

beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('useProviders', () => {
  it('deve retornar lista de prestadores', async () => {
    const { result } = renderHook(() => useProviders());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
  });

  it('deve iniciar com isLoading true', () => {
    const { result } = renderHook(() => useProviders());
    expect(result.current.isLoading).toBe(true);
  });
});

describe('useProviderById', () => {
  it('deve estar desabilitado quando id está vazio', () => {
    const { result } = renderHook(() => useProviderById(''));
    expect(result.current.fetchStatus).toBe('idle');
  });

  it('deve buscar prestador por id', async () => {
    const { result } = renderHook(() => useProviderById('provider-1'));

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
  });
});

describe('providerKeys', () => {
  it('deve gerar query keys corretamente', () => {
    expect(providerKeys.all).toEqual(['providers']);
    expect(providerKeys.lists()).toEqual(['providers', 'list']);
    expect(providerKeys.detail('abc')).toEqual(['providers', 'detail', 'abc']);
  });
});
