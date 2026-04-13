using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Ratings.Domain.ValueObjects;

/// <summary>
/// Identificador único da avaliação.
/// </summary>
public class ReviewId : ValueObject
{
    public Guid Value { get; }

    public ReviewId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReviewId não pode ser vazio");
        Value = value;
    }

    public static ReviewId New() => new(UuidGenerator.NewId());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ReviewId reviewId)
    {
        ArgumentNullException.ThrowIfNull(reviewId);
        return reviewId.Value;
    }

    public static implicit operator ReviewId(Guid guid) => new(guid);
}
