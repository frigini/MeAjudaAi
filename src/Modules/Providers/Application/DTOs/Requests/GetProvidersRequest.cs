using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

public record GetProvidersRequest : PagedRequest
{
    public string? Name { get; init; }
    public int? Type { get; init; }
    public int? VerificationStatus { get; init; }
}
