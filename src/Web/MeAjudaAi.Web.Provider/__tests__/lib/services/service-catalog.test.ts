import { describe, it, expect, vi } from 'vitest';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiServicesGet: vi.fn(),
}));

describe('getPopularServices', () => {
  it('should return empty array on error', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockRejectedValueOnce(new Error('API Error'));
    
    const { getPopularServices } = await import('@/lib/services/service-catalog');
    const result = await getPopularServices();
    
    expect(result).toEqual([]);
  });

  it('should return services when API succeeds', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    const mockServices = [{ id: '1', name: 'Serviço 1' }];
    vi.mocked(apiServicesGet).mockResolvedValueOnce({ data: { data: mockServices } } as any);
    
    const { getPopularServices } = await import('@/lib/services/service-catalog');
    const result = await getPopularServices();
    
    expect(result).toEqual(mockServices);
  });

  it('should return empty array when data is undefined', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockResolvedValueOnce({ data: undefined } as any);
    
    const { getPopularServices } = await import('@/lib/services/service-catalog');
    const result = await getPopularServices();
    
    expect(result).toEqual([]);
  });

  it('should return empty array when data.data is null', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockResolvedValueOnce({ data: { data: null } } as any);
    
    const { getPopularServices } = await import('@/lib/services/service-catalog');
    const result = await getPopularServices();
    
    expect(result).toEqual([]);
  });

  it('should pass activeOnly: true to API', async () => {
    const { apiServicesGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiServicesGet).mockResolvedValueOnce({ data: { data: [] } } as any);
    
    const { getPopularServices } = await import('@/lib/services/service-catalog');
    await getPopularServices();
    
    expect(apiServicesGet).toHaveBeenCalledWith({
      query: { activeOnly: true }
    });
  });
});
