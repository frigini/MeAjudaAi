using MeAjudaAi.Architecture.Tests.Helpers;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using Xunit;
using Moq;

namespace MeAjudaAi.Architecture.Tests;

[Trait("Category", "Architecture")]
public class HandlerRegistrationTests
{
    private static IServiceCollection AddAllModulesForArchitectureTests(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["Stripe:ApiKey"] = "sk_test_dummy",
                ["Messaging:Enabled"] = "false"
            })
            .Build();

        var hostingEnv = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == "Testing");

        services.AddLogging();
        // Assume shared messaging is registered
        services.AddSingleton(Mock.Of<MeAjudaAi.Shared.Messaging.IMessageBus>());

        MeAjudaAi.Modules.Bookings.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Communications.Infrastructure.Extensions.AddCommunicationsInfrastructure(services, configuration);
        MeAjudaAi.Modules.Documents.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Locations.Infrastructure.Extensions.AddInfrastructure(services, configuration);
        MeAjudaAi.Modules.Payments.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Providers.Infrastructure.Extensions.AddInfrastructure(services, configuration);
        MeAjudaAi.Modules.Ratings.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.SearchProviders.Infrastructure.Extensions.AddSearchProvidersInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Extensions.AddServiceCatalogsInfrastructure(services, configuration);
        MeAjudaAi.Modules.Users.Infrastructure.Extensions.AddInfrastructure(services, configuration);

        return services;
    }

    [Fact]
    public void AllEventHandlers_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        AddAllModulesForArchitectureTests(services);

        var serviceProvider = services.BuildServiceProvider();
        var infraAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();
        
        // Discover all concrete IEventHandler<T> types in Infrastructure assemblies
        var handlerTypes = infraAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                       t.GetInterfaces().Any(i => i.IsGenericType && 
                                                i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .ToList();

        var unregisteredHandlers = new List<string>();

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            foreach (var interfaceType in interfaceTypes)
            {
                var registrations = services.Where(sd => sd.ServiceType == interfaceType).ToList();
                var isRegistered = registrations.Any(sd => 
                    sd.ImplementationType == handlerType || 
                    (sd.ImplementationFactory != null && CanResolve(serviceProvider, interfaceType, handlerType)));

                if (!isRegistered)
                {
                    unregisteredHandlers.Add($"{handlerType.FullName} (as {interfaceType.Name})");
                }
            }
        }

        unregisteredHandlers.Should().BeEmpty(
            "All event handlers found in infrastructure should be registered in the Dependency Injection container. " +
            "If a handler is missing, check the module's Extensions.cs file. " +
            "Unregistered handlers: \n" + string.Join("\n", unregisteredHandlers));
    }

    [Fact]
    public void AllDomainEventHandlers_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        AddAllModulesForArchitectureTests(services);

        var serviceProvider = services.BuildServiceProvider();
        var infraAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();
        
        // Discover all concrete IDomainEventHandler<T> types in Infrastructure assemblies
        var handlerTypes = infraAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                       t.GetInterfaces().Any(i => i.IsGenericType && 
                                                typeof(IDomainEvent).IsAssignableFrom(i.GetGenericArguments()[0])))
            .ToList();

        var unregisteredHandlers = new List<string>();

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && typeof(IDomainEvent).IsAssignableFrom(i.GetGenericArguments()[0]));

            foreach (var interfaceType in interfaceTypes)
            {
                var registrations = services.Where(sd => sd.ServiceType == interfaceType).ToList();
                var isRegistered = registrations.Any(sd => 
                    sd.ImplementationType == handlerType || 
                    (sd.ImplementationFactory != null && CanResolve(serviceProvider, interfaceType, handlerType)));

                if (!isRegistered)
                {
                    unregisteredHandlers.Add($"{handlerType.FullName} (as {interfaceType.Name})");
                }
            }
        }

        unregisteredHandlers.Should().BeEmpty(
            "All domain event handlers found in infrastructure should be registered in the Dependency Injection container. " +
            "If a handler is missing, check the module's Extensions.cs file. " +
            "Unregistered handlers: \n" + string.Join("\n", unregisteredHandlers));
    }

    private static bool CanResolve(IServiceProvider serviceProvider, Type serviceType, Type implementationType)
    {
        var instances = serviceProvider.GetServices(serviceType);
        return instances.Any(i => i != null && i.GetType() == implementationType);
    }
}
