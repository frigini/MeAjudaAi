using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.ApiService.Services.Orchestration.Interfaces;

public interface IClientConfigurationService
{
    ClientConfiguration GetClientConfiguration();
}
