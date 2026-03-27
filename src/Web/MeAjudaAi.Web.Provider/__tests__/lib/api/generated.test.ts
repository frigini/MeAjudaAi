import { describe, it, expect } from 'vitest';

describe('sdk exports', () => {
  it('should export SDK functions', async () => {
    const sdk = await import('@/lib/api/generated/sdk.gen');
    expect(sdk).toBeDefined();
  });

  it('should export types', async () => {
    const types = await import('@/lib/api/generated/types.gen');
    expect(types).toBeDefined();
  });
});
