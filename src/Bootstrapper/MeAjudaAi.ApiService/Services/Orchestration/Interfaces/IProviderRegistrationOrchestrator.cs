using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

namespace MeAjudaAi.ApiService.Services.Orchestration.Interfaces;

public interface IProviderRegistrationOrchestrator
{
    Task<Result<ProviderDto>> RegisterProviderAsync(
        RegisterProviderRequest request,
        CancellationToken cancellationToken);
}
