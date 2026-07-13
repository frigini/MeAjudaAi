using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Tests.Integration.Infrastructure;

/// <summary>
/// Classe base para testes de integração específicos do módulo Providers.
/// Herda de BaseIntegrationTest para usar containers compartilhados.
/// </summary>
[Trait("Category", "Integration")]
public abstract class ProvidersIntegrationTestBase : BaseIntegrationTest
{
    private ProvidersDbContext? _dbContext;

    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                Schema = "providers",
            },
            Cache = new TestCacheOptions
            {
                Enabled = false
            },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddProvidersTestInfrastructure(options);
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
        var provider = new ProviderBuilder()
            .WithUserId(userId)
            .WithName(name)
            .WithType(type)
            .WithBusinessProfile(businessProfile)
            .Build();
        var dbContext = DbContext;
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
    /// Acesso direto ao contexto de banco de dados do módulo Providers
    /// </summary>
    protected ProvidersDbContext DbContext => _dbContext ??= GetService<ProvidersDbContext>();
}
