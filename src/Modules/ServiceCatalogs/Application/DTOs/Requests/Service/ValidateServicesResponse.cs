namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

/// <summary>
/// Resposta da validação de serviços contendo os IDs inválidos e inativos.
/// </summary>
/// <param name="AllValid">Indica se todos os serviços são válidos.</param>
/// <param name="InvalidServiceIds">IDs dos serviços que não existem.</param>
/// <param name="InactiveServiceIds">IDs dos serviços que estão inativos.</param>
public sealed record ValidateServicesResponse(
    bool AllValid,
    IReadOnlyCollection<Guid> InvalidServiceIds,
    IReadOnlyCollection<Guid> InactiveServiceIds);
