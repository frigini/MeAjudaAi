using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Shared.Seeding;

public static class SeedingExtensions
{
    /// <summary>
    /// Adiciona serviços de seeding de dados de desenvolvimento
    /// </summary>
    public static IServiceCollection AddDevelopmentSeeding(this IServiceCollection services)
    {
        services.AddScoped<IDevelopmentDataSeeder, DevelopmentDataSeeder>();
        return services;
    }

    /// <summary>
    /// Executa seed de dados se ambiente for Development e banco estiver vazio
    /// </summary>
    public static async Task SeedDevelopmentDataIfNeededAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (!environment.IsDevelopment())
        {
            return;
        }

        var seeder = scope.ServiceProvider.GetService<IDevelopmentDataSeeder>();
        if (seeder != null)
        {
            await seeder.ForceSeedAsync(cancellationToken);
        }
    }
}
