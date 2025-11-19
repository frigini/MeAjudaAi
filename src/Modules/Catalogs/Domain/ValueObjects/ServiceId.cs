using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Service aggregate.
/// </summary>
public class ServiceId : ValueObject
{
    public Guid Value { get; }

    public ServiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty");
        Value = value;
    }

    public static ServiceId New() => new(UuidGenerator.NewId());
    public static ServiceId From(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ServiceId id) => id.Value;
    public static implicit operator ServiceId(Guid value) => new(value);
}
