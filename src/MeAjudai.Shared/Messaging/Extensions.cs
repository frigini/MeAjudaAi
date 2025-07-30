using Azure.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Messages;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace MeAjudaAi.Shared.Messaging;

internal static class Extensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services,
        IConfiguration configuration,
        Action<MessageBusOptions>? configureOptions = null)
    {
        var connectionString = configuration.GetConnectionString("ServiceBus")
            ?? throw new InvalidOperationException("ServiceBus connection string is required");

        services.AddRebus(configure => configure
            .Transport(t => t.UseAzureServiceBus(
                configuration.GetConnectionString("ServiceBus"), "main"))
            .Routing(r => r.TypeBased()
                .Map<ServiceProviderRegistered>("serviceprovider.events")
                .Map<ServiceRequested>("customer.events"))
            .Options(o => o.SetNumberOfWorkers(1)));

        services.AddSingleton(new ServiceBusClient(connectionString));

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<MessageBusOptions>(_ => { });
        }

        services.AddSingleton<IMessageBus, ServiceBusMessageBus>();

        return services;
    }
}