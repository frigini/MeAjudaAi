import { describe, it, expect } from 'vitest';
import { getVerificationBadgeVariant } from './utils';

describe('Admin utils', () => {
  it('deve retornar "success" para status Verified (3)', () => {
    expect(getVerificationBadgeVariant(3)).toBe('success');
  });

  it('deve retornar "warning" para status InProgress (2)', () => {
    expect(getVerificationBadgeVariant(2)).toBe('warning');
  });

  it('deve retornar "warning" para status Suspended (5)', () => {
    expect(getVerificationBadgeVariant(5)).toBe('warning');
  });

  it('deve retornar "destructive" para status Rejected (4)', () => {
    expect(getVerificationBadgeVariant(4)).toBe('destructive');
  });

  it('deve retornar "secondary" para status Pending (1) ou None (0)', () => {
    expect(getVerificationBadgeVariant(1)).toBe('secondary');
    expect(getVerificationBadgeVariant(0)).toBe('secondary');
  });

  it('deve retornar "secondary" para status indefinido ou desconhecido', () => {
    expect(getVerificationBadgeVariant(undefined)).toBe('secondary');
    // @ts-expect-error - Testando um status numérico inválido intencionalmente
    expect(getVerificationBadgeVariant(99)).toBe('secondary');
  });
});
