import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ApiError, baseFetch, authenticatedFetch, publicFetch } from '@/lib/api/fetch-client';

vi.mock('@/lib/api/client', () => ({
  client: {
    getConfig: vi.fn(() => ({ baseUrl: 'http://localhost:7002' })),
  },
}));

const createMockHeaders = (init?: Record<string, string>) => {
  const headers = new Headers();
  if (init) {
    Object.entries(init).forEach(([key, value]) => {
      headers.append(key, value);
    });
  }
  return headers;
};

describe('ApiError', () => {
  it('deve criar ApiError com mensagem e status', () => {
    const error = new ApiError('Error message', 404);
    expect(error.message).toBe('Error message');
    expect(error.status).toBe(404);
    expect(error.name).toBe('ApiError');
  });
});

describe('baseFetch', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('deve fazer requisição GET', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: createMockHeaders({ 'content-length': '10' }),
      json: async () => ({ data: { id: '1' } }),
    } as any);

    const result = await baseFetch('/api/test', { method: 'get' });
    expect(result).toEqual({ id: '1' });
  });

  it('deve fazer requisição com token', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: createMockHeaders({ 'content-length': '10' }),
      json: async () => ({ data: { id: '1' } }),
    } as any);

    await baseFetch('/api/test', { method: 'get', token: 'test-token', requireAuth: true });
    
    expect(global.fetch).toHaveBeenCalledWith(
      'http://localhost:7002/api/test',
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      })
    );
  });

  it('deve rejeitar quando requireAuth mas sem token', async () => {
    await expect(
      baseFetch('/api/test', { method: 'get', requireAuth: true, token: null })
    ).rejects.toThrow('Missing access token');
  });

  it('deve tratar erro de resposta', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: false,
      status: 400,
      statusText: 'Bad Request',
      json: async () => ({ message: 'Validation error' }),
    } as any);

    await expect(
      baseFetch('/api/test', { method: 'get' })
    ).rejects.toThrow('Validation error');
  });

  it('deve retornar undefined para status 204', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      status: 204,
      headers: createMockHeaders({ 'content-length': '0' }),
    } as any);

    const result = await baseFetch('/api/test', { method: 'delete' });
    expect(result).toBeUndefined();
  });
});

describe('authenticatedFetch', () => {
  it('deve chamar baseFetch com requireAuth true', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: createMockHeaders({ 'content-length': '10' }),
      json: async () => ({ data: { id: '1' } }),
    } as any);

    const result = await authenticatedFetch('/api/test', { method: 'get', token: 'test-token' });
    expect(result).toEqual({ id: '1' });
    expect(global.fetch).toHaveBeenCalledWith(
      'http://localhost:7002/api/test',
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      })
    );
  });
});

describe('publicFetch', () => {
  it('deve chamar baseFetch sem requireAuth', async () => {
    global.fetch = vi.fn().mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: createMockHeaders({ 'content-length': '10' }),
      json: async () => ({ data: { id: '1' } }),
    } as any);

    const result = await publicFetch('/api/test');
    expect(result).toEqual({ id: '1' });
  });
});
