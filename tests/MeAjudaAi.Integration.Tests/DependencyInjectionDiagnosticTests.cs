using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE DIAGNÓSTICO PARA DEPENDENCY INJECTION
/// 
/// Verifica se todos os serviços necessários estão registrados corretamente
/// </summary>
public class DependencyInjectionDiagnosticTests(ITestOutputHelper testOutput) : ApiTestBase
{
    [Fact]
    public void Should_Have_QueryDispatcher_Registered()
    {
        // Arrange & Act
        var queryDispatcher = Services.GetService<IQueryDispatcher>();

        // Assert
        testOutput.WriteLine($"QueryDispatcher registration: {queryDispatcher != null}");
        queryDispatcher.Should().NotBeNull("IQueryDispatcher should be registered");
    }

    [Fact]
    public void Should_Have_GetProvidersQueryHandler_Registered()
    {
        // Arrange & Act
        var handler = Services.GetService<IQueryHandler<GetProvidersQuery, Result<PagedResult<ProviderDto>>>>();

        // Assert
        testOutput.WriteLine($"GetProvidersQueryHandler registration: {handler != null}");
        handler.Should().NotBeNull("GetProvidersQueryHandler should be registered");
    }

    [Fact]
    public void Should_Have_All_Critical_Services_Registered()
    {
        // Arrange
        var criticalServices = new[]
        {
            typeof(IQueryDispatcher),
            typeof(IQueryHandler<GetProvidersQuery, Result<PagedResult<ProviderDto>>>),
            typeof(MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext),
            typeof(MeAjudaAi.Modules.Providers.Domain.Repositories.IProviderRepository)
        };

        // Act & Assert
        foreach (var serviceType in criticalServices)
        {
            var service = Services.GetService(serviceType);
            testOutput.WriteLine($"{serviceType.Name}: {service != null}");
            service.Should().NotBeNull($"{serviceType.Name} should be registered");
        }
    }

    [Fact]
    public void Should_List_All_Registered_QueryHandlers()
    {
        // Arrange
        var serviceProvider = Services;

        // Act - Get all IQueryHandler registrations
        var queryHandlerType = typeof(IQueryHandler<,>);
        var registeredServices = serviceProvider.GetServices<object>();

        // Get all services that implement any IQueryHandler interface
        var allServices = Services.GetType()
            .GetProperty("Services")?.GetValue(Services) as IEnumerable<ServiceDescriptor>;

        if (allServices != null)
        {
            var queryHandlers = allServices
                .Where(s => s.ServiceType.IsGenericType &&
                           s.ServiceType.GetGenericTypeDefinition() == queryHandlerType)
                .ToList();

            testOutput.WriteLine($"Registered QueryHandlers count: {queryHandlers.Count}");

            foreach (var handler in queryHandlers)
            {
                testOutput.WriteLine($"- {handler.ServiceType.Name}: {handler.ImplementationType?.Name}");
            }

            queryHandlers.Should().NotBeEmpty("At least some query handlers should be registered");
        }
    }
}
