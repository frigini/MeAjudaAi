using MeAjudaAi.Modules.Payments.API.Endpoints;
using MeAjudaAi.Modules.Payments.Application;
using MeAjudaAi.Modules.Payments.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Payments.API;

public static class Extensions
{
    /// <summary>
    /// Registra os serviços e configurações do módulo de pagamentos no container de DI.
    /// </summary>
    /// <param name="services">Container de injeção de dependências.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="environment">Ambiente de execução.</param>
    /// <returns>O container atualizado.</returns>
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura e mapeia os endpoints do módulo de pagamentos no pipeline do ASP.NET.
    /// </summary>
    /// <param name="app">O roteador de endpoints.</param>
    /// <returns>O roteador atualizado.</returns>
    public static IEndpointRouteBuilder UsePaymentsModule(this IEndpointRouteBuilder app)
    {
        PaymentsEndpoints.Map(app);
        return app;
    }
}
