import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { 
  useAllowedCities, 
  useAllowedCityById, 
  useCreateAllowedCity, 
  useUpdateAllowedCity, 
  usePatchAllowedCity,
  useDeleteAllowedCity 
} from '@/hooks/admin/use-allowed-cities';
import * as api from '@/lib/api/generated';

vi.mock('@/lib/api/generated', () => ({
  apiAllowedCitiesGet: vi.fn(),
  apiAllowedCitiesGet2: vi.fn(),
  apiAllowedCitiesPost: vi.fn(),
  apiAllowedCitiesPut: vi.fn(),
  apiAllowedCitiesPatch: vi.fn(),
  apiAllowedCitiesDelete: vi.fn(),
}));

describe('useAllowedCities Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todas as cidades permitidas', async () => {
    vi.mocked(api.apiAllowedCitiesGet).mockResolvedValue({ data: [{ id: '1', cityName: 'City 1' }] });
    const { result } = renderHook(() => useAllowedCities());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBeDefined();
    expect(result.current.data).toHaveLength(1);
    expect(api.apiAllowedCitiesGet).toHaveBeenCalled();
  });

  it('deve buscar cidade por ID', async () => {
    vi.mocked(api.apiAllowedCitiesGet2).mockResolvedValue({ data: { id: 'r-1', cityName: 'City Name' } });
    const { result } = renderHook(() => useAllowedCityById('r-1'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.cityName).toBe('City Name');
    expect(api.apiAllowedCitiesGet2).toHaveBeenCalledWith({ path: { id: 'r-1' } });
  });

  it('deve criar uma cidade permitida', async () => {
    vi.mocked(api.apiAllowedCitiesPost).mockResolvedValue({ data: { id: 'new' } });
    const { result } = renderHook(() => useCreateAllowedCity());
    
    await result.current.mutateAsync({ cityName: 'New City', stateCode: 'RJ' });
    expect(api.apiAllowedCitiesPost).toHaveBeenCalledWith({ 
      body: { cityName: 'New City', stateCode: 'RJ' } 
    });
  });

  it('deve atualizar uma cidade permitida (formato simplificado)', async () => {
    vi.mocked(api.apiAllowedCitiesPut).mockResolvedValue({ data: { id: '1' } });
    const { result } = renderHook(() => useUpdateAllowedCity());
    
    await result.current.mutateAsync({ id: '1', body: { cityName: 'Updated' } });
    expect(api.apiAllowedCitiesPut).toHaveBeenCalledWith({ 
      path: { id: '1' }, 
      body: { cityName: 'Updated' } 
    });
  });

  it('deve atualizar uma cidade permitida (formato completo)', async () => {
    vi.mocked(api.apiAllowedCitiesPut).mockResolvedValue({ data: { id: '1' } });
    const { result } = renderHook(() => useUpdateAllowedCity());
    
    await result.current.mutateAsync({ path: { id: '1' }, body: { cityName: 'Updated Full' } });
    expect(api.apiAllowedCitiesPut).toHaveBeenCalledWith({ 
      path: { id: '1' }, 
      body: { cityName: 'Updated Full' } 
    });
  });

  it('deve atualizar parcialmente uma cidade permitida', async () => {
    vi.mocked(api.apiAllowedCitiesPatch).mockResolvedValue({ data: { id: '1' } });
    const { result } = renderHook(() => usePatchAllowedCity());
    
    await result.current.mutateAsync({ id: '1', body: { isActive: false } });
    expect(api.apiAllowedCitiesPatch).toHaveBeenCalledWith({ 
      path: { id: '1' }, 
      body: { isActive: false } 
    });
  });

  it('deve deletar uma cidade permitida', async () => {
    vi.mocked(api.apiAllowedCitiesDelete).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useDeleteAllowedCity());
    
    await result.current.mutateAsync('r-1');
    expect(api.apiAllowedCitiesDelete).toHaveBeenCalledWith({ path: { id: 'r-1' } });
  });

  it('deve mapear erros (error matrices) caso a API falhe', async () => {
    vi.mocked(api.apiAllowedCitiesGet).mockRejectedValue(new Error('Matrix Failure'));
    const { result } = renderHook(() => useAllowedCities());
    
    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBeDefined();
    expect(result.current.error?.message).toBe('Matrix Failure');
  });
});
