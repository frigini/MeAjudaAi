using MeAjudaAi.Architecture.Tests.Helpers;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Architecture.Tests;

[Trait("Category", "Architecture")]
public class HandlerRegistrationTests
{
    [Fact]
    public void AllEventHandlers_ShouldBeRegisteredInDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        
        // Simular o registro de todos os módulos
        // No MeAjudaAi, cada módulo costuma ter uma extensão Add[ModuleName]Module
        // Aqui vamos descobrir e invocar essas extensões ou simular o comportamento
        
        var modules = ModuleDiscoveryHelper.DiscoverModules();
        var infraAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();
        
        // Ativar os registros de cada módulo
        foreach (var module in modules)
        {
            if (module.InfrastructureAssembly != null)
            {
                var extensionsType = module.InfrastructureAssembly.GetType($"MeAjudaAi.Modules.{module.Name}.Infrastructure.Extensions");
                if (extensionsType != null)
                {
                    var addModuleMethod = extensionsType.GetMethod($"Add{module.Name}Module", BindingFlags.Public | BindingFlags.Static);
                    if (addModuleMethod != null)
                    {
                        // Alguns AddModule recebem IConfiguration
                        var parameters = addModuleMethod.GetParameters();
                        if (parameters.Length == 2 && parameters[1].ParameterType == typeof(IConfiguration))
                        {
                            addModuleMethod.Invoke(null, new object[] { services, configuration });
                        }
                        else if (parameters.Length == 1)
                        {
                            addModuleMethod.Invoke(null, new object[] { services });
                        }
                    }
                }
            }
        }

        var serviceProvider = services.BuildServiceProvider();

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
                // Verifica se o handler está registrado para a interface correspondente
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
        var configuration = new ConfigurationBuilder().Build();
        
        var modules = ModuleDiscoveryHelper.DiscoverModules();
        var infraAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();
        
        foreach (var module in modules)
        {
            if (module.InfrastructureAssembly != null)
            {
                var extensionsType = module.InfrastructureAssembly.GetType($"MeAjudaAi.Modules.{module.Name}.Infrastructure.Extensions");
                if (extensionsType != null)
                {
                    var addModuleMethod = extensionsType.GetMethod($"Add{module.Name}Module", BindingFlags.Public | BindingFlags.Static);
                    if (addModuleMethod != null)
                    {
                        var parameters = addModuleMethod.GetParameters();
                        if (parameters.Length == 2 && parameters[1].ParameterType == typeof(IConfiguration))
                        {
                            addModuleMethod.Invoke(null, new object[] { services, configuration });
                        }
                        else if (parameters.Length == 1)
                        {
                            addModuleMethod.Invoke(null, new object[] { services });
                        }
                    }
                }
            }
        }

        var serviceProvider = services.BuildServiceProvider();

        // Discover all concrete IDomainEventHandler<T> types in Infrastructure assemblies
        // Note: Some projects might use a different name for the domain event handler interface
        // In MeAjudaAi, it seems to be IDomainEventHandler<T> or similar. Let's check IEventHandler<T> too 
        // as it is often used for both if they share the same base interface logic.
        
        var handlerTypes = infraAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                       t.GetInterfaces().Any(i => i.IsGenericType && 
                                                (i.Name.Contains("IDomainEventHandler") || 
                                                 i.Name.Contains("IDomainEventConsumer"))))
            .ToList();

        var unregisteredHandlers = new List<string>();

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && (i.Name.Contains("IDomainEventHandler") || i.Name.Contains("IDomainEventConsumer")));

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
        try
        {
            var instances = serviceProvider.GetServices(serviceType);
            return instances.Any(i => i != null && i.GetType() == implementationType);
        }
        catch
        {
            return false;
        }
    }
}
