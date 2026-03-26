import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { 
  useAllowedCities, 
  useAllowedCityById, 
  useCreateAllowedCity, 
  useUpdateAllowedCity, 
  useDeleteAllowedCity 
} from '@/hooks/admin/use-allowed-cities';
import * as api from '@/lib/api/generated';

vi.mock('@/lib/api/generated', () => ({
  apiAllowedCitiesGet: vi.fn(),
  apiAllowedCitiesGet2: vi.fn(),
  apiAllowedCitiesPost: vi.fn(),
  apiAllowedCitiesPut: vi.fn(),
  apiAllowedCitiesDelete: vi.fn(),
}));

describe('useAllowedCities Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todas as cidades permitidas', async () => {
    vi.mocked(api.apiAllowedCitiesGet).mockResolvedValue([{ id: '1', cityName: 'City 1' }] as any);
    const { result } = renderHook(() => useAllowedCities());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(api.apiAllowedCitiesGet).toHaveBeenCalled();
  });

  it('deve buscar cidade por ID', async () => {
    vi.mocked(api.apiAllowedCitiesGet2).mockResolvedValue({ id: 'r-1', cityName: 'City Name' } as any);
    const { result } = renderHook(() => useAllowedCityById('r-1'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data.cityName).toBe('City Name');
    expect(api.apiAllowedCitiesGet2).toHaveBeenCalledWith({ path: { id: 'r-1' } });
  });

  it('deve criar uma cidade permitida', async () => {
    vi.mocked(api.apiAllowedCitiesPost).mockResolvedValue({ id: 'new' } as any);
    const { result } = renderHook(() => useCreateAllowedCity());
    
    await result.current.mutateAsync({ cityName: 'New City', stateCode: 'RJ' });
    expect(api.apiAllowedCitiesPost).toHaveBeenCalledWith({ 
      body: { cityName: 'New City', stateCode: 'RJ' } 
    });
  });

  it('deve atualizar uma cidade permitida', async () => {
    vi.mocked(api.apiAllowedCitiesPut).mockResolvedValue({ id: '1' } as any);
    const { result } = renderHook(() => useUpdateAllowedCity());
    
    await result.current.mutateAsync({ id: '1', body: { cityName: 'Updated' } });
    expect(api.apiAllowedCitiesPut).toHaveBeenCalledWith({ 
      path: { id: '1' }, 
      body: { cityName: 'Updated' } 
    });
  });

  it('deve deletar uma cidade permitida', async () => {
    vi.mocked(api.apiAllowedCitiesDelete).mockResolvedValue({ success: true } as any);
    const { result } = renderHook(() => useDeleteAllowedCity());
    
    await result.current.mutateAsync('r-1');
    expect(api.apiAllowedCitiesDelete).toHaveBeenCalledWith({ path: { id: 'r-1' } });
  });
});
