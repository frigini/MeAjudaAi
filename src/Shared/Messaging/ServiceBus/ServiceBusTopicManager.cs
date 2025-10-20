using Azure.Messaging.ServiceBus.Administration;
using MeAjudaAi.Shared.Messaging.Strategy;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.ServiceBus;

internal class ServiceBusTopicManager(
    ServiceBusAdministrationClient adminClient,
    ServiceBusOptions options,
    IEventTypeRegistry eventRegistry,
    ITopicStrategySelector topicSelector,
    ILogger<ServiceBusTopicManager> logger) : IServiceBusTopicManager
{
    private readonly ServiceBusOptions _options = options;

    public async Task EnsureTopicsExistAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Ensuring Service Bus topics exist...");

        // Descobrir todos os eventos
        var eventTypes = await eventRegistry.GetAllEventTypesAsync(cancellationToken);
        var requiredTopics = new HashSet<string>();

        // Coletar todos os tópicos necessários
        foreach (var eventType in eventTypes)
        {
            var topicName = topicSelector.SelectTopicForEvent(eventType);
            requiredTopics.Add(topicName);
        }

        // Adicionar tópicos configurados manualmente
        requiredTopics.Add(_options.DefaultTopicName);
        foreach (var domainTopic in _options.DomainTopics.Values)
        {
            requiredTopics.Add(domainTopic);
        }

        // Criar tópicos
        foreach (var topicName in requiredTopics)
        {
            await CreateTopicIfNotExistsAsync(topicName, cancellationToken);
        }

        logger.LogInformation("Finished ensuring {Count} topics exist", requiredTopics.Count);
    }

    public async Task CreateTopicIfNotExistsAsync(string topicName, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await adminClient.TopicExistsAsync(topicName, cancellationToken);
            if (exists.Value)
            {
                logger.LogDebug("Topic {TopicName} already exists", topicName);
                return;
            }

            var topicOptions = new CreateTopicOptions(topicName)
            {
                DefaultMessageTimeToLive = TimeSpan.FromDays(14),
                MaxSizeInMegabytes = 1024,
                EnableBatchedOperations = true,
                EnablePartitioning = true,
                SupportOrdering = false
            };

            await adminClient.CreateTopicAsync(topicOptions, cancellationToken);
            logger.LogInformation("Created topic: {TopicName}", topicName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create topic: {TopicName}", topicName);
            throw;
        }
    }

    public async Task CreateSubscriptionIfNotExistsAsync(
        string topicName,
        string subscriptionName,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await adminClient.SubscriptionExistsAsync(topicName, subscriptionName, cancellationToken);
            if (exists.Value)
            {
                logger.LogDebug("Subscription {SubscriptionName} already exists on topic {TopicName}",
                    subscriptionName, topicName);
                return;
            }

            var subscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName)
            {
                DefaultMessageTimeToLive = TimeSpan.FromDays(14),
                MaxDeliveryCount = 10,
                EnableBatchedOperations = true,
                LockDuration = TimeSpan.FromMinutes(5)
            };

            // Adicionar filtro se especificado
            CreateRuleOptions? ruleOptions = null;
            if (!string.IsNullOrEmpty(filter))
            {
                ruleOptions = new CreateRuleOptions("CustomFilter", new SqlRuleFilter(filter));
            }

            await adminClient.CreateSubscriptionAsync(subscriptionOptions, ruleOptions, cancellationToken);
            logger.LogInformation("Created subscription: {SubscriptionName} on topic: {TopicName}",
                subscriptionName, topicName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create subscription: {SubscriptionName} on topic: {TopicName}",
                subscriptionName, topicName);
            throw;
        }
    }
}
