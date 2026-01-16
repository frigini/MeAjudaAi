using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para adição de documento a um prestador de serviços.
/// </summary>
public record AddDocumentRequest : Request
{
    /// <summary>
    /// Número do documento.
    /// </summary>
    public string Number { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do documento.
    /// </summary>
    public EDocumentType DocumentType { get; init; }
}
