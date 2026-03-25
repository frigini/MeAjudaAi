import { describe, it, expect } from 'vitest';
import { DEFAULT_VERIFICATION_STATUS } from '@/lib/constants';
import { EVerificationStatus } from '@/types/api/provider';

describe('constants', () => {
  it('deve ter status de verificação padrão como Pending', () => {
    expect(DEFAULT_VERIFICATION_STATUS).toBe(EVerificationStatus.Pending);
  });
});
