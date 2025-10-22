namespace MeAjudaAi.Shared.Messaging;

public interface IEventTypeRegistry
{
    Task<IEnumerable<Type>> GetAllEventTypesAsync(CancellationToken cancellationToken = default);

    Task<Type?> GetEventTypeAsync(string eventName, CancellationToken cancellationToken = default);
}
