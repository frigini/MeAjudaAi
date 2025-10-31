using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Extensions;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Documento de identificação do prestador de serviços.
/// Implementa validação específica para cada tipo de documento brasileiro.
/// </summary>
public sealed class Document : ValueObject
{
    public string Number { get; private set; }
    public EDocumentType DocumentType { get; private set; }

    /// <summary>
    /// Construtor privado para Entity Framework
    /// </summary>
    private Document()
    {
        Number = string.Empty;
        DocumentType = EDocumentType.CPF;
    }

    [JsonConstructor]
    public Document(string number, EDocumentType documentType)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Número do documento não pode ser vazio", nameof(number));

        Number = number.Trim();
        DocumentType = documentType;

        if (!IsValid())
            throw new ArgumentException($"Documento do tipo {documentType} com número {number} é inválido", nameof(number));
    }

    private bool IsValid()
    {
        return DocumentType switch
        {
            EDocumentType.CPF => Number.IsValidCpf(),
            EDocumentType.CNPJ => Number.IsValidCnpj(),
            EDocumentType.RG => Number.Length >= 5,
            EDocumentType.CNH => Number.Length >= 9,
            EDocumentType.Passport => Number.Length >= 6,
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