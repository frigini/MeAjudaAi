import { describe, it, expect, vi, beforeEach } from 'vitest';
import { baseFetch, ApiError, authenticatedFetch, publicFetch } from './fetch-client';
import { client } from "@/lib/api/client";

// Mock global fetch
const globalFetch = vi.stubGlobal('fetch', vi.fn());

vi.mock("@/lib/api/client", () => ({
  client: {
    getConfig: vi.fn(() => ({ baseUrl: 'http://api.test' }))
  }
}));

describe('fetch-client (Provider)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('baseFetch', () => {
    it('deve realizar uma requisição GET básica', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'Content-Type': 'application/json' }),
        json: async () => ({ id: 1 })
      } as Response);

      const result = await baseFetch('/test', { method: 'get' });

      expect(result).toEqual({ id: 1 });
      expect(fetch).toHaveBeenCalledWith('http://api.test/test', expect.objectContaining({
        method: 'get'
      }));
    });

    it('deve incluir token de autorização se requireAuth for true', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'Content-Length': '0' }),
      } as Response);

      await baseFetch('/auth', { requireAuth: true, token: 'secret-token' });

      expect(fetch).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer secret-token'
        })
      }));
    });

    it('deve lançar ApiError se requireAuth for true e token estiver ausente', async () => {
      await expect(baseFetch('/auth', { requireAuth: true, token: null }))
        .rejects.toThrow(new ApiError("Missing access token"));
    });

    it('deve configurar Content-Type se houver body', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'Content-Length': '0' }),
      } as Response);

      await baseFetch('/post', { method: 'post', body: { name: 'Test' } });

      expect(fetch).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({
        headers: expect.objectContaining({
          'Content-Type': 'application/json'
        }),
        body: JSON.stringify({ name: 'Test' })
      }));
    });

    it('deve lidar com respostas 204 No Content', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 204,
        headers: new Headers({ 'Content-Length': '0' }),
      } as Response);

      const result = await baseFetch('/empty', {});
      expect(result).toBeUndefined();
    });

    it('deve lidar com wrapper Result<T> (value)', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'Content-Type': 'application/json' }),
        json: async () => ({ value: { data: 'wrapped' } })
      } as Response);

      const result = await baseFetch('/wrapped', {});
      expect(result).toEqual({ data: 'wrapped' });
    });

    it('deve lançar erro se Result<T> tiver value nulo', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'Content-Type': 'application/json' }),
        json: async () => ({ value: null })
      } as Response);

      await expect(baseFetch('/null', {}))
        .rejects.toThrow(new ApiError("Response contained null/undefined value for expected Result<T>"));
    });

    it('deve lidar com wrapper ApiResponse<T> (data)', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'Content-Type': 'application/json' }),
        json: async () => ({ data: 'api-data', success: true })
      } as Response);

      const result = await baseFetch('/api', {});
      expect(result).toBe('api-data');
    });

    it('deve mapear erros de validação do .NET', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: false,
        status: 400,
        statusText: 'Bad Request',
        json: async () => ({ 
          errors: { 
            Name: ['Name is required'],
            Email: ['Invalid email format']
          } 
        })
      } as Response);

      await expect(baseFetch('/error', {}))
        .rejects.toThrow(new ApiError("Name is required, Invalid email format", 400));
    });

    it('deve mapear erros com o padrão Result.Failure do .NET', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: false,
        status: 404,
        json: async () => ({ 
          error: { description: 'Resource not found' } 
        })
      } as Response);

      await expect(baseFetch('/404', {}))
        .rejects.toThrow(new ApiError("Resource not found", 404));
    });

    it('deve usar statusText como fallback se nenhum mapeamento de erro funcionar', async () => {
      vi.mocked(fetch).mockResolvedValue({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        json: async () => ({})
      } as Response);

      await expect(baseFetch('/500', {}))
        .rejects.toThrow(new ApiError("Request failed: Internal Server Error", 500));
    });
  });

  describe('authenticatedFetch', () => {
    it('deve chamar baseFetch com requireAuth: true', async () => {
      vi.mocked(fetch).mockResolvedValue({ ok: true, status: 200, headers: new Headers() } as any);
      await authenticatedFetch('/auth', { token: 'tok' });
      expect(fetch).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({
        headers: expect.objectContaining({ Authorization: 'Bearer tok' })
      }));
    });
  });

  describe('publicFetch', () => {
    it('deve chamar baseFetch com requireAuth: false', async () => {
      vi.mocked(fetch).mockResolvedValue({ ok: true, status: 200, headers: new Headers() } as any);
      await publicFetch('/public');
      expect(fetch).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({
        headers: expect.not.objectContaining({ Authorization: expect.anything() })
      }));
    });
  });
});
