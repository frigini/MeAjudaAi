import { describe, it, expect } from 'vitest';
import { VerificationStatusSchema } from '@/lib/schemas/verification-status';
import { EVerificationStatus } from '@/types/api/provider';

describe('VerificationStatusSchema', () => {
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
    { input: 'REJECTED', expected: EVerificationStatus.Rejected },
    { input: 'inprogress', expected: EVerificationStatus.InProgress },
    { input: 'in_progress', expected: EVerificationStatus.InProgress },
    { input: 'suspended', expected: EVerificationStatus.Suspended },
    { input: 'none', expected: EVerificationStatus.None },
  ])('deve converter "$input" para $expected', ({ input, expected }) => {
    const result = VerificationStatusSchema.safeParse(input);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data).toBe(expected);
    }
  });

  it('deve retornar undefined para valores desconhecidos', () => {
    const result = VerificationStatusSchema.safeParse('unknown');
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data).toBeUndefined();
    }
  });

  it.each([null, undefined])('deve tratar %s graciosamente', (input) => {
    const result = VerificationStatusSchema.safeParse(input);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data).toBeFalsy();
    }
  });
});
