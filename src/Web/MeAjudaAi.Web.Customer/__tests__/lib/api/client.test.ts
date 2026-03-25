import { describe, it, expect, vi } from 'vitest';

describe('client', () => {
  it('deve exportar client', async () => {
    const { client } = await import('@/lib/api/client');
    expect(client).toBeDefined();
    expect(client.getConfig).toBeDefined();
  });

  it('deve ter configuração baseUrl', async () => {
    const { client } = await import('@/lib/api/client');
    const config = client.getConfig();
    expect(config).toHaveProperty('baseUrl');
  });
});
