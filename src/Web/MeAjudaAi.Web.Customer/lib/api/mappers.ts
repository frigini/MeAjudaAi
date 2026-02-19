import type {
    MeAjudaAiModulesSearchProvidersApplicationDtosSearchableProviderDto,
    MeAjudaAiModulesProvidersApplicationDtosProviderDto,
} from '@/lib/api/generated/types.gen';
import type { ProviderDto } from '@/types/api/provider';

// Mapeamento de ProviderType (backend enum) para frontend Enum
import { EProviderType, EProviderStatus, EVerificationStatus, EProviderTier, ProviderServiceDto } from '@/types/api/provider';

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
            contactInfo: { email: '' },
            primaryAddress: {
                street: '', number: '', neighborhood: '', city: dto.city ?? '', state: dto.state ?? '', zipCode: '', country: 'Brasil'
            }
        } as any, // Partial mock since Search doesn't return full profile

        type: EProviderType.Individual, // Default or map if available
        status: EProviderStatus.PendingBasicInfo, // Default safe
        verificationStatus: EVerificationStatus.Pending, // Default safe
        tier: EProviderTier.Standard,
        documents: [],
        qualifications: [],
        createdAt: "" // Do not fabricate date for search results
    };
}

// Interface definitions to match actual API response structure to avoid generation sync issues
interface LocalBusinessProfileDto {
    legalName: string;
    fantasyName?: string;
    description?: string;
    contactInfo?: {
        email: string;
        phoneNumber: string;
    };
    primaryAddress?: {
        street: string;
        number: string;
        complement?: string;
        neighborhood: string;
        city: string;
        state: string;
        zipCode: string;
        country: string;
    };
}

interface LocalProviderDto extends MeAjudaAiModulesProvidersApplicationDtosProviderDto {
    businessProfile?: LocalBusinessProfileDto;
    services?: Array<{
        serviceId: string;
        price: number;
        currency: string;
        serviceName: string;
    }>;
    averageRating?: number;
    reviewCount?: number;
    tier?: EProviderTier;
    createdAt?: string;
}

/**
 * Converte ProviderDto (da API de detalhes) para ProviderDto (tipo da aplicação)
 */
export function mapApiProviderToProvider(
    rawDto: MeAjudaAiModulesProvidersApplicationDtosProviderDto
): ProviderDto {
    // Cast to local interface to access missing properties safely
    const dto = rawDto as unknown as LocalProviderDto;

    const businessProfile = dto.businessProfile;
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
        phone: contactInfo?.phoneNumber ?? undefined,
        avatarUrl: getMockAvatarUrl(dto.id),
        description: businessProfile?.description || '',

        averageRating: dto.averageRating ?? 0,
        reviewCount: dto.reviewCount ?? 0,

        // Map services
        services: services.map(s => ({
            serviceId: s.serviceId,
            price: s.price,
            currency: s.currency,
            serviceName: s.serviceName
        })),

        city: address?.city ?? '',
        state: address?.state ?? '',

        // Cast int to Enum
        type: (dto.type as unknown as EProviderType) ?? EProviderType.Individual,

        // Map full objects
        businessProfile: businessProfile as any, // Keep as any if strict match fails downstream, or map to app type
        status: (dto.status as unknown as EProviderStatus) ?? EProviderStatus.PendingBasicInfo,
        verificationStatus: (dto.verificationStatus as unknown as EVerificationStatus) ?? EVerificationStatus.Pending,
        tier: dto.tier ?? EProviderTier.Standard,
        documents: dto.documents?.map((d: any) => ({
            id: d.id,
            providerId: d.providerId,
            documentType: d.documentType,
            fileName: d.fileName,
            fileUrl: d.fileUrl,
            status: d.status,
            uploadedAt: d.uploadedAt,
            verifiedAt: d.verifiedAt,
            rejectionReason: d.rejectionReason,
            ocrData: d.ocrData
        })) || [],
        qualifications: dto.qualifications?.map((q: any) => ({
            name: q.name,
            issuer: q.issuer,
            year: q.year,
            fileUrl: q.fileUrl
        })) || [],

        createdAt: dto.createdAt ?? "", // Only use if present, otherwise empty string (or change type to optional)
        updatedAt: dto.updatedAt
    };
}
