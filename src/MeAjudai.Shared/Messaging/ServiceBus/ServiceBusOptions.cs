namespace MeAjudaAi.Shared.Messaging.ServiceBus;

public sealed class ServiceBusOptions
{
    public const string SectionName = "Messaging:ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultTopicName { get; set; } = "meajudaai-events";
    public ETopicStrategy Strategy { get; set; } = ETopicStrategy.SingleWithFilters; // SingleWithFilters, MultipleByDomain, Hybrid

    public Dictionary<string, string> DomainTopics { get; set; } = new()
    {
        ["ServiceProvider"] = "serviceprovider-events",
        ["Customer"] = "customer-events",
        ["Billing"] = "billing-events",
        ["Notification"] = "notification-events"
    };

    public string GetTopicForDomain(string domain)
    {
        return DomainTopics.TryGetValue(domain, out var topic) ? topic : DefaultTopicName;
    }
}