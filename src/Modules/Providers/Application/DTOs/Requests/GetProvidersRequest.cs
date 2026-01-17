using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

public record GetProvidersRequest
{
    public string? Name { get; init; }
    public int? Type { get; init; }
    public int? VerificationStatus { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
