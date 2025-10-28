using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Extensions;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Documento de identificação do prestador de serviços.
/// Implementa validação específica para cada tipo de documento brasileiro.
/// </summary>
public class Document : ValueObject
{
    public string Number { get; private set; }
    public EDocumentType DocumentType { get; private set; }

    [JsonConstructor]
    public Document(string number, EDocumentType documentType)
    {
        Number = number;
        DocumentType = documentType;

        AddNotifications(new Contract<Notification>()
            .Requires()
            .IsTrue(Validate(), "Document.Number", ErrorMessages.InvalidDocument)
        );
    }

    private bool Validate()
    {
        return DocumentType switch
        {
            EDocumentType.CPF => Number.IsValidCpf(),
            EDocumentType.CNPJ => Number.IsValidCnpj(),
            EDocumentType.RG => !string.IsNullOrWhiteSpace(Number) && Number.Length >= 5,
            EDocumentType.CNH => !string.IsNullOrWhiteSpace(Number) && Number.Length >= 9,
            EDocumentType.Passport => !string.IsNullOrWhiteSpace(Number) && Number.Length >= 6,
            EDocumentType.Other => !string.IsNullOrWhiteSpace(Number),
            _ => false
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
        yield return DocumentType;
    }

    public override string ToString() => $"{DocumentType}: {Number}";
}
