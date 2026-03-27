import { describe, it, expect, vi } from 'vitest';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiSearchGet: vi.fn(),
}));

vi.mock('@/lib/api/auth-headers', () => ({
  getAuthHeaders: vi.fn().mockResolvedValue({}),
}));

describe('geocodeCity', () => {
  it('should return null when no results found', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: { data: [] } } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('CidadeInvalida');
    
    expect(result).toBeNull();
  });

  it('should return null when latitude/longitude missing', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: { data: [{}] } } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });

  it('should catch errors and return null', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockRejectedValueOnce(new Error('API Error'));
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });
});
