using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Id do usuário.
/// </summary>
public class UserId : ValueObject
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId não pode ser vazio");
        Value = value;
    }

    public static UserId New() => new(UuidGenerator.NewId());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(UserId userId) => userId.Value;
    public static implicit operator UserId(Guid guid) => new(guid);
}
