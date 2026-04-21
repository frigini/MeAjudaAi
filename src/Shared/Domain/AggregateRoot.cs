using System.ComponentModel.DataAnnotations.Schema;

namespace MeAjudaAi.Shared.Domain;

public abstract class AggregateRoot<TId> : BaseEntity
{
    public new TId Id { get; protected set; } = default!;

    [NotMapped]
    public int Version { get; protected set; } = 1;

    protected AggregateRoot() { }

    protected AggregateRoot(TId id)
    {
        Id = id;
    }
}
