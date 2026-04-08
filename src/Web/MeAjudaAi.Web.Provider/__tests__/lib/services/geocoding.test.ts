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
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: [] } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('CidadeInvalida');
    
    expect(result).toBeNull();
  });

  it('should return null when latitude is null', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: [{ latitude: null, longitude: -42.456 }] } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });

  it('should return null when longitude is null', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: [{ latitude: -21.123, longitude: null }] } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });

  it('should return null when latitude/longitude missing', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: [{}] } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });

  it('should return GeocodingResult when valid data found', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ 
      data: [{ 
        latitude: -21.123, 
        longitude: -42.456,
        displayName: 'Muriaé, MG'
      }]
    } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toEqual({
      latitude: -21.123,
      longitude: -42.456,
      displayName: 'Muriaé, MG'
    });
  });

  it('should use query as displayName when displayName is missing', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ 
      data: [{ 
        latitude: -21.123, 
        longitude: -42.456
      }]
    } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toEqual({
      latitude: -21.123,
      longitude: -42.456,
      displayName: 'Muriaé'
    });
  });

  it('should use empty displayName when it is empty string', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ 
      data: [{ 
        latitude: -21.123, 
        longitude: -42.456,
        displayName: ''
      }]
    } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toEqual({
      latitude: -21.123,
      longitude: -42.456,
      displayName: ''
    });
  });

  it('should catch errors and return null', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockRejectedValueOnce(new Error('API Error'));
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });

  it('should return null when data.data is null', async () => {
    const { apiSearchGet } = await import('@/lib/api/generated/sdk.gen');
    vi.mocked(apiSearchGet).mockResolvedValueOnce({ data: null } as any);
    
    const { geocodeCity } = await import('@/lib/services/geocoding');
    const result = await geocodeCity('Muriaé');
    
    expect(result).toBeNull();
  });
});
