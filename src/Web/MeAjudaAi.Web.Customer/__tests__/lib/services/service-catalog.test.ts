import { describe, it, expect, vi } from 'vitest';
import { getPopularServices } from '@/lib/services/service-catalog';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiServicesGet: vi.fn(),
}));

describe('getPopularServices', () => {
  it('deve retornar lista de serviços', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockResolvedValueOnce({
      data: {
        data: [
          { id: '1', name: 'Elétrica' },
          { id: '2', name: 'Hidráulica' },
        ],
      },
    });

    const result = await getPopularServices();

    expect(result).toHaveLength(2);
    expect(result[0].name).toBe('Elétrica');
  });

  it('deve retornar array vazio em caso de erro', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockRejectedValueOnce(new Error('API Error'));

    const result = await getPopularServices();

    expect(result).toEqual([]);
  });

  it('deve retornar array vazio quando data é null', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockResolvedValueOnce({
      data: null,
    });

    const result = await getPopularServices();

    expect(result).toEqual([]);
  });
});
