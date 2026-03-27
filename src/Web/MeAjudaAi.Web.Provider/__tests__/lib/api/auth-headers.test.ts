import { describe, it, expect, vi } from 'vitest';

vi.mock('next-auth/react', () => ({
  getSession: vi.fn().mockResolvedValue(null),
}));

describe('auth-headers', () => {
  it('should export getAuthHeaders', async () => {
    const { getAuthHeaders } = await import('@/lib/api/auth-headers');
    expect(getAuthHeaders).toBeDefined();
  });
});
