namespace MeAjudaAi.Shared.Messaging.Factories;

/// <summary>
/// Factory para criar o MessageBus apropriado baseado no ambiente
/// </summary>
public interface IMessageBusFactory
{
    IMessageBus CreateMessageBus();
}
