namespace MeAjudaAi.Shared.Domain;

public abstract class AggregateRoot<TId> : BaseEntity
{
    public new TId Id { get; protected set; } = default!;

    protected AggregateRoot() { }

    protected AggregateRoot(TId id)
    {
        Id = id;
    }
}
