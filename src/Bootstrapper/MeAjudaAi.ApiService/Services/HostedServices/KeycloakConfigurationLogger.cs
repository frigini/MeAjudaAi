using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.ApiService.Services.HostedServices;

/// <summary>
/// Hosted service para logar a configuração do Keycloak durante a inicialização da aplicação
/// </summary>
public sealed class KeycloakConfigurationLogger(
    IOptions<KeycloakOptions> keycloakOptions,
    ILogger<KeycloakConfigurationLogger> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = keycloakOptions.Value;

        // Loga a configuração efetiva do Keycloak (sem segredos)
        logger.LogInformation("Keycloak authentication configured - Authority: {Authority}, ClientId: {ClientId}, ValidateIssuer: {ValidateIssuer}",
            options.AuthorityUrl, options.ClientId, options.ValidateIssuer);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
