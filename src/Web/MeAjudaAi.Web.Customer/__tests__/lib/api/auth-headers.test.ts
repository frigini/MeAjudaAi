import { describe, it, expect, vi } from 'vitest';
import { getAuthHeaders } from '@/lib/api/auth-headers';

vi.mock('@/auth', () => ({
  auth: vi.fn(),
}));

describe('getAuthHeaders', () => {
  it('deve retornar headers vazios quando não há sessão', async () => {
    const { auth } = await import('@/auth');
    vi.mocked(auth).mockResolvedValueOnce(null);

    const headers = await getAuthHeaders();
    expect(headers).toEqual({});
  });

  it('deve retornar headers com token quando há sessão', async () => {
    const { auth } = await import('@/auth');
    vi.mocked(auth).mockResolvedValueOnce({
      accessToken: 'mock-token-123'
    });

    const headers = await getAuthHeaders();
    expect(headers).toEqual({
      Authorization: 'Bearer mock-token-123'
    });
  });
});
