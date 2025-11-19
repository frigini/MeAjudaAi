using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Catalogs.Tests.Infrastructure;

/// <summary>
/// Classe base para testes de integração específicos do módulo Catalogs.
/// </summary>
public abstract class CatalogsIntegrationTestBase : IntegrationTestBase
{
    /// <summary>
    /// Configurações padrão para testes do módulo Catalogs
    /// </summary>
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"test_db_{GetType().Name.ToUpperInvariant()}",
                Username = "test_user",
                Password = "test_password",
                Schema = "catalogs"
            },
            Cache = new TestCacheOptions
            {
                Enabled = true // Usa o Redis compartilhado
            },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };
    }

    /// <summary>
    /// Configura serviços específicos do módulo Catalogs
    /// </summary>
    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddCatalogsTestInfrastructure(options);
    }

    /// <summary>
    /// Setup específico do módulo Catalogs (configurações adicionais se necessário)
    /// </summary>
    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        // Qualquer setup específico adicional do módulo Catalogs pode ser feito aqui
        // As migrações são aplicadas automaticamente pelo sistema de auto-descoberta
        await Task.CompletedTask;
    }

    /// <summary>
    /// Cria uma categoria de serviço para teste e persiste no banco de dados
    /// </summary>
    protected async Task<ServiceCategory> CreateServiceCategoryAsync(
        string name,
        string? description = null,
        int displayOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var category = ServiceCategory.Create(name, description, displayOrder);

        var dbContext = GetService<CatalogsDbContext>();
        await dbContext.ServiceCategories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category;
    }

    /// <summary>
    /// Cria um serviço para teste e persiste no banco de dados
    /// </summary>
    protected async Task<Service> CreateServiceAsync(
        ServiceCategoryId categoryId,
        string name,
        string? description = null,
        int displayOrder = 0,
        CancellationToken cancellationToken = default)
    {
        var service = Service.Create(categoryId, name, description, displayOrder);

        var dbContext = GetService<CatalogsDbContext>();
        await dbContext.Services.AddAsync(service, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return service;
    }

    /// <summary>
    /// Cria uma categoria de serviço e um serviço associado
    /// </summary>
    protected async Task<(ServiceCategory Category, Service Service)> CreateCategoryWithServiceAsync(
        string categoryName,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var category = await CreateServiceCategoryAsync(categoryName, cancellationToken: cancellationToken);
        var service = await CreateServiceAsync(category.Id, serviceName, cancellationToken: cancellationToken);

        return (category, service);
    }
}
