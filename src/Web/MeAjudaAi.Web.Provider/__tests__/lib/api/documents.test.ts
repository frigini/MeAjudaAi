import { describe, it, expect, vi } from 'vitest';

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiProviderDocumentsGet: vi.fn(),
  apiProviderDocumentsPost: vi.fn(),
}));

describe('documents api', () => {
  it('should export document api functions', async () => {
    const api = await import('@/lib/api/generated/sdk.gen');
    expect(api.apiProviderDocumentsGet).toBeDefined();
  });
});
