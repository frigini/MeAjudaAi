import { describe, it, expect } from 'vitest';
import { normalizeProviderType, normalizeVerificationStatus } from '@/lib/utils/normalization';
import { EProviderType, EVerificationStatus } from '@/types/api/provider';

describe('normalizeProviderType', () => {
  it('should return EProviderType for number input', () => {
    expect(normalizeProviderType(0)).toBe(EProviderType.Individual);
    expect(normalizeProviderType(2)).toBe(EProviderType.Company);
  });

  it('should return EProviderType.None for string none', () => {
    expect(normalizeProviderType('none')).toBe(EProviderType.None);
  });

  it('should return EProviderType.Individual for string individual', () => {
    expect(normalizeProviderType('individual')).toBe(EProviderType.Individual);
    expect(normalizeProviderType('pessoafisica')).toBe(EProviderType.Individual);
  });

  it('should return EProviderType.Company for string company', () => {
    expect(normalizeProviderType('company')).toBe(EProviderType.Company);
    expect(normalizeProviderType('pessoajuridica')).toBe(EProviderType.Company);
  });

  it('should return EProviderType.Freelancer for string freelancer', () => {
    expect(normalizeProviderType('freelancer')).toBe(EProviderType.Freelancer);
    expect(normalizeProviderType('autonomo')).toBe(EProviderType.Freelancer);
  });

  it('should return EProviderType.Cooperative for string cooperative', () => {
    expect(normalizeProviderType('cooperative')).toBe(EProviderType.Cooperative);
    expect(normalizeProviderType('cooperativa')).toBe(EProviderType.Cooperative);
  });

  it('should parse numeric string', () => {
    expect(normalizeProviderType('0')).toBe(EProviderType.Individual);
    expect(normalizeProviderType('2')).toBe(EProviderType.Company);
  });

  it('should return EProviderType.None for invalid input', () => {
    expect(normalizeProviderType('invalid')).toBe(EProviderType.None);
  });

  it('should return EProviderType.None for null input', () => {
    expect(normalizeProviderType(null)).toBe(EProviderType.None);
  });

  it('should return EProviderType.None for object input', () => {
    expect(normalizeProviderType({})).toBe(EProviderType.None);
  });

  it('should return EProviderType.None for undefined input', () => {
    expect(normalizeProviderType(undefined)).toBe(EProviderType.None);
  });
});

describe('normalizeVerificationStatus', () => {
  it('should return EVerificationStatus.Verified for string verified', () => {
    expect(normalizeVerificationStatus('verified')).toBe(EVerificationStatus.Verified);
  });

  it('should return EVerificationStatus.Rejected for string rejected', () => {
    expect(normalizeVerificationStatus('rejected')).toBe(EVerificationStatus.Rejected);
  });

  it('should return EVerificationStatus.Suspended for string suspended', () => {
    expect(normalizeVerificationStatus('suspended')).toBe(EVerificationStatus.Suspended);
  });

  it('should return EVerificationStatus.InProgress for string inprogress', () => {
    expect(normalizeVerificationStatus('inprogress')).toBe(EVerificationStatus.InProgress);
  });

  it('should return EVerificationStatus.InProgress for string in_progress', () => {
    expect(normalizeVerificationStatus('in_progress')).toBe(EVerificationStatus.InProgress);
  });

  it('should return EVerificationStatus.Pending for string pending', () => {
    expect(normalizeVerificationStatus('pending')).toBe(EVerificationStatus.Pending);
  });

  it('should return EVerificationStatus.UnderReview for string underreview', () => {
    expect(normalizeVerificationStatus('underreview')).toBe(EVerificationStatus.UnderReview);
  });

  it('should return EVerificationStatus.UnderReview for string under_review', () => {
    expect(normalizeVerificationStatus('under_review')).toBe(EVerificationStatus.UnderReview);
  });

  it('should return EVerificationStatus.Approved for string approved', () => {
    expect(normalizeVerificationStatus('approved')).toBe(EVerificationStatus.Approved);
  });

  it('should pass through number input', () => {
    expect(normalizeVerificationStatus(3)).toBe(3);
  });

  it('should return undefined for invalid string', () => {
    expect(normalizeVerificationStatus('invalid')).toBeUndefined();
  });

  it('should pass through null input', () => {
    expect(normalizeVerificationStatus(null)).toBeNull();
  });

  it('should return EVerificationStatus.None for string none', () => {
    expect(normalizeVerificationStatus('none')).toBe(EVerificationStatus.None);
  });
});
