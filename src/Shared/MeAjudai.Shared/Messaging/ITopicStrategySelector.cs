namespace MeAjudaAi.Shared.Messaging;

public interface ITopicStrategySelector
{
    string SelectTopicForEvent<T>();

    string SelectTopicForEvent(Type eventType);
}