import type {
    MeAjudaAiModulesSearchProvidersApplicationDtosSearchableProviderDto,
    MeAjudaAiModulesProvidersApplicationDtosProviderDto,
} from '@/lib/api/generated/types.gen';
import type { ProviderDto } from '@/types/api/provider';

// Mapeamento de ProviderType (backend enum) para frontend string
// 0=None, 1=Individual, 2=Company, 3=Cooperative, 4=Freelancer
// NOTE: Alinhado com o enum ProviderType em MeAjudaAi.Core.Shared.Contracts
const PROVIDER_TYPE_MAP: Record<number, ProviderDto['providerType']> = {
    0: 'None',
    1: 'Individual',
    2: 'Company',
    3: 'Cooperative',
    4: 'Freelancer',
};

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
        name: dto.name ?? '',
        // SearchableProviderDto não expõe email por privacidade
        email: '',
        // SearchableProviderDto atual não retorna avatarUrl
        // Mock visual para testes: atribui uma imagem estática (1-10) baseada no hash do ID
        avatarUrl: getMockAvatarUrl(dto.providerId),
        averageRating: dto.averageRating ?? 0,
        // Mapeando totalReviews para reviewCount
        reviewCount: dto.totalReviews ?? 0,
        // SearchableProviderDto retorna apenas serviceIds
        // Mapeando para nomes mockados baseados no ID para visualização
        services: dto.serviceIds?.map(id => ({
            id: id,
            name: getMockServiceName(id),
            description: '',
            category: { id: '', name: 'Geral', description: '' }
        })) ?? [],
        city: dto.city ?? '',
        state: dto.state ?? '',
        description: dto.description ?? undefined,
        // TODO: Mapear providerType quando SearchableProviderDto incluir essa informação
        providerType: 'Individual',
    };
}

/**
 * Converte ProviderDto (da API de detalhes) para ProviderDto (tipo da aplicação)
 */
export function mapApiProviderToProvider(
    dto: MeAjudaAiModulesProvidersApplicationDtosProviderDto
): ProviderDto {
    const businessProfile = dto.businessProfile;
    const contactInfo = businessProfile?.contactInfo;
    const address = businessProfile?.primaryAddress;

    // Nome de exibição: fantasia, razão social ou nome do usuário
    const displayName = businessProfile?.fantasyName || businessProfile?.legalName || dto.name || "Prestador";

    return {
        id: dto.id ?? '',
        name: displayName,
        // ProviderDto não tem email na raiz, apenas em businessProfile.contactInfo
        email: contactInfo?.email ?? '',
        phone: contactInfo?.phoneNumber ?? undefined,
        // ProviderDto não tem profilePictureUrl na raiz
        // Mock visual para testes: atribui uma imagem estática (1-10) baseada no hash do ID
        avatarUrl: getMockAvatarUrl(dto.id),
        // ProviderDto não tem averageRating/reviewCount na raiz. Assumimos 0 por enquanto.
        // TODO: Enriquecer com dados reais quando API retornar rating e reviews no detalhe
        averageRating: 0,
        reviewCount: 0,
        // ProviderDto não tem services na raiz. Retornando vazio.
        // TODO: Enriquecer com dados reais quando API retornar services no detalhe
        services: [],

        city: address?.city ?? '',
        state: address?.state ?? '',

        providerType: PROVIDER_TYPE_MAP[dto.type ?? 0] ?? 'Individual',
    };
}
