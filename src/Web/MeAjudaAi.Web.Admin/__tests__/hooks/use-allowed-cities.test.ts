import { describe, it, expect, vi } from 'vitest';
import { renderHook, waitFor } from 'test-support';

vi.mock('@/lib/api/generated', () => ({
  apiAllowedCitiesGet: vi.fn().mockResolvedValue({ data: [
    { id: 'city-1', cityName: 'Muriaé', stateCode: 'MG', isActive: true },
  ]}),
  apiAllowedCitiesGet2: vi.fn().mockResolvedValue({ data: { id: 'city-1', cityName: 'Muriaé' } }),
  apiAllowedCitiesPost: vi.fn().mockResolvedValue({ data: {} }),
  apiAllowedCitiesPut: vi.fn().mockResolvedValue({ data: {} }),
  apiAllowedCitiesPatch: vi.fn().mockResolvedValue({ data: {} }),
  apiAllowedCitiesDelete: vi.fn().mockResolvedValue({}),
}));

import { useAllowedCities, useAllowedCityById, allowedCitiesKeys } from '@/hooks/admin/use-allowed-cities';

describe('useAllowedCities', () => {
  it('deve retornar lista de cidades permitidas', async () => {
    const { result } = renderHook(() => useAllowedCities());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
  });
});

describe('useAllowedCityById', () => {
  it('deve estar desabilitado quando id está vazio', () => {
    const { result } = renderHook(() => useAllowedCityById(''));
    expect(result.current.fetchStatus).toBe('idle');
  });

  it('deve buscar cidade por id', async () => {
    const { result } = renderHook(() => useAllowedCityById('city-1'));

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
  });
});

describe('allowedCitiesKeys', () => {
  it('deve gerar query keys corretamente', () => {
    expect(allowedCitiesKeys.all).toEqual(['allowedCities']);
    expect(allowedCitiesKeys.lists()).toEqual(['allowedCities', 'list']);
    expect(allowedCitiesKeys.detail('x')).toEqual(['allowedCities', 'detail', 'x']);
  });
});
