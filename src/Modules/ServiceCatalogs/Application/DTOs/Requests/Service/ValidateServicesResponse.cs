namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

public sealed record ValidateServicesResponse(
    bool AllValid,
    IReadOnlyCollection<Guid> InvalidServiceIds,
    IReadOnlyCollection<Guid> InactiveServiceIds);
