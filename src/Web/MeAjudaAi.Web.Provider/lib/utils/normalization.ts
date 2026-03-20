import { EProviderType, EVerificationStatus } from "@/types/api/provider";

/**
 * Normaliza valores de entrada para o enum EProviderType.
 */
export function normalizeProviderType(val: unknown): EProviderType {
    if (typeof val === 'number') return val as EProviderType;
    if (typeof val === 'string') {
        const lower = val.toLowerCase();
        if (lower === 'none') return EProviderType.None;
        if (lower === 'individual' || lower === 'pessoafisica') return EProviderType.Individual;
        if (lower === 'company' || lower === 'pessoajuridica') return EProviderType.Company;
        if (lower === 'freelancer' || lower === 'autonomo') return EProviderType.Freelancer;
        if (lower === 'cooperative' || lower === 'cooperativa') return EProviderType.Cooperative;

        if (/^\d+$/.test(val)) {
            return parseInt(val, 10) as EProviderType;
        }
    }
    return EProviderType.None;
}

/**
 * Normaliza valores de entrada para o enum EVerificationStatus.
 */
export function normalizeVerificationStatus(val: unknown): EVerificationStatus | undefined {
    if (typeof val === 'string') {
        const lower = val.toLowerCase();
        if (lower === 'verified') return EVerificationStatus.Verified;
        if (lower === 'rejected') return EVerificationStatus.Rejected;
        if (lower === 'inprogress' || lower === 'in_progress') return EVerificationStatus.InProgress;
        if (lower === 'suspended') return EVerificationStatus.Suspended;
        if (lower === 'none') return EVerificationStatus.None;
        if (lower === 'pending') return EVerificationStatus.Pending;
        if (lower === 'underreview' || lower === 'under_review') return EVerificationStatus.UnderReview;
        if (lower === 'approved') return EVerificationStatus.Approved;
        
        // Check if it's a valid enum value
        const enumValues = Object.values(EVerificationStatus);
        if (enumValues.includes(val as EVerificationStatus)) {
            return val as EVerificationStatus;
        }
        
        return undefined;
    }
    return val as unknown as EVerificationStatus;
}
