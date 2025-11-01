using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Extensions;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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

        // Normalize CPF and CNPJ to digits-only, keep other document types trimmed as-is
        Number = documentType switch
        {
            EDocumentType.CPF or EDocumentType.CNPJ => Regex.Replace(number, @"[^\d]", ""),
            _ => number.Trim()
        };

        DocumentType = documentType;

        if (!IsValid())
            throw new ArgumentException($"Documento do tipo {documentType} com número {number} é inválido", nameof(number));
    }

    private bool IsValid()
    {
        return DocumentType switch
        {
            EDocumentType.CPF => IsValidCpfBasic(Number),
            EDocumentType.CNPJ => IsValidCnpjBasic(Number),
            EDocumentType.RG => Number.Length >= 5,
            EDocumentType.CNH => Number.Length >= 9,
            EDocumentType.Passport => Number.Length >= 6,
            EDocumentType.Other => !string.IsNullOrWhiteSpace(Number),
            _ => false
        };
    }

    /// <summary>
    /// Validação básica de CPF - versão temporária
    /// </summary>
    private static bool IsValidCpfBasic(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove caracteres não numéricos
        cpf = Regex.Replace(cpf, @"[^\d]", "");

        // Verifica se tem 11 dígitos e não são todos iguais
        return cpf.Length == 11 && !cpf.All(c => c == cpf[0]);
    }

    /// <summary>
    /// Validação básica de CNPJ - versão temporária
    /// </summary>
    private static bool IsValidCnpjBasic(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove caracteres não numéricos
        cnpj = Regex.Replace(cnpj, @"[^\d]", "");

        // Verifica se tem 14 dígitos e não são todos iguais
        return cnpj.Length == 14 && !cnpj.All(c => c == cnpj[0]);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
        yield return DocumentType;
    }

    public override string ToString() => $"{DocumentType}: {Number}";
}