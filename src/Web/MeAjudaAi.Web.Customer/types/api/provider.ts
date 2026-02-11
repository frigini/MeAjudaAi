export interface ServiceDto {
    serviceId: string;
    serviceName: string;
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
    userId: string;
    name: string;
    type: string;
    businessProfile: BusinessProfileDto;
    status: string;
    verificationStatus: VerificationStatus;
    services: ServiceDto[];
}
