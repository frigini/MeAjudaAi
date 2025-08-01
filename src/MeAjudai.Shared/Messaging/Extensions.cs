using Azure.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Messages;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace MeAjudaAi.Shared.Messaging;

internal static class Extensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MessageBusOptions>? configureOptions = null)
    {
        services.AddOptions<ServiceBusOptions>()
            .Configure(opts => configuration.GetSection(ServiceBusOptions.SectionName).Bind(opts))
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.ConnectionString),
                "ServiceBus connection string not found. Configure 'Messaging:ServiceBus:ConnectionString' in appsettings.json")
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.TopicName),
                "ServiceBus topic name not found. Configure 'Messaging:ServiceBus:TopicName' in appsettings.json");

        services.Configure<MessageBusOptions>(_ => { });
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.AddSingleton(serviceProvider =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            return new ServiceBusClient(serviceBusOptions.ConnectionString);
        });

        services.AddRebus((configure, serviceProvider) =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            var messageBusOptions = serviceProvider.GetRequiredService<IOptions<MessageBusOptions>>().Value;

            return configure
                .Transport(t => t.UseAzureServiceBus(
                    serviceBusOptions.ConnectionString,
                    serviceBusOptions.TopicName))
                .Routing(r => r.TypeBased()
                    .Map<ServiceProviderRegistered>(messageBusOptions.TopicNamingConvention(typeof(ServiceProviderRegistered)))
                    .Map<ServiceRequested>(messageBusOptions.TopicNamingConvention(typeof(ServiceRequested))))
                .Options(o => o
                    .SetNumberOfWorkers(messageBusOptions.MaxConcurrentCalls));
        });

        services.AddSingleton<IMessageBus, ServiceBusMessageBus>();

        return services;
    }
}