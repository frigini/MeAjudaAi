using System.Reflection;
using Azure.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Tests.Mocks.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensions para configurar os mocks de messaging nos testes
/// </summary>
public static class MessagingMockExtensions
{
    /// <summary>
    /// Adiciona os mocks de messaging ao container de DI usando Scrutor onde aplicável
    /// </summary>
    public static IServiceCollection AddMessagingMocks(this IServiceCollection services)
    {
        // Remove implementações reais se existirem
        RemoveRealImplementations(services);

        // Usa Scrutor para registrar automaticamente todos os mocks de messaging do assembly atual
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes
                .Where(type => type.Namespace != null &&
                              type.Namespace.Contains("Messaging") &&
                              type.Name.StartsWith("Mock")))
            .AsSelf()
            .WithSingletonLifetime());

        // Registra os mocks específicos

        // Registra os mocks como as implementações do IMessageBus
        services.AddSingleton<IMessageBus>(provider => provider.GetRequiredService<MockServiceBusMessageBus>());

        return services;
    }

    /// <summary>
    /// Remove implementações reais dos sistemas de messaging
    /// </summary>
    private static void RemoveRealImplementations(IServiceCollection services)
    {
        // Remove ServiceBusClient se registrado
        var serviceBusDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceBusClient));
        if (serviceBusDescriptor != null)
        {
            services.Remove(serviceBusDescriptor);
        }

        // Remove outras implementações de IMessageBus
        var messageBusDescriptors = services.Where(d => d.ServiceType == typeof(IMessageBus)).ToList();
        foreach (var descriptor in messageBusDescriptors)
        {
            services.Remove(descriptor);
        }
    }
}
