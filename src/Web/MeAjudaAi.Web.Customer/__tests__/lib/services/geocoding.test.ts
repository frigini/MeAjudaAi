import { describe, it, expect, vi } from 'vitest';
import { geocodeCity } from '@/lib/services/geocoding';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiSearchGet: vi.fn(),
}));

vi.mock('@/lib/api/auth-headers', () => ({
  getAuthHeaders: vi.fn(() => Promise.resolve({})),
}));

describe('geocodeCity', () => {
  it('deve retornar resultado de geocodificação', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({
      data: [
        {
          latitude: -22.9068,
          longitude: -43.1729,
          displayName: 'Rio de Janeiro, RJ',
        },
      ],
    });

    const result = await geocodeCity('Rio de Janeiro');

    expect(result).toEqual({
      latitude: -22.9068,
      longitude: -43.1729,
      displayName: 'Rio de Janeiro, RJ',
    });
  });

  it('deve retornar null quando não há resultados', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({
      data: [],
    });

    const result = await geocodeCity('CidadeInvalida');

    expect(result).toBeNull();
  });

  it('deve retornar null quando há erro', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockRejectedValueOnce(new Error('Network error'));

    const result = await geocodeCity('Rio de Janeiro');

    expect(result).toBeNull();
  });

  it('deve usar query como displayName quando não disponível', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({
      data: [
        {
          latitude: -22.9068,
          longitude: -43.1729,
          displayName: '',
        },
      ],
    });

    const result = await geocodeCity('Rio de Janeiro');

    expect(result?.displayName).toBe('Rio de Janeiro');
  });
});
