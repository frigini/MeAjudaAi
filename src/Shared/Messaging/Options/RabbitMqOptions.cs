using MeAjudaAi.Shared.Messaging.Strategy;

namespace MeAjudaAi.Shared.Messaging.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMQ";

    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultQueueName { get; set; } = "MeAjudaAi-events";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public ETopicStrategy Strategy { get; set; } = ETopicStrategy.Hybrid;

    public Dictionary<string, string> DomainQueues { get; set; } = new()
    {
        ["Users"] = "users-events"
        //["ServiceProvider"] = "serviceprovider-events",
        //["Customer"] = "customer-events",
        //["Billing"] = "billing-events",
        //["Notification"] = "notification-events"
    };

    public string GetQueueForDomain(string domain)
    {
        return DomainQueues.TryGetValue(domain, out var queue) ? queue : DefaultQueueName;
    }

    public string BuildConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
            return ConnectionString;

        return $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
    }
}
