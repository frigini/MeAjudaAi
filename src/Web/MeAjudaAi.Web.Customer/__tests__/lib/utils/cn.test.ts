import { describe, it, expect } from 'vitest';
import { cn } from '@/lib/utils/cn';

describe('cn', () => {
  it('deve mesclar classes corretamente', () => {
    expect(cn('foo', 'bar')).toBe('foo bar');
  });

  it('deve substituir classes conflitantes', () => {
    expect(cn('p-4 p-2')).toBe('p-2');
  });

  it('deve lidar com classes condicionais', () => {
    const condition = true;
    expect(cn('foo', condition && 'bar')).toBe('foo bar');
  });

  it('deve lidar com arrays', () => {
    expect(cn(['foo', 'bar'])).toBe('foo bar');
  });

  it('deve lidar com objetos', () => {
    expect(cn({ foo: true, bar: false })).toBe('foo');
  });

  it('deve retornar string vazia para entrada vazia', () => {
    expect(cn()).toBe('');
  });
});
