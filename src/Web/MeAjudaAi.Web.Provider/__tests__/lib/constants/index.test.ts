import { describe, it, expect } from 'vitest';
import { DEFAULT_VERIFICATION_STATUS } from '@/lib/constants';
import { EVerificationStatus } from '@/types/api/provider';

describe('constants', () => {
  it('should export DEFAULT_VERIFICATION_STATUS as Pending', () => {
    expect(DEFAULT_VERIFICATION_STATUS).toBe(EVerificationStatus.Pending);
  });
});
