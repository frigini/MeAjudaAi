namespace MeAjudaAi.Shared.Messaging.ServiceBus;

public sealed class ServiceBusOptions
{
    public const string SectionName = "Messaging:ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string TopicName { get; set; } = "meajudaai-events";
}