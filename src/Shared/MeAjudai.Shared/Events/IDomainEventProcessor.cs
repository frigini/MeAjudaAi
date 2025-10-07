namespace MeAjudaAi.Shared.Events;

public interface IDomainEventProcessor
{
    Task ProcessDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}