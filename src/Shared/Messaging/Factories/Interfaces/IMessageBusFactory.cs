namespace MeAjudaAi.Shared.Messaging.Factories.Interfaces;

/// <summary>
/// Factory para criar o MessageBus apropriado baseado no ambiente
/// </summary>
public interface IMessageBusFactory
{
    IMessageBus CreateMessageBus();
}
