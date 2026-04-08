import { describe, it, expect } from 'vitest';
import { unwrapResponse } from '@/lib/api/response-utils';

describe('unwrapResponse', () => {
  it('deve retornar undefined para null', () => {
    expect(unwrapResponse(null)).toBeUndefined();
  });

  it('deve retornar undefined para undefined', () => {
    expect(unwrapResponse(undefined)).toBeUndefined();
  });

  it('deve extrair valor de wrapper { value: T }', () => {
    const wrapper = { value: { id: '1', name: 'Test' } };
    const result = unwrapResponse<{ id: string; name: string }>(wrapper);
    expect(result).toEqual({ id: '1', name: 'Test' });
  });

  it('deve extrair valor de wrapper { result: T }', () => {
    const wrapper = { result: { id: '2', name: 'Test 2' } };
    const result = unwrapResponse<{ id: string; name: string }>(wrapper);
    expect(result).toEqual({ id: '2', name: 'Test 2' });
  });

  it('deve retornar resposta direta se não for wrapper', () => {
    const direct = { id: '3', name: 'Direct' };
    const result = unwrapResponse(direct);
    expect(result).toEqual({ id: '3', name: 'Direct' });
  });

  it('deve retornar string diretamente', () => {
    const result = unwrapResponse<string>('test string');
    expect(result).toBe('test string');
  });

  it('deve retornar array diretamente', () => {
    const arr = [1, 2, 3];
    const result = unwrapResponse<number[]>(arr);
    expect(result).toEqual([1, 2, 3]);
  });
});
