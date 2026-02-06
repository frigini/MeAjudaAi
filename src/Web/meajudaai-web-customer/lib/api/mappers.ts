import type {
    MeAjudaAiModulesSearchProvidersApplicationDtosSearchableProviderDto,
    MeAjudaAiModulesProvidersApplicationDtosProviderDto,
} from '@/lib/api/generated';
import type { ProviderDto } from '@/types/api/provider';

/**
 * Converte SearchableProviderDto (da API de busca) para ProviderDto (tipo da aplicação)
 */
export function mapSearchableProviderToProvider(
    dto: MeAjudaAiModulesSearchProvidersApplicationDtosSearchableProviderDto
): ProviderDto {
    return {
        id: dto.providerId || '',
        name: dto.name || '',
        // SearchableProviderDto não expõe email por privacidade
        email: '',
        // SearchableProviderDto atual não retorna avatarUrl
        avatarUrl: null,
        averageRating: dto.averageRating || 0,
        // Mapeando totalReviews para reviewCount
        reviewCount: dto.totalReviews || 0,
        // SearchableProviderDto retorna apenas serviceIds, não os detalhes completos
        // TODO: Considerar buscar nomes dos serviços ou alterar a API para retornar nomes
        services: [],
        city: dto.city || '',
        state: dto.state || '',
        // Tipo de prestador não disponível na busca
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
        id: dto.id || '',
        name: displayName,
        // ProviderDto não tem email na raiz, apenas em businessProfile.contactInfo
        email: contactInfo?.email || '',
        // ProviderDto não tem profilePictureUrl na raiz
        avatarUrl: null,
        // ProviderDto não tem averageRating/reviewCount na raiz. Assumimos 0 por enquanto.
        averageRating: 0,
        reviewCount: 0,
        // ProviderDto não tem services na raiz. Retornando vazio.
        services: [],

        city: address?.city || '',
        state: address?.state || '',

        // Mapeamento do enum: 0=None, 1=Individual, 2=Company, 3=Cooperative, 4=Freelancer
        providerType: dto.type === 2 ? 'Company' : 'Individual',
    };
}
