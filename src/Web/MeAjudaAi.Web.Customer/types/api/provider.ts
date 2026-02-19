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
    price: number;
    currency: string;
    serviceName?: string; // Flattened for display
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
    issuer: string;
    year: number;
    fileUrl?: string;
}

export enum EProviderType {
    None = 0,
    Individual = 1,
    Company = 2,
    Freelancer = 3,
    Cooperative = 4
}

export enum EProviderStatus {
    PendingBasicInfo = 0,
    PendingDocumentVerification = 1,
    Active = 2,
    Rejected = 3,
    Suspended = 4
}

export enum EVerificationStatus {
    Pending = 0,
    Verified = 1,
    Rejected = 2
}

export enum EProviderTier {
    Standard = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

export interface DocumentDto {
    type: string;
    url: string;
    status: string;
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
    phone?: string;
    avatarUrl?: string;
    averageRating?: number;
    reviewCount?: number;

    // Helper accessors for Location
    city?: string;
    state?: string;
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
    phoneNumber: string; // Required in backend
    email?: string;
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
    [EVerificationStatus.Pending]: "Pendente",
    [EVerificationStatus.Verified]: "Verificado",
    [EVerificationStatus.Rejected]: "Rejeitado",
};
