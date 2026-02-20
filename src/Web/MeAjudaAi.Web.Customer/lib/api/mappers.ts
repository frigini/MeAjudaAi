import type {
    MeAjudaAiModulesSearchProvidersApplicationDtosSearchableProviderDto,
    MeAjudaAiModulesProvidersApplicationDtosProviderDto,
} from '@/lib/api/generated/types.gen';
import type { ProviderDto, BusinessProfileDto } from '@/types/api/provider';

// Mapeamento de ProviderType (backend enum) para frontend Enum
import {
    EProviderType,
    EProviderStatus,
    EVerificationStatus,
    EProviderTier,
    EDocumentType,
    EDocumentStatus
} from '@/types/api/provider';

// Mock de serviços para mapeamento visual
// TODO: Remover quando a API retornar os nomes dos serviços
const MOCK_SERVICES = [
    "Pedreiro", "Eletricista", "Encanador", "Pintor",
    "Jardineiro", "Montador de Móveis", "Faxina", "Marido de Aluguel"
];

function getMockServiceName(id: string): string {
    const hash = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return MOCK_SERVICES[hash % MOCK_SERVICES.length];
}

function getMockAvatarUrl(id?: string): string {
    const hash = id ? id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0) % 10 : 0;
    return `/images/providers/provider-${hash + 1}.svg`;
}

/**
 * Converte SearchableProviderDto (da API de busca) para ProviderDto (tipo da aplicação)
 */
export function mapSearchableProviderToProvider(
    dto: MeAjudaAiModulesSearchProvidersApplicationDtosSearchableProviderDto
): ProviderDto {
    return {
        id: dto.providerId ?? '',
        userId: '', // SearchableProvider doesn't expose UserID directly usually
        name: dto.name ?? '',
        email: '',
        avatarUrl: getMockAvatarUrl(dto.providerId),
        averageRating: dto.averageRating ?? 0,
        reviewCount: dto.totalReviews ?? 0,

        // Mapeando para ProviderServiceDto
        services: dto.serviceIds?.map(id => ({
            serviceId: id,
            price: 0, // Mock price
            currency: 'BRL',
            serviceName: getMockServiceName(id)
        })) ?? [],

        city: dto.city ?? '',
        state: dto.state ?? '',
        businessProfile: {
            legalName: dto.name ?? '',
            contactInfo: { email: '', phoneNumber: undefined }, // Typed to indicate partial structure
            primaryAddress: {
                street: '', number: '', neighborhood: '', city: dto.city ?? '', state: dto.state ?? '', zipCode: '', country: 'Brasil'
            }
        } as unknown as BusinessProfileDto, // Explicit unknown cast for partial mock

        type: EProviderType.Individual, // Default or map if available
        status: EProviderStatus.Active, // Default to Active so it shows in search results
        verificationStatus: EVerificationStatus.Pending, // Default safe
        tier: EProviderTier.Standard,
        documents: [],
        qualifications: [],
        createdAt: "" // Do not fabricate date for search results
    };
}

// Local type definition to extend generated types with missing runtime fields
// This is necessary because the generated SDK types are currently missing these fields
// that are returned by the API at runtime.
// TODO: Remove ExtendedProviderDto after SDK regeneration (see GitHub issue #42)
type ExtendedProviderDto = Omit<MeAjudaAiModulesProvidersApplicationDtosProviderDto, 'documents' | 'qualifications'> & {
    services?: {
        serviceId?: string;
        price?: number;
        currency?: string;
        serviceName?: string;
    }[];
    averageRating?: number;
    reviewCount?: number;
    rejectionReason?: string;
    suspensionReason?: string;
    tier?: EProviderTier | number;
    profilePictureUrl?: string;
    // Documents in ProviderDto seems to be using a different DTO in the spec than what is returned or expected here
    documents?: {
        id?: string;
        providerId?: string;
        documentType?: number;
        fileName?: string;
        fileUrl?: string;
        status?: number;
        uploadedAt?: string;
        verifiedAt?: string;
        rejectionReason?: string;
        ocrData?: string;
        number?: string;
        isPrimary?: boolean;
    }[];
    qualifications?: {
        name?: string;
        description?: string;
        issuingOrganization?: string;
        issueDate?: string;
        expirationDate?: string;
        documentNumber?: string;
    }[];
};

