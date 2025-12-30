using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;

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
            throw new ArgumentException("ProviderId não pode ser vazio");
        Value = value;
    }

    public static ProviderId New() => new(UuidGenerator.NewId());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ProviderId providerId)
    {
        ArgumentNullException.ThrowIfNull(providerId);
        return providerId.Value;
    }
    public static implicit operator ProviderId(Guid guid) => new(guid);
}
