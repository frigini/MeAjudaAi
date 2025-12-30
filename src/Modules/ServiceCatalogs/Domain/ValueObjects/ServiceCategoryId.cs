using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;

/// <summary>
/// Identificador fortemente tipado para o agregado ServiceCategory.
/// </summary>
public class ServiceCategoryId : ValueObject
{
    public Guid Value { get; }

    public ServiceCategoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceCategoryId cannot be empty");
        Value = value;
    }

    public static ServiceCategoryId New() => new(UuidGenerator.NewId());
    public static ServiceCategoryId From(Guid value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ServiceCategoryId id) => id.Value;
    public static implicit operator ServiceCategoryId(Guid value) => new(value);
}
