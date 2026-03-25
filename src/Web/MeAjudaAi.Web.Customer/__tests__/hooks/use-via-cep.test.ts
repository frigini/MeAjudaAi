import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useViaCep } from '@/hooks/use-via-cep';

const mockFetch = vi.fn();
global.fetch = mockFetch;

describe('useViaCep Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('deve retornar isLoading false inicialmente', () => {
    const { result } = renderHook(() => useViaCep());
    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('deve retornar null para CEP inválido', async () => {
    const { result } = renderHook(() => useViaCep());
    
    const address = await result.current.fetchAddress('123');
    
    expect(address).toBeNull();
    expect(result.current.isLoading).toBe(false);
  });

  it('deve buscar endereço para CEP válido', async () => {
    const mockCepData = {
      cep: '20550160',
      logradouro: 'Rua Teste',
      bairro: 'Bairro Teste',
      cidade: 'Rio de Janeiro',
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
});
