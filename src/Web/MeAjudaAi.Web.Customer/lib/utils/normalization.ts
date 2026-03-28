import { EProviderType, EVerificationStatus } from "@/types/api/provider";

const providerTypeValues = new Set<number>(
    (Object.values(EProviderType) as number[]).filter((v) => typeof v === 'number')
);

const verificationStatusValues = new Set<number>(
    (Object.values(EVerificationStatus) as number[]).filter((v) => typeof v === 'number')
);

function isValidProviderType(val: number): val is EProviderType {
    return providerTypeValues.has(val);
}

function isValidVerificationStatus(val: number): val is EVerificationStatus {
    return verificationStatusValues.has(val);
}

/**
 * Normaliza valores de entrada para o enum EProviderType.
 */
export function normalizeProviderType(val: unknown): EProviderType {
    if (typeof val === 'number' && Number.isInteger(val)) {
        return isValidProviderType(val) ? val : EProviderType.None;
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
            if (isValidProviderType(num)) {
                return num;
            }
        }
    }
    return EProviderType.None;
}

/**
 * Normaliza valores de entrada para o enum EVerificationStatus.
 */
export function normalizeVerificationStatus(val: unknown): EVerificationStatus | undefined {
    if (typeof val === 'string') {
        if (/^\d+$/.test(val) || /^\+\d+$/.test(val)) {
            const num = parseInt(val.replace('+', ''), 10);
            if (Number.isFinite(num) && Number.isInteger(num) && isValidVerificationStatus(num)) {
                return num;
            }
            return undefined;
        }
        const lower = val.toLowerCase();
        if (lower === 'verified') return EVerificationStatus.Verified;
        if (lower === 'rejected') return EVerificationStatus.Rejected;
        if (lower === 'inprogress' || lower === 'in_progress') return EVerificationStatus.InProgress;
        if (lower === 'suspended') return EVerificationStatus.Suspended;
        if (lower === 'none') return EVerificationStatus.None;
        if (lower === 'pending') return EVerificationStatus.Pending;
        return undefined;
    }
    if (typeof val === 'number') {
        if (Number.isFinite(val) && Number.isInteger(val) && isValidVerificationStatus(val)) {
            return val;
        }
    }
    return undefined;
}
