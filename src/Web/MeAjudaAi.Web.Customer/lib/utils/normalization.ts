import { EProviderType, EVerificationStatus } from "@/types/api/provider";

const providerTypeValues = Object.values(EProviderType).filter((v): v is number => typeof v === 'number');
const verificationStatusValues = Object.values(EVerificationStatus).filter((v) => typeof v === 'string');

/**
 * Normaliza valores de entrada para o enum EProviderType.
 */
export function normalizeProviderType(val: unknown): EProviderType {
    if (typeof val === 'number') {
        return providerTypeValues.includes(val) ? val : EProviderType.None;
    }
    if (typeof val === 'string') {
        const lower = val.toLowerCase();
        if (lower === 'none') return EProviderType.None;
        if (lower === 'individual' || lower === 'pessoafisica') return EProviderType.Individual;
        if (lower === 'company' || lower === 'pessoajuridica') return EProviderType.Company;
        if (lower === 'freelancer' || lower === 'autonomo') return EProviderType.Freelancer;
        if (lower === 'cooperative' || lower === 'cooperativa') return EProviderType.Cooperative;

        if (/^\d+$/.test(val)) {
            const num = parseInt(val, 10);
            return providerTypeValues.includes(num) ? num : EProviderType.None;
        }
    }
    return EProviderType.None;
}

/**
 * Normaliza valores de entrada para o enum EVerificationStatus.
 */
export function normalizeVerificationStatus(val: unknown): EVerificationStatus | undefined {
    if (typeof val === 'string') {
        if (/^\d+$/.test(val)) {
            const num = parseInt(val, 10);
            if (num >= 0 && num < verificationStatusValues.length) {
                return verificationStatusValues[num] as EVerificationStatus;
            }
            return undefined;
        }
        const lower = val.toLowerCase();
        if (lower === 'verified') return EVerificationStatus.Verified;
        if (lower === 'rejected') return EVerificationStatus.Rejected;
        if (lower === 'inprogress' || lower === 'in_progress') return EVerificationStatus.InProgress;
        if (lower === 'suspended') return EVerificationStatus.Suspended;
        if (lower === 'none') return EVerificationStatus.None;
        return undefined;
    }
    return undefined;
}
