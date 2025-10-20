namespace MeAjudaAi.Shared.Messaging.Strategy;

public interface ITopicStrategySelector
{
    string SelectTopicForEvent<T>();

    string SelectTopicForEvent(Type eventType);
}
