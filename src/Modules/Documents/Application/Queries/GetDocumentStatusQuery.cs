using MeAjudaAi.Modules.Documents.Application.DTOs;
using MediatR;

namespace MeAjudaAi.Modules.Documents.Application.Queries;

public record GetDocumentStatusQuery(Guid DocumentId) : IRequest<DocumentDto?>;
