import { describe, it, expect } from 'vitest';
import * as utils from '@/lib/utils';

describe('lib/utils index', () => {
  it('deve exportar cn', () => {
    expect(utils.cn).toBeDefined();
    expect(typeof utils.cn).toBe('function');
  });

  it('deve usar cn para combinar classes', () => {
    const result = utils.cn('foo', false && 'bar', 'baz');
    expect(result).toBe('foo baz');
  });

  it('deve usar cn com objetos', () => {
    const result = utils.cn({ foo: true, bar: false, baz: true });
    expect(result).toBe('foo baz');
  });
});
