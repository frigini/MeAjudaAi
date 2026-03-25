import { describe, it, expect } from 'vitest';
import { normalizeProviderType, normalizeVerificationStatus } from '@/lib/utils/normalization';
import { EProviderType, EVerificationStatus } from '@/types/api/provider';

describe('normalizeProviderType', () => {
  it.each([
    { input: 0, expected: EProviderType.None },
    { input: 1, expected: EProviderType.Individual },
    { input: '0', expected: EProviderType.None },
    { input: '1', expected: EProviderType.Individual },
    { input: 'individual', expected: EProviderType.Individual },
    { input: 'INDIVIDUAL', expected: EProviderType.Individual },
    { input: 'pessoafisica', expected: EProviderType.Individual },
    { input: 'company', expected: EProviderType.Company },
    { input: 'pessoajuridica', expected: EProviderType.Company },
    { input: 'freelancer', expected: EProviderType.Freelancer },
    { input: 'autonomo', expected: EProviderType.Freelancer },
    { input: 'cooperative', expected: EProviderType.Cooperative },
    { input: 'cooperativa', expected: EProviderType.Cooperative },
  ])('deve converter "$input" para $expected', ({ input, expected }) => {
    expect(normalizeProviderType(input)).toBe(expected);
  });

  it('deve retornar None para valores desconhecidos', () => {
    expect(normalizeProviderType('unknown')).toBe(EProviderType.None);
  });

  it('deve retornar None para null', () => {
    expect(normalizeProviderType(null)).toBe(EProviderType.None);
  });

  it('deve retornar None para undefined', () => {
    expect(normalizeProviderType(undefined)).toBe(EProviderType.None);
  });
});

describe('normalizeVerificationStatus', () => {
  it.each([
    { input: 0, expected: EVerificationStatus.None },
    { input: 1, expected: EVerificationStatus.Pending },
    { input: 2, expected: EVerificationStatus.InProgress },
    { input: 3, expected: EVerificationStatus.Verified },
    { input: 4, expected: EVerificationStatus.Rejected },
    { input: 5, expected: EVerificationStatus.Suspended },
    { input: '0', expected: EVerificationStatus.None },
    { input: '1', expected: EVerificationStatus.Pending },
    { input: 'verified', expected: EVerificationStatus.Verified },
    { input: 'VERIFIED', expected: EVerificationStatus.Verified },
    { input: 'rejected', expected: EVerificationStatus.Rejected },
    { input: 'inprogress', expected: EVerificationStatus.InProgress },
    { input: 'in_progress', expected: EVerificationStatus.InProgress },
    { input: 'suspended', expected: EVerificationStatus.Suspended },
    { input: 'none', expected: EVerificationStatus.None },
  ])('deve converter "$input" para $expected', ({ input, expected }) => {
    expect(normalizeVerificationStatus(input)).toBe(expected);
  });

  it('deve retornar undefined para valores desconhecidos', () => {
    expect(normalizeVerificationStatus('unknown')).toBeUndefined();
  });

  it.each([null, undefined])('deve tratar %s graciosamente', (input) => {
    const result = normalizeVerificationStatus(input);
    expect(result).toBeFalsy();
  });
});
