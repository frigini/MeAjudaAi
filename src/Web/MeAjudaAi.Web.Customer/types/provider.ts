export enum EProviderType {
    None = "None",
    Individual = "Individual",
    Company = "Company",
    Cooperative = "Cooperative",
    Freelancer = "Freelancer"
}

export enum EProviderStatus {
    PendingBasicInfo = "PendingBasicInfo",
    PendingDocumentVerification = "PendingDocumentVerification",
    Active = "Active",
    Suspended = "Suspended",
    Rejected = "Rejected"
}

export enum EProviderTier {
    Standard = "Standard",
    Silver = "Silver",
    Gold = "Gold",
    Platinum = "Platinum"
}

export enum EVerificationStatus {
    Pending = "Pending",
    InProgress = "InProgress",
    Verified = "Verified",
    Rejected = "Rejected",
    Suspended = "Suspended"
}

export const PROVIDER_TYPE_LABELS: Record<EProviderType, string> = {
    [EProviderType.None]: "Nenhum",
    [EProviderType.Individual]: "Pessoa Física",
    [EProviderType.Company]: "Pessoa Jurídica",
    [EProviderType.Cooperative]: "Cooperativa",
    [EProviderType.Freelancer]: "Autônomo"
};

export const PROVIDER_STATUS_LABELS: Record<EProviderStatus, string> = {
    [EProviderStatus.PendingBasicInfo]: "Pendente (Dados Básicos)",
    [EProviderStatus.PendingDocumentVerification]: "Aguardando Documentos",
    [EProviderStatus.Active]: "Ativo",
    [EProviderStatus.Suspended]: "Suspenso",
    [EProviderStatus.Rejected]: "Rejeitado"
};

export const PROVIDER_TIER_LABELS: Record<EProviderTier, string> = {
    [EProviderTier.Standard]: "Básico",
    [EProviderTier.Silver]: "Prata",
    [EProviderTier.Gold]: "Ouro",
    [EProviderTier.Platinum]: "Platina"
};

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
