using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            // Use EF Core change tracking for safer cleanup
            // Only providers table is exposed as DbSet, child entities are accessed through navigation properties
            var providers = await dbContext.Providers
                .Include(p => p.Documents)
                .Include(p => p.Qualifications)
                .ToListAsync();

            if (providers.Any())
            {
                dbContext.Providers.RemoveRange(providers);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EF Core cleanup failed: {ex.Message}. Trying raw SQL...");
            
            // Fallback to raw SQL if EF Core fails
            try
            {
                // Use correct table names (lowercase without quotes for most cases)
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM providers.qualification;");
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM providers.document;");
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM providers.providers;");
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Raw SQL cleanup failed: {ex2.Message}. Trying TRUNCATE...");
                
                // Se DELETE falhar, tentar TRUNCATE com cascata
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE providers.qualification, providers.document, providers.providers RESTART IDENTITY CASCADE;");
                }
                catch (Exception ex3)
                {
                    Console.WriteLine($"TRUNCATE failed: {ex3.Message}. Recreating database...");
                    
                    // Se ainda falhar, recriar o schema
                    await dbContext.Database.EnsureDeletedAsync();
                    await dbContext.Database.EnsureCreatedAsync();
                }
            }
        }
    }
}
