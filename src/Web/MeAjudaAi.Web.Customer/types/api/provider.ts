export interface ServiceCategoryDto {
    id: string;
    name: string;
    description?: string;
}

export interface ServiceDto {
    id: string; // Guid
    name: string;
    description?: string;
    category?: ServiceCategoryDto;
    // Legacy mapping if needed
    serviceId?: string;
    serviceName?: string;
}

export interface ProviderServiceDto {
    serviceId: string;
    serviceName: string; // Flattened for display
}

export interface AddressDto {
    street: string;
    number: string;
    complement?: string | null;
    neighborhood: string;
    city: string;
    state: string;
    zipCode: string; // CEP
    country: string;
}

export interface ContactInfoDto {
    email: string;
    phoneNumber?: string | null;
    website?: string | null;
}

export interface BusinessProfileDto {
    legalName: string;
    fantasyName?: string | null;
    description?: string | null;
    contactInfo: ContactInfoDto;
    primaryAddress: AddressDto;
}

export interface QualificationDto {
    name: string;
    description?: string;
    issuingOrganization?: string;
    issueDate?: string | null;
    expirationDate?: string | null;
    documentNumber?: string;
}

export const EProviderType = {
    None: 0,
    Individual: 1,
    Company: 2,
    Cooperative: 3,
    Freelancer: 4
} as const;
export type EProviderType = typeof EProviderType[keyof typeof EProviderType];

export const EProviderStatus = {
    PendingBasicInfo: 1,
    PendingDocumentVerification: 2,
    Active: 3,
    Suspended: 4,
    Rejected: 5
} as const;
export type EProviderStatus = typeof EProviderStatus[keyof typeof EProviderStatus];

export const EVerificationStatus = {
    None: 0,
    Pending: 1,
    InProgress: 2,
    Verified: 3,
    Rejected: 4,
    Suspended: 5
} as const;
export type EVerificationStatus = typeof EVerificationStatus[keyof typeof EVerificationStatus];

export const EProviderTier = {
    Standard: 0,
    Silver: 1,
    Gold: 2,
    Platinum: 3
} as const;
export type EProviderTier = typeof EProviderTier[keyof typeof EProviderTier];

export const EDocumentType = {
    None: 0,
    CPF: 1,
    CNPJ: 2,
    RG: 3,
    CNH: 4,
    Passport: 5,
    Other: 99
} as const;
export type EDocumentType = typeof EDocumentType[keyof typeof EDocumentType];

export const EDocumentStatus = {
    Uploaded: 1,
    PendingVerification: 2,
    Verified: 3,
    Rejected: 4,
    Failed: 5
} as const;
export type EDocumentStatus = typeof EDocumentStatus[keyof typeof EDocumentStatus];

export interface DocumentDto {
    id?: string; // Guid
    providerId?: string; // Guid
    documentType: EDocumentType;
    fileName: string;
    fileUrl: string;
    status?: EDocumentStatus;
    uploadedAt?: string; // DateTime
    verifiedAt?: string | null;
    rejectionReason?: string | null;
    ocrData?: string | null;
    number?: string;
    isPrimary?: boolean;
}

export interface ProviderDto {
    id: string; // Guid
    userId: string; // Guid
    name: string; // This is the registered name (User's name usually)
    type: EProviderType;
    businessProfile: BusinessProfileDto;
    status: EProviderStatus;
    verificationStatus: EVerificationStatus;
    tier: EProviderTier;

    // Collections
    documents: DocumentDto[];
    qualifications: QualificationDto[];
    services: ProviderServiceDto[];

    createdAt: string; // DateTime
    updatedAt?: string | null;

    // UI Helpers (optional, based on mapping)
    email?: string;
    phone?: string | null;
    avatarUrl?: string | null;
    description?: string | null;
    averageRating?: number;
    reviewCount?: number;
    rejectionReason?: string | null;
    suspensionReason?: string | null;

    // Helper accessors for Location
    city?: string;
    state?: string;
}

export interface ProviderStatusDto {
    status: EProviderStatus;
    tier: EProviderTier;
    verificationStatus: EVerificationStatus;
    rejectionReason?: string | null;
    suspensionReason?: string | null;
}

export interface RegisterProviderRequest {
    name: string;
    type: EProviderType;
    documentNumber: string;
    phoneNumber: string; // Required in backend
    email: string; // Required in backend
    acceptedTerms: boolean;
    acceptedPrivacyPolicy: boolean;
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
    [EProviderStatus.PendingDocumentVerification]: "Em Análise",
    [EProviderStatus.Active]: "Ativo",
    [EProviderStatus.Rejected]: "Rejeitado",
    [EProviderStatus.Suspended]: "Suspenso",
};

export const PROVIDER_TIER_LABELS: Record<EProviderTier, string> = {
    [EProviderTier.Standard]: "Standard",
    [EProviderTier.Silver]: "Prata", // Fixed translation to match UI expectation
    [EProviderTier.Gold]: "Ouro",
    [EProviderTier.Platinum]: "Platina",
};

export const VERIFICATION_STATUS_LABELS: Record<EVerificationStatus, string> = {
    [EVerificationStatus.None]: "Não Iniciado",
    [EVerificationStatus.Pending]: "Pendente",
    [EVerificationStatus.InProgress]: "Em Análise",
    [EVerificationStatus.Verified]: "Verificado",
    [EVerificationStatus.Rejected]: "Rejeitado",
    [EVerificationStatus.Suspended]: "Suspenso",
};
