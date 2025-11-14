using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Search.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for SearchableProvider entity.
/// </summary>
public sealed record SearchableProviderId(Guid Value)
{
    public static SearchableProviderId New() => new(Guid.CreateVersion7());

    public static SearchableProviderId From(Guid value) => new(value);

    public static implicit operator Guid(SearchableProviderId id) => id.Value;
}
