import { describe, it, expect, vi, beforeEach } from 'vitest';
import { geocodeCity } from './geocoding';
import { apiSearchGet } from "@/lib/api/generated/sdk.gen";
import { getAuthHeaders } from "@/lib/api/auth-headers";

vi.mock("@/lib/api/generated/sdk.gen", () => ({
  apiSearchGet: vi.fn(),
}));

vi.mock("@/lib/api/auth-headers", () => ({
  getAuthHeaders: vi.fn(),
}));

describe('geocoding service (Provider)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve retornar coordenadas para uma busca bem sucedida', async () => {
    vi.mocked(getAuthHeaders).mockResolvedValue({ Authorization: 'Bearer token' });
    vi.mocked(apiSearchGet).mockResolvedValue({ 
      data: [{ latitude: -23.55, longitude: -46.63, displayName: 'São Paulo, SP' }] 
    } as any);

    const result = await geocodeCity('São Paulo');

    expect(result).toEqual({
      latitude: -23.55,
      longitude: -46.63,
      displayName: 'São Paulo, SP'
    });
    expect(apiSearchGet).toHaveBeenCalledWith(expect.objectContaining({
      query: { query: 'São Paulo' }
    }));
  });

  it('deve usar o query como displayName se displayName estiver ausente na resposta', async () => {
    vi.mocked(apiSearchGet).mockResolvedValue({ 
      data: [{ latitude: 10, longitude: 20, displayName: null }] 
    } as any);

    const result = await geocodeCity('Test City');

    expect(result?.displayName).toBe('Test City');
  });

  it('deve retornar null se a resposta for vazia', async () => {
    vi.mocked(apiSearchGet).mockResolvedValue({ data: [] } as any);

    const result = await geocodeCity('Nonexistent');

    expect(result).toBeNull();
  });

  it('deve retornar null se a resposta for null ou undefined', async () => {
    vi.mocked(apiSearchGet).mockResolvedValue({ data: null } as any);

    const result = await geocodeCity('Error');

    expect(result).toBeNull();
  });

  it('deve retornar null em caso de erro na API', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.mocked(apiSearchGet).mockRejectedValue(new Error('Network Error'));

    const result = await geocodeCity('Fail');

    expect(result).toBeNull();
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });
});
