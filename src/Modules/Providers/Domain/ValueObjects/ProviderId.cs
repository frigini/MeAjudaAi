using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Identificador único do prestador de serviços.
/// </summary>
public class ProviderId : ValueObject
{
    public Guid Value { get; }

    public ProviderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderId cannot be empty");
        Value = value;
    }

    public static ProviderId New() => new(UuidGenerator.NewId());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ProviderId providerId) => providerId.Value;
    public static implicit operator ProviderId(Guid guid) => new(guid);
}
