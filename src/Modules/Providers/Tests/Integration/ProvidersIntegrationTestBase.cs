using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

/// <summary>
/// Classe base para testes de integração específicos do módulo Providers.
/// </summary>
public abstract class ProvidersIntegrationTestBase : IntegrationTestBase
{
    /// <summary>
    /// Configurações padrão para testes do módulo Providers
    /// </summary>
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"MeAjudaAi", // Usar banco de desenvolvimento
                Username = "postgres",
                Password = "development123",
                Schema = "providers"
            },
            Cache = new TestCacheOptions
            {
                Enabled = false // Não usa cache por padrão
            },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };
    }

    /// <summary>
    /// Configura serviços específicos do módulo Providers
    /// </summary>
    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddProvidersTestInfrastructure(options);
    }

    /// <summary>
    /// Setup específico do módulo Providers (configurações adicionais se necessário)
    /// </summary>
    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        // Qualquer setup específico adicional do módulo Providers pode ser feito aqui
        // As migrações são aplicadas automaticamente pelo sistema de auto-descoberta
        await Task.CompletedTask;
    }

    /// <summary>
    /// Cria um provedor para teste e persiste no banco de dados
    /// </summary>
    protected async Task<Provider> CreateProviderAsync(
        Guid userId,
        string name,
        EProviderType type,
        BusinessProfile businessProfile,
        CancellationToken cancellationToken = default)
    {
        var provider = new Provider(userId, name, type, businessProfile);

        // Obter contexto
        var dbContext = GetService<ProvidersDbContext>();

        await dbContext.Providers.AddAsync(provider, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return provider;
    }

    /// <summary>
    /// Cria um perfil empresarial padrão para testes
    /// </summary>
    protected static BusinessProfile CreateTestBusinessProfile(string email = "test@example.com", string phone = "+5511999999999")
    {
        var contactInfo = new ContactInfo(email, phone);
        var address = new Address("Test Street", "123", "Test Neighborhood", "Test City", "Test State", "12345-678", "Brazil");
        
        return new BusinessProfile("Test Company Legal Name", contactInfo, address, "Test Company Fantasy", "Test company description");
    }

    /// <summary>
    /// Limpa dados das tabelas para isolamento entre testes
    /// </summary>
    protected async Task CleanupDatabase()
    {
        var dbContext = GetService<ProvidersDbContext>();
        
        try
        {
            // Ordem importante devido aos foreign keys
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM providers.\"Qualification\";");
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM providers.\"Document\";");  
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM providers.providers;");
        }
        catch (Exception)
        {
            // Se DELETE falhar, tentar TRUNCATE com cascata
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE providers.\"Qualification\", providers.\"Document\", providers.providers RESTART IDENTITY CASCADE;");
            }
            catch
            {
                // Se ainda falhar, recriar o schema
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
            }
        }
    }
}