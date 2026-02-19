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
        status: EProviderStatus.Active, // Active
        verificationStatus: EVerificationStatus.Verified, // Verified (mock)
        tier: EProviderTier.Standard,
        documents: [],
        qualifications: [],
        createdAt: new Date().toISOString()
    };
}

/**
 * Converte ProviderDto (da API de detalhes) para ProviderDto (tipo da aplicação)
 */
export function mapApiProviderToProvider(
    dto: MeAjudaAiModulesProvidersApplicationDtosProviderDto
): ProviderDto {
    const businessProfile = dto.businessProfile;
    // @ts-ignore
    const contactInfo = businessProfile?.contactInfo;
    // @ts-ignore
    const address = businessProfile?.primaryAddress;

    // @ts-ignore
    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || dto.name || "Prestador";
    // @ts-ignore
    const services = dto.services || [];

    return {
        id: dto.id ?? '',
        userId: dto.userId ?? '',
        name: displayName,

        // Helper accessors
        // @ts-ignore
        email: contactInfo?.email ?? '',
        // @ts-ignore
        phone: contactInfo?.phoneNumber ?? undefined,
        avatarUrl: getMockAvatarUrl(dto.id),
        // @ts-ignore
        description: businessProfile?.description || '',

        // @ts-ignore
        averageRating: dto.averageRating ?? 0,
        // @ts-ignore
        reviewCount: dto.reviewCount ?? 0,

        // Map services if they exist in DTO, otherwise empty
        // @ts-ignore
        services: services.map((s: any) => ({
            serviceId: s.serviceId,
            price: s.price,
            currency: s.currency,
            serviceName: s.serviceName
        })),

        // @ts-ignore
        city: address?.city ?? '',
        // @ts-ignore
        state: address?.state ?? '',

        // Cast int to Enum
        type: (dto.type as unknown as EProviderType) ?? EProviderType.Individual,

        // Map full objects
        // @ts-ignore
        businessProfile: businessProfile,
        // @ts-ignore
        status: (dto.status as unknown as EProviderStatus) ?? EProviderStatus.PendingBasicInfo,
        // @ts-ignore
        verificationStatus: (dto.verificationStatus as unknown as EVerificationStatus) ?? EVerificationStatus.Pending,
        // @ts-ignore
        tier: (dto.tier as unknown as EProviderTier) ?? EProviderTier.Standard,
        // @ts-ignore
        documents: dto.documents || [],
        // @ts-ignore
        qualifications: dto.qualifications || [],

        createdAt: dto.createdAt ?? new Date().toISOString(),
        updatedAt: dto.updatedAt
    };
}
