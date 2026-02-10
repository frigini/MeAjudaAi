using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar a query de busca de prestador público.
/// </summary>
public sealed class GetPublicProviderByIdQueryHandler : IQueryHandler<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>
{
    private readonly IProviderRepository _providerRepository;

    public GetPublicProviderByIdQueryHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<Result<PublicProviderDto?>> HandleAsync(
        GetPublicProviderByIdQuery query, 
        CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByIdAsync(query.Id, cancellationToken);

        if (provider is null)
        {
            return Result<PublicProviderDto?>.Failure(Error.NotFound("Prestador não encontrado."));
        }

        // Validação adicional: Apenas prestadores ativos devem ser consultados publicamente
        // Se estiver suspenso ou rejeitado, retornamos NotFound por segurança/privacidade
        if (provider.Status != MeAjudaAi.Modules.Providers.Domain.Enums.EProviderStatus.Active)
        {
             return Result<PublicProviderDto?>.Failure(Error.NotFound("Prestador não encontrado."));
        }

        // TODO: Em um cenário real, buscaríamos avaliações e serviços de seus respectivos módulos/repositórios
        // Por enquanto, seguimos o padrão de retornar dados mockados/placeholders para satisfazer o contrato de UI
        // até que os módulos de Avaliação e Serviço estejam integrados.

        var businessProfile = provider.BusinessProfile;
        
        // Mapeamento para DTO seguro
        var dto = new PublicProviderDto(
            provider.Id,
            provider.Name,
            provider.Type,
            businessProfile.FantasyName,
            businessProfile.Description,
            businessProfile.PrimaryAddress?.City,
            businessProfile.PrimaryAddress?.State,
            provider.CreatedAt,
            
            // Dados enriquecidos (Mockados por enquanto)
            Rating: 4.8, // Valor default para UI
            ReviewCount: 12, // Valor default para UI
            Services: new[] { "Serviço Geral" }, // Placeholder
            PhoneNumbers: businessProfile.ContactInfo?.PhoneNumber != null 
                ? new[] { businessProfile.ContactInfo.PhoneNumber } 
                : Array.Empty<string>()
        );

        return Result<PublicProviderDto?>.Success(dto);
    }
}
