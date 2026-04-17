using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

[ExcludeFromCodeCoverage]

public record UploadDocumentCommand(
    Guid ProviderId,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSizeBytes) : ICommand<UploadDocumentResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
