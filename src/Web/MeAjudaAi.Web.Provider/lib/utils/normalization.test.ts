import { describe, it, expect } from 'vitest';
import { normalizeProviderType, normalizeVerificationStatus } from './normalization';
import { EProviderType, EVerificationStatus } from '@/types/api/provider';

describe('Normalization Utils (Provider)', () => {
  describe('normalizeProviderType', () => {
    it('deve retornar o valor numérico se for número', () => {
      expect(normalizeProviderType(EProviderType.Individual)).toBe(EProviderType.Individual);
    });

    it('deve normalizar strings para EProviderType', () => {
      expect(normalizeProviderType('Individual')).toBe(EProviderType.Individual);
      expect(normalizeProviderType('pessoafisica')).toBe(EProviderType.Individual);
      expect(normalizeProviderType('Company')).toBe(EProviderType.Company);
      expect(normalizeProviderType('pessoajuridica')).toBe(EProviderType.Company);
      expect(normalizeProviderType('Freelancer')).toBe(EProviderType.Freelancer);
      expect(normalizeProviderType('autonomo')).toBe(EProviderType.Freelancer);
      expect(normalizeProviderType('Cooperative')).toBe(EProviderType.Cooperative);
      expect(normalizeProviderType('cooperativa')).toBe(EProviderType.Cooperative);
      expect(normalizeProviderType('None')).toBe(EProviderType.None);
    });

    it('deve converter strings numéricas', () => {
      expect(normalizeProviderType('2')).toBe(EProviderType.Company);
    });

    it('deve retornar None para valores inválidos', () => {
      expect(normalizeProviderType('invalid')).toBe(EProviderType.None);
      expect(normalizeProviderType(null)).toBe(EProviderType.None);
    });
  });

  describe('normalizeVerificationStatus', () => {
    it('deve normalizar strings para EVerificationStatus', () => {
      expect(normalizeVerificationStatus('Verified')).toBe(EVerificationStatus.Verified);
      expect(normalizeVerificationStatus('rejected')).toBe(EVerificationStatus.Rejected);
      expect(normalizeVerificationStatus('InProgress')).toBe(EVerificationStatus.InProgress);
      expect(normalizeVerificationStatus('in_progress')).toBe(EVerificationStatus.InProgress);
      expect(normalizeVerificationStatus('suspended')).toBe(EVerificationStatus.Suspended);
      expect(normalizeVerificationStatus('pending')).toBe(EVerificationStatus.Pending);
      expect(normalizeVerificationStatus('UnderReview')).toBe(EVerificationStatus.UnderReview);
      expect(normalizeVerificationStatus('under_review')).toBe(EVerificationStatus.UnderReview);
      expect(normalizeVerificationStatus('approved')).toBe(EVerificationStatus.Approved);
    });

    it('deve retornar undefined para strings inválidas', () => {
      expect(normalizeVerificationStatus('invalid')).toBeUndefined();
    });

    it('deve retornar o valor original se não for string', () => {
      expect(normalizeVerificationStatus(EVerificationStatus.Verified)).toBe(EVerificationStatus.Verified);
      expect(normalizeVerificationStatus(1)).toBe(1);
    });
  });
});
