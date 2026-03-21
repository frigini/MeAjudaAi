export enum EProviderType {
    None = -1,
    Individual = 0,
    LegalEntity = 1,
    Company = 2,
    Cooperative = 3,
    Freelancer = 4
}

export enum EProviderStatus {
    PendingBasicInfo = 'PendingBasicInfo',
    PendingDocumentVerification = 'PendingDocumentVerification',
    Active = 'Active',
    Suspended = 'Suspended',
    Rejected = 'Rejected'
}

export enum EProviderTier {
    Standard = 'Standard',
    Premium = 'Premium',
    Enterprise = 'Enterprise'
}

export enum EDocumentType {
    CNH = 'CNH',
    CPF = 'CPF',
    RG = 'RG',
    CNPJ = 'CNPJ',
    Certificate = 'Certificate',
    Other = 'Other'
}

export enum EDocumentStatus {
    Pending = 'Pending',
    PendingVerification = 'PendingVerification',
    Approved = 'Approved',
    Rejected = 'Rejected'
}

export enum EVerificationStatus {
    None = 'None',
    Pending = 'Pending',
    UnderReview = 'UnderReview',
    InProgress = 'InProgress',
    Verified = 'Verified',
    Approved = 'Approved',
    Rejected = 'Rejected',
    Suspended = 'Suspended'
}

export interface AddressDto {
    street: string;
    number: string;
    complement?: string;
    neighborhood: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
}

export interface ContactInfoDto {
    email: string;
    phoneNumber?: string;
    website?: string;
    additionalPhoneNumbers: string[];
}

export interface BusinessProfileDto {
    legalName: string;
    fantasyName?: string;
    description?: string;
    contactInfo: ContactInfoDto;
    primaryAddress?: AddressDto;
    showAddressToClient: boolean;
}

export interface ProviderDto {
    id: string;
    userId: string;
    name: string;
    slug?: string;
    email?: string;
    phone?: string;
    avatarUrl?: string;
    averageRating?: number;
    reviewCount?: number;
    city?: string;
    state?: string;
    description?: string;
    rejectionReason?: string;
    suspensionReason?: string;
    type: EProviderType;
    status: EProviderStatus;
    verificationStatus: EVerificationStatus;
    tier: EProviderTier;
    businessProfile?: BusinessProfileDto;
    documents?: Array<{
        id?: string;
        providerId?: string;
        number?: string;
        documentType: EDocumentType | number;
        fileName?: string;
        fileUrl?: string;
        status?: EDocumentStatus | number;
        uploadedAt?: string;
        verifiedAt?: string;
        rejectionReason?: string;
        ocrData?: string;
        isPrimary?: boolean;
    }>;
    qualifications?: Array<{
        name: string;
        description?: string;
        issuingOrganization?: string;
        issueDate?: string;
        expirationDate?: string;
        documentNumber?: string;
    }>;
    services?: Array<{
        serviceId: string;
        price?: number;
        currency?: string;
        serviceName: string;
    }>;
    createdAt: string;
    updatedAt?: string;
    isDeleted?: boolean;
    deletedAt?: string;
    isActive?: boolean;
}
