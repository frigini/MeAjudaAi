using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Documents.Domain.ValueObjects;

/// <summary>
/// Identificador único do documento.
/// </summary>
public sealed class DocumentId : ValueObject
{
    public Guid Value { get; }

    public DocumentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DocumentId cannot be empty");
        Value = value;
    }

    public static DocumentId New() => new(UuidGenerator.NewId());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(DocumentId documentId)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        return documentId.Value;
    }
    public static implicit operator DocumentId(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
