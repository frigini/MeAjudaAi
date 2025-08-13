using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.ValuleObjects;

public record UserId : ValueObject
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");

        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    public static implicit operator Guid(UserId userId) => userId.Value;
    public static implicit operator UserId(Guid guid) => new(guid);
    }