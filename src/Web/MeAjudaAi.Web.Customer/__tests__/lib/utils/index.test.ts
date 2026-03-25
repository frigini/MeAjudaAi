import { describe, it, expect } from 'vitest';
import * as utils from '@/lib/utils';

describe('lib/utils index', () => {
  it('deve exportar cn', () => {
    expect(utils.cn).toBeDefined();
    expect(typeof utils.cn).toBe('function');
  });
});
