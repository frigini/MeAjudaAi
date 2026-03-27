import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useViaCep } from '@/hooks/use-via-cep';

const mockFetch = vi.fn();

describe('useViaCep Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('fetch', mockFetch);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('deve retornar isLoading false inicialmente', () => {
    const { result } = renderHook(() => useViaCep());
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('deve retornar null para CEP inválido', async () => {
    const { result } = renderHook(() => useViaCep());
    
    const invalidCeps = ['123', '12', '', '12345'];
    for (const cep of invalidCeps) {
      const address = await result.current.fetchAddress(cep);
      expect(address).toBeNull();
    }
    expect(result.current.isLoading).toBe(false);
  });

  it('deve buscar endereço para CEP válido', async () => {
    const mockCepData = {
      cep: '20550160',
      logradouro: 'Rua Teste',
      complemento: '',
      bairro: 'Bairro Teste',
      localidade: 'Rio de Janeiro',
      uf: 'RJ',
    };

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockCepData,
    });

    const { result } = renderHook(() => useViaCep());
    
    const address = await result.current.fetchAddress('20550160');
    
    expect(address).toEqual(mockCepData);
    expect(result.current.isLoading).toBe(false);
  });

  it('deve retornar erro para CEP não encontrado', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ erro: true }),
    });

    const { result } = renderHook(() => useViaCep());
    
    const address = await result.current.fetchAddress('00000000');
    
    expect(address).toBeNull();
    await waitFor(() => {
      expect(result.current.error).toBe('CEP não encontrado');
    });
  });

  it('deve lidar com erro de rede', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useViaCep());
    
    const address = await result.current.fetchAddress('20550160');
    
    expect(address).toBeNull();
    await waitFor(() => {
      expect(result.current.error).toBe('Falha ao consultar CEP');
    });
  });

  it('deve cancelar requisição anterior', async () => {
    const mockCepData2 = { cep: '20550161', logradouro: 'Rua Teste 2', complemento: '', bairro: 'Bairro Teste', localidade: 'Rio de Janeiro', uf: 'RJ' };
    
    mockFetch
      .mockImplementationOnce((_url, options) => {
        const signal = options?.signal;
        return new Promise((_, reject) => {
            if (signal?.aborted) {
                reject(new DOMException('Aborted', 'AbortError'));
                return;
            }
            signal?.addEventListener('abort', () => {
                reject(new DOMException('Aborted', 'AbortError'));
            });
        });
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => mockCepData2,
      });

    const { result } = renderHook(() => useViaCep());
    
    const promise1 = result.current.fetchAddress('20550160');
    // Calling a second time should immediately abort the first
    const promise2 = result.current.fetchAddress('20550161');
    
    const address1 = await promise1;
    const address2 = await promise2;
    
    expect(address1).toBeNull();
    expect(address2).toEqual(mockCepData2);
    
    expect(mockFetch).toHaveBeenCalledTimes(2);
  });
});
