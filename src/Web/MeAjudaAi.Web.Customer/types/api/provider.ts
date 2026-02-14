export interface ServiceCategoryDto {
    id: string;
    name: string;
    description?: string;
}

export interface ServiceDto {
    id?: string;
    name?: string;
    description?: string;
    category?: ServiceCategoryDto;

    // Legacy support if needed, or remove if unused
    serviceId?: string;
    serviceName?: string;
}

export interface ContactInfoDto {
    email: string;
    phoneNumber: string;
    website?: string;
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

export interface BusinessProfileDto {
    legalName: string;
    fantasyName?: string;
    description?: string;
    contactInfo: ContactInfoDto;
    primaryAddress: AddressDto;
}

export enum EVerificationStatus {
    Pending = 1,
    Verified = 2,
    Rejected = 3,
    Suspended = 4
}

export type VerificationStatus = "Pending" | "Verified" | "Rejected" | "Suspended";

export interface ProviderDto {
    id: string;
    name: string;
    email?: string;
    phone?: string;
    avatarUrl?: string; // UI friendly

    // Business data
    description?: string;
    averageRating?: number;
    reviewCount?: number;

    // Location
    city?: string;
    state?: string;

    // Classification
    providerType?: string; // 'Individual' | 'Company' | ...

    // Services
    services: ServiceDto[];

    // Detailed profile data (optional/nullable)
    userId?: string;
    businessProfile?: BusinessProfileDto;
    status?: string;
    verificationStatus?: VerificationStatus;
}
