using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Messaging.Strategy;

namespace MeAjudaAi.Shared.Messaging.Options;

[ExcludeFromCodeCoverage]
public static class ModuleNames
{
    public const string Users = "Users";
    public const string Payments = "Payments";
    public const string Communications = "Communications";
    public const string Ratings = "Ratings";
    public const string Providers = "Providers";
    public const string Documents = "Documents";
    public const string Locations = "Locations";
    public const string SearchProviders = "SearchProviders";
    public const string ServiceCatalogs = "ServiceCatalogs";
}

[ExcludeFromCodeCoverage]
public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMQ";

    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultQueueName { get; set; } = "meajudaai-events";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public ETopicStrategy Strategy { get; set; } = ETopicStrategy.Hybrid;

    public Dictionary<string, string> DomainQueues { get; set; } = new()
    {
        [ModuleNames.Users] = "users-events",
        [ModuleNames.Payments] = "payments-events",
        [ModuleNames.Communications] = "communications-events",
        [ModuleNames.Ratings] = "ratings-events",
        [ModuleNames.Providers] = "providers-events",
        [ModuleNames.Documents] = "documents-events",
        [ModuleNames.Locations] = "locations-events",
        [ModuleNames.SearchProviders] = "searchproviders-events",
        [ModuleNames.ServiceCatalogs] = "servicecatalogs-events"
    };

    public string GetQueueForDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return DefaultQueueName;
        return DomainQueues.TryGetValue(domain.Trim(), out var queue) ? queue : DefaultQueueName;
    }

    public string BuildConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
            return ConnectionString;

        return $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
    }
}
