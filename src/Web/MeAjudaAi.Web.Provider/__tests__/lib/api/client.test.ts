import { describe, it, expect, vi } from 'vitest';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiProviderProfileGet: vi.fn(),
}));

vi.mock('@/lib/api/generated/types.gen', () => ({}));

vi.stubGlobal('process', {
  env: {
    NEXT_PUBLIC_API_URL: 'http://test-api:3000'
  }
});

describe('api client', () => {
  it('should export client', async () => {
    const { client } = await import('@/lib/api/client');
    expect(client).toBeDefined();
  });

  it('should have default baseUrl configuration', async () => {
    const { client } = await import('@/lib/api/client');
    const config = client.getConfig();
    expect(config.baseUrl).toBeDefined();
  });

  it('should set Content-Type header by default', async () => {
    const { client } = await import('@/lib/api/client');
    const config = client.getConfig();
    expect(config.headers).toBeDefined();
    expect(config.headers['Content-Type']).toBe('application/json');
  });
});
