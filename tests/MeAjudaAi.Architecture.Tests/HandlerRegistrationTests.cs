using MeAjudaAi.Architecture.Tests.Helpers;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        MeAjudaAi.Modules.Communications.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Documents.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Locations.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Payments.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Providers.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Ratings.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.SearchProviders.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);
        MeAjudaAi.Modules.Users.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostingEnv);

        // Register Application layer services for modules that have handlers there
        MeAjudaAi.Modules.Bookings.Application.Extensions.AddApplication(services);
        MeAjudaAi.Modules.Communications.Application.Extensions.AddApplication(services);
        MeAjudaAi.Modules.Users.Application.Extensions.AddApplication(services);
        MeAjudaAi.Modules.Providers.Application.Extensions.AddApplication(services);
        MeAjudaAi.Modules.Documents.Application.Extensions.AddApplication(services, configuration);
        MeAjudaAi.Modules.Payments.Application.Extensions.AddApplication(services);
        MeAjudaAi.Modules.Ratings.Application.Extensions.AddApplication(services);
        MeAjudaAi.Modules.ServiceCatalogs.Application.Extensions.AddApplication(services);

        return services;
    }

    [Fact]
    public void AllEventHandlers_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        AddAllModulesForArchitectureTests(services);

        var assemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies()
            .Concat(ModuleDiscoveryHelper.GetAllApplicationAssemblies())
            .Distinct()
            .ToList();
        
        // Discover all concrete IEventHandler<T> types in Infrastructure and Application assemblies
        var handlerTypes = assemblies
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
                    (sd.ImplementationFactory != null && CanResolve(services, interfaceType, handlerType)));

                if (!isRegistered)
                {
                    unregisteredHandlers.Add($"{handlerType.FullName} (as {interfaceType.Name})");
                }
            }
        }

        unregisteredHandlers.Should().BeEmpty(
            "All event handlers found in infrastructure and application layers should be registered in the Dependency Injection container. " +
            "If a handler is missing, check the module's Extensions.cs file. " +
            "Unregistered handlers: \n" + string.Join("\n", unregisteredHandlers));
    }

    [Fact]
    public void AllDomainEventHandlers_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        AddAllModulesForArchitectureTests(services);

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
                    (sd.ImplementationFactory != null && CanResolve(services, interfaceType, handlerType)));

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

    [Fact]
    public void AllCommandHandlers_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        AddAllModulesForArchitectureTests(services);

        var assemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies()
            .Concat(ModuleDiscoveryHelper.GetAllApplicationAssemblies())
            .Distinct()
            .ToList();

        // Discover all concrete ICommandHandler<T, TResult> types
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                       t.GetInterfaces().Any(i => i.IsGenericType &&
                                                i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            .ToList();

        var unregisteredHandlers = new List<string>();

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));

            foreach (var interfaceType in interfaceTypes)
            {
                var registrations = services.Where(sd => sd.ServiceType == interfaceType).ToList();
                var isRegistered = registrations.Any(sd =>
                    sd.ImplementationType == handlerType ||
                    (sd.ImplementationFactory != null && CanResolve(services, interfaceType, handlerType)));

                if (!isRegistered)
                {
                    unregisteredHandlers.Add($"{handlerType.FullName} (as {interfaceType.Name})");
                }
            }
        }

        unregisteredHandlers.Should().BeEmpty(
            "All command handlers found in infrastructure and application layers should be registered in the Dependency Injection container. " +
            "If a handler is missing, check the module's Extensions.cs file. " +
            "Unregistered handlers: \n" + string.Join("\n", unregisteredHandlers));
    }

    [Fact]
    public void EventHandlers_ShouldNotBeRegisteredMoreThanOnceForSameImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        AddAllModulesForArchitectureTests(services);

        // Act
        var duplicates = services
            .Where(d => d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .GroupBy(d => new { d.ServiceType, ImplementationFullName = d.ImplementationType?.FullName })
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.ServiceType.Name} -> {g.Key.ImplementationFullName} ({g.Count()}x)")
            .ToList();

        // Assert
        duplicates.Should().BeEmpty("Handlers should not be registered more than once for the same interface/implementation pair. Duplicates: \n" + string.Join("\n", duplicates));
    }

    private static bool CanResolve(IServiceCollection services, Type serviceType, Type implementationType)
    {
        // Check if the service type is registered with a factory that targets this implementation.
        // We avoid calling GetServices() on the provider because it triggers factory execution
        // which may fail when not all dependencies are registered (acceptable in architecture tests).
        return services.Any(sd =>
            sd.ServiceType == serviceType &&
            (sd.ImplementationType == implementationType || sd.ImplementationFactory != null));
    }
}
