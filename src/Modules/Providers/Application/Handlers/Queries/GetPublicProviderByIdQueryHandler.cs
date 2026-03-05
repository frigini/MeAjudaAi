using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.FeatureManagement;
using System.Collections.Generic;
using System.Linq;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar a query de busca de prestador público.
/// </summary>
public sealed class GetPublicProviderByIdQueryHandler : IQueryHandler<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IFeatureManager _featureManager;

    public GetPublicProviderByIdQueryHandler(IProviderRepository providerRepository, IFeatureManager featureManager)
    {
        _providerRepository = providerRepository;
        _featureManager = featureManager;
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
        if (provider.Status != EProviderStatus.Active)
        {
             return Result<PublicProviderDto?>.Failure(Error.NotFound("Prestador não encontrado."));
        }

        var businessProfile = provider.BusinessProfile;
        
        // Verifica se a privacidade restrita está habilitada via feature flag
        var isPrivacyEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.PublicProfilePrivacy);

        // A privacidade é forçada se a feature flag estiver ligada OU se o usuário não estiver autenticado
        var shouldRedactContactInfo = isPrivacyEnabled || !query.IsAuthenticated;

        var phoneNumbers = ResolvePhoneNumbers(shouldRedactContactInfo, businessProfile);
        
        var email = !shouldRedactContactInfo && businessProfile.ContactInfo != null
            ? businessProfile.ContactInfo.Email
            : null;
            
        var services = !isPrivacyEnabled
            ? provider.Services.Select(s => s.ServiceName).ToList()
            : new List<string>();

        // Mapeamento para DTO seguro com valores default (reais virão de integração futura)
        var dto = new PublicProviderDto(
            provider.Id,
            provider.Name,
            provider.Type,
            businessProfile.FantasyName,
            businessProfile.Description,
            businessProfile.PrimaryAddress?.City,
            businessProfile.PrimaryAddress?.State,
            provider.CreatedAt,
            
            // Dados enriquecidos: Usando safe defaults (null/0) conforme solicitado
            Rating: null, 
            ReviewCount: 0, 
            
            // Dados sensíveis ou dependentes de módulos externos são condicionados por privacidade
            Services: services,
            PhoneNumbers: phoneNumbers,
            Email: email,
            VerificationStatus: provider.VerificationStatus
        );

        return Result<PublicProviderDto?>.Success(dto);
    }

    private static IEnumerable<string> ResolvePhoneNumbers(bool isPrivacyEnabled, BusinessProfile profile)
    {
        if (isPrivacyEnabled || profile.ContactInfo is null)
            return Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(profile.ContactInfo.PhoneNumber))
            return profile.ContactInfo.AdditionalPhoneNumbers;

        return new[] { profile.ContactInfo.PhoneNumber }
            .Concat(profile.ContactInfo.AdditionalPhoneNumbers);
    }
}
