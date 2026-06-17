using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs;

namespace MeAjudaAi.Modules.Providers.Application.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs internos para DTOs de contrato do módulo Providers.
/// </summary>
public static class ResponseMapperExtensions
{
    /// <summary>
    /// Mapeia ProviderDto (interno) para ModuleProviderDto (contrato).
    /// </summary>
    public static ModuleProviderDto ToContract(this ProviderDto provider)
    {
        var doc = provider.Documents?.FirstOrDefault(d => d.IsPrimary) ?? provider.Documents?.FirstOrDefault();

        return new ModuleProviderDto(
            Id: provider.Id,
            Name: provider.Name,
            Slug: provider.Slug,
            Email: provider.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
            Document: doc?.Number ?? string.Empty,
            ProviderType: provider.Type.ToString(),
            VerificationStatus: provider.VerificationStatus.ToString(),
            CreatedAt: provider.CreatedAt,
            UpdatedAt: provider.UpdatedAt ?? provider.CreatedAt,
            IsActive: provider.IsActive,
            Phone: provider.BusinessProfile?.ContactInfo?.PhoneNumber,
            DeviceToken: provider.DeviceToken);
    }

    /// <summary>
    /// Mapeia ProviderDto (interno) para ModuleProviderBasicDto (contrato básico).
    /// </summary>
    public static ModuleProviderBasicDto ToBasicContract(this ProviderDto provider)
    {
        return new ModuleProviderBasicDto(
            Id: provider.Id,
            Name: provider.Name,
            Slug: provider.Slug,
            Email: provider.BusinessProfile?.ContactInfo?.Email ?? string.Empty,
            ProviderType: provider.Type.ToString(),
            VerificationStatus: provider.VerificationStatus.ToString(),
            IsActive: provider.IsActive);
    }

    /// <summary>
    /// Mapeia uma coleção de ProviderDto para ModuleProviderBasicDto.
    /// </summary>
    public static IReadOnlyList<ModuleProviderBasicDto> ToBasicContract(this IEnumerable<ProviderDto> providers)
    {
        return providers.Select(ToBasicContract).ToList();
    }
}
