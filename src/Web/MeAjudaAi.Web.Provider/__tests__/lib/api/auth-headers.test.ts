import { describe, it, expect, vi } from 'vitest';

vi.mock('@/auth', () => ({
  auth: vi.fn(),
}));

describe('getAuthHeaders', () => {
  it('should return empty headers when no session', async () => {
    const { auth } = await import('@/auth');
    vi.mocked(auth).mockResolvedValueOnce(null);
    
    const { getAuthHeaders } = await import('@/lib/api/auth-headers');
    const headers = await getAuthHeaders();
    expect(headers).toEqual({});
  });

  it('should return headers with Bearer token when session has accessToken', async () => {
    const { auth } = await import('@/auth');
    vi.mocked(auth).mockResolvedValueOnce({
      accessToken: 'test-token-123'
    } as any);
    
    const { getAuthHeaders } = await import('@/lib/api/auth-headers');
    const headers = await getAuthHeaders();
    expect(headers).toEqual({ Authorization: 'Bearer test-token-123' });
  });

  it('should return empty headers when session exists but no accessToken', async () => {
    const { auth } = await import('@/auth');
    vi.mocked(auth).mockResolvedValueOnce({} as any);
    
    const { getAuthHeaders } = await import('@/lib/api/auth-headers');
    const headers = await getAuthHeaders();
    expect(headers).toEqual({});
  });
});
