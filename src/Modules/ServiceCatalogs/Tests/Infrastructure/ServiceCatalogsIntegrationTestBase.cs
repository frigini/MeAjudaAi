using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;

/// <summary>
/// Classe base para testes de integração específicos do módulo ServiceCatalogs.
/// </summary>
public abstract class ServiceCatalogsIntegrationTestBase : IntegrationTestBase
{
    /// <summary>
    /// Configurações padrão para testes do módulo ServiceCatalogs
    /// </summary>
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"test_db_{GetType().Name.ToUpperInvariant()[..Math.Min(50, GetType().Name.Length)]}",
                Username = "test_user",
                Password = "test_password",
                Schema = "ServiceCatalogs"
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
    /// Configura serviços específicos do módulo ServiceCatalogs
    /// </summary>
    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddServiceCatalogsTestInfrastructure(options);
    }

    /// <summary>
    /// Setup específico do módulo ServiceCatalogs (configurações adicionais se necessário)
    /// </summary>
    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        // Qualquer setup específico adicional do módulo ServiceCatalogs pode ser feito aqui
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
        // Adiciona Guid ao nome para garantir unicidade entre testes paralelos
        var uniqueName = $"{name}_{Guid.NewGuid():N}";
        var category = ServiceCategory.Create(uniqueName, description, displayOrder);

        var dbContext = GetService<ServiceCatalogsDbContext>();
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
        // Adiciona Guid ao nome para garantir unicidade entre testes paralelos
        var uniqueName = $"{name}_{Guid.NewGuid():N}";
        var service = Service.Create(categoryId, uniqueName, description, displayOrder);

        var dbContext = GetService<ServiceCatalogsDbContext>();
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
