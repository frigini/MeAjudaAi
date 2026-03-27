import { describe, it, expect, vi } from 'vitest';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiProviderProfileGet: vi.fn(),
}));

vi.mock('@/lib/api/generated/types.gen', () => ({}));

describe('api client', () => {
  it('should export client', async () => {
    const { client } = await import('@/lib/api/client');
    expect(client).toBeDefined();
  });
});
