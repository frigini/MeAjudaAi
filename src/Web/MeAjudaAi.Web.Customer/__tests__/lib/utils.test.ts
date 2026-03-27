import { describe, it, expect } from 'vitest';
import { getWhatsappLink } from '@/lib/utils/phone';
import { normalizeProviderType, normalizeVerificationStatus } from '@/lib/utils/normalization';
import { EProviderType, EVerificationStatus } from '@/types/api/provider';

describe('Utility Functions Coverage', () => {
    describe('phone utils', () => {
        it('deve gerar link do whatsapp corretamente', () => {
            expect(getWhatsappLink('11999998888')).toBe('https://wa.me/5511999998888');
            expect(getWhatsappLink('5511999998888')).toBe('https://wa.me/5511999998888');
            expect(getWhatsappLink('123')).toBe(null);
        });
    });

    describe('normalization utils', () => {
        it('deve normalizar tipo de provedor', () => {
            expect(normalizeProviderType(EProviderType.Individual)).toBe(EProviderType.Individual);
            expect(normalizeProviderType('individual')).toBe(EProviderType.Individual);
            expect(normalizeProviderType('pessoafisica')).toBe(EProviderType.Individual);
            expect(normalizeProviderType('company')).toBe(EProviderType.Company);
            expect(normalizeProviderType('none')).toBe(EProviderType.None);
            expect(normalizeProviderType('1')).toBe(1);
            expect(normalizeProviderType({})).toBe(EProviderType.None);
        });

        it('deve normalizar status de verificação', () => {
            expect(normalizeVerificationStatus(EVerificationStatus.Verified)).toBe(EVerificationStatus.Verified);
            expect(normalizeVerificationStatus('verified')).toBe(EVerificationStatus.Verified);
            expect(normalizeVerificationStatus('rejected')).toBe(EVerificationStatus.Rejected);
            expect(normalizeVerificationStatus('inprogress')).toBe(EVerificationStatus.InProgress);
            expect(normalizeVerificationStatus('1')).toBe(1);
            expect(normalizeVerificationStatus('none')).toBe(EVerificationStatus.None);
            expect(normalizeVerificationStatus('unknown')).toBe(undefined);
            expect(normalizeVerificationStatus({})).toBeUndefined();
        });
    });
});
