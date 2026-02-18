export enum EProviderType {
    Individual = "Individual",
    Company = "Company"
}

export enum EProviderStatus {
    PendingBasicInfo = "PendingBasicInfo",
    PendingDocuments = "PendingDocuments",
    PendingVerification = "PendingVerification",
    Active = "Active",
    Suspended = "Suspended",
    Rejected = "Rejected",
    Deleted = "Deleted"
}

export enum EProviderTier {
    Standard = "Standard",
    Silver = "Silver",
    Gold = "Gold",
    Platinum = "Platinum"
}

export enum EVerificationStatus {
    Pending = "Pending",
    UnderReview = "UnderReview",
    Verified = "Verified",
    Rejected = "Rejected",
    Revoked = "Revoked"
}

export interface ProviderStatusDto {
    status: EProviderStatus;
    tier: EProviderTier;
    verificationStatus: EVerificationStatus;
    rejectionReason?: string | null;
}

export interface RegisterProviderRequest {
    name: string;
    type: EProviderType;
    documentNumber: string;
    phoneNumber?: string;
    email?: string;
}

export interface ProviderDto {
    id: string;
    userId: string;
    name: string;
    type: EProviderType;
    status: EProviderStatus;
    tier: EProviderTier;
    verificationStatus: EVerificationStatus;
    // outros campos omitidos por enquanto
}
