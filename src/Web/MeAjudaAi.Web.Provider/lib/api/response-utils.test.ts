import { describe, it, expect } from 'vitest';
import { unwrapResponse } from './response-utils';

describe('response-utils (Provider)', () => {
  it('deve retornar undefined se o response for null ou undefined', () => {
    expect(unwrapResponse(null)).toBeUndefined();
    expect(unwrapResponse(undefined)).toBeUndefined();
  });

  it('deve desempacotar objeto com propriedade "value"', () => {
    const response = { value: { id: 1, name: 'Test' } };
    expect(unwrapResponse(response)).toEqual({ id: 1, name: 'Test' });
  });

  it('deve desempacotar objeto com propriedade "result"', () => {
    const response = { result: { success: true } };
    expect(unwrapResponse(response)).toEqual({ success: true });
  });

  it('deve retornar o próprio objeto se não estiver empacotado em "value" ou "result"', () => {
    const response = { id: 2, name: 'Direct' };
    expect(unwrapResponse(response)).toEqual({ id: 2, name: 'Direct' });
  });

  it('deve retornar o valor primitivo se o response for um primitivo', () => {
    expect(unwrapResponse('string')).toBe('string');
    expect(unwrapResponse(123)).toBe(123);
    expect(unwrapResponse(true)).toBe(true);
  });
});
