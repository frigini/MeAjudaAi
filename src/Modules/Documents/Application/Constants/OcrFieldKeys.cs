namespace MeAjudaAi.Modules.Documents.Application.Constants;

/// <summary>
/// Chaves de campos OCR comuns extraídos de documentos brasileiros
/// </summary>
public static class OcrFieldKeys
{
    // Documento de identidade
    public const string DocumentNumber = "DocumentNumber";
    public const string FullName = "FullName";
    public const string DateOfBirth = "DateOfBirth";
    public const string IssueDate = "IssueDate";
    public const string ExpiryDate = "ExpiryDate";
    public const string Cpf = "CPF";
    public const string Rg = "RG";
    public const string IssuingAuthority = "IssuingAuthority";

    // Endereço
    public const string Address = "Address";
    public const string City = "City";
    public const string State = "State";
    public const string PostalCode = "PostalCode";

    // Outros
    public const string Gender = "Gender";
    public const string Nationality = "Nationality";
}
