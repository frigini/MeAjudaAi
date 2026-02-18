using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para adição de documento a um prestador de serviços.
/// </summary>
/// <param name="Number">Número do documento</param>
/// <param name="DocumentType">Tipo do documento</param>
public sealed record AddDocumentRequest(string Number, EDocumentType DocumentType);
