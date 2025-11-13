using MeAjudaAi.Modules.Documents.Application.DTOs;
using MediatR;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

public record UploadDocumentCommand(
    Guid ProviderId,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSizeBytes) : IRequest<UploadDocumentResponse>;
