namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;

public sealed record ValidateServicesResponse
{
    public bool AllValid { get; init; }
    public IReadOnlyCollection<Guid> InvalidServiceIds { get; init; }
    public IReadOnlyCollection<Guid> InactiveServiceIds { get; init; }

    public ValidateServicesResponse(bool allValid, IReadOnlyCollection<Guid>? invalidServiceIds, IReadOnlyCollection<Guid>? inactiveServiceIds)
    {
        AllValid = allValid;
        InvalidServiceIds = invalidServiceIds ?? Array.Empty<Guid>();
        InactiveServiceIds = inactiveServiceIds ?? Array.Empty<Guid>();
    }
}
