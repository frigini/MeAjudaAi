namespace MeAjudaAi.Modules.Documents.Application.Constants;

/// <summary>
/// Constantes para modelos de Azure AI Document Intelligence
/// </summary>
public static class DocumentModelConstants
{
    /// <summary>
    /// Model IDs para Azure AI Document Intelligence
    /// </summary>
    public static class ModelIds
    {
        /// <summary>
        /// Modelo pré-construído para documentos de identidade (RG, CNH, etc)
        /// </summary>
        public const string IdentityDocument = "prebuilt-idDocument";

        /// <summary>
        /// Modelo pré-construído genérico para documentos não estruturados
        /// </summary>
        public const string GenericDocument = "prebuilt-document";

        /// <summary>
        /// Modelo pré-construído para faturas e recibos
        /// </summary>
        public const string Invoice = "prebuilt-invoice";

        /// <summary>
        /// Modelo pré-construído para recibos
        /// </summary>
        public const string Receipt = "prebuilt-receipt";
    }

    /// <summary>
    /// Tipos de documento conhecidos (mapeamento para EDocumentType)
    /// </summary>
    public static class DocumentTypes
    {
        public const string IdentityDocument = "identitydocument";
        public const string ProofOfResidence = "proofofresidence";
        public const string CriminalRecord = "criminalrecord";
        public const string Other = "other";
    }

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
}