/**
 * Converte ProviderDto (da API de detalhes) para ProviderDto (tipo da aplicação)
 */
export function mapApiProviderToProvider(
    rawDto: MeAjudaAiModulesProvidersApplicationDtosProviderDto
): ProviderDto {
    // Cast to extended interface to access missing properties safely
    const dto = rawDto as unknown as ExtendedProviderDto;

    const businessProfile = dto.businessProfile as unknown as BusinessProfileDto;
    const contactInfo = businessProfile?.contactInfo;
    const address = businessProfile?.primaryAddress;

    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || dto.name || "Prestador";
    const services = dto.services || [];

    return {
        id: dto.id ?? '',
        userId: dto.userId ?? '',
        name: displayName,

        // Helper accessors
        email: contactInfo?.email ?? '',
        phone: contactInfo?.phoneNumber,
        avatarUrl: dto.profilePictureUrl ?? getMockAvatarUrl(dto.id),
        description: businessProfile?.description || '',

        averageRating: dto.averageRating ?? 0,
        reviewCount: dto.reviewCount ?? 0,
        rejectionReason: dto.rejectionReason,
        suspensionReason: dto.suspensionReason,

        // Map services
        services: services.map(s => ({
            serviceId: s.serviceId ?? '',
            price: s.price ?? 0,
            currency: s.currency ?? 'BRL',
            serviceName: s.serviceName ?? ''
        })),

        city: address?.city ?? '',
        state: address?.state ?? '',

        // Cast int to Enum (Backend enums map to numbers in JSON)
        type: (dto.type as unknown as EProviderType) ?? EProviderType.Individual,

        // Map full objects
        businessProfile: {
            legalName: businessProfile?.legalName ?? '',
            fantasyName: businessProfile?.fantasyName,
            description: businessProfile?.description,
            contactInfo: {
                email: contactInfo?.email ?? '',
                phoneNumber: contactInfo?.phoneNumber,
                website: contactInfo?.website
            },
            primaryAddress: {
                street: address?.street ?? '',
                number: address?.number ?? '',
                complement: address?.complement,
                neighborhood: address?.neighborhood ?? '',
                city: address?.city ?? '',
                state: address?.state ?? '',
                zipCode: address?.zipCode ?? '',
                country: address?.country ?? ''
            }
        },
        status: (dto.status as unknown as EProviderStatus) ?? EProviderStatus.PendingBasicInfo,
        verificationStatus: (dto.verificationStatus as unknown as EVerificationStatus) ?? EVerificationStatus.Pending,
        tier: (dto.tier as unknown as EProviderTier) ?? EProviderTier.Standard,
        documents: dto.documents?.map(d => ({
            id: d.id,
            providerId: d.providerId,
            documentType: (d.documentType as unknown as EDocumentType) ?? EDocumentType.Other,
            fileName: d.fileName ?? '',
            fileUrl: d.fileUrl ?? '',
            status: (d.status as unknown as EDocumentStatus) ?? EDocumentStatus.PendingVerification,
            uploadedAt: d.uploadedAt,
            verifiedAt: d.verifiedAt,
            rejectionReason: d.rejectionReason,
            ocrData: d.ocrData,
            number: d.number,
            isPrimary: d.isPrimary
        })) || [],
        qualifications: dto.qualifications?.map(q => ({
            name: q.name ?? '',
            description: q.description,
            issuingOrganization: q.issuingOrganization,
            issueDate: q.issueDate,
            expirationDate: q.expirationDate,
            documentNumber: q.documentNumber
        })) || [],

        createdAt: dto.createdAt ?? "",
        updatedAt: dto.updatedAt
    };
}
