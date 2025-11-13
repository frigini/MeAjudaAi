using MediatR;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

public record RequestVerificationCommand(Guid DocumentId) : IRequest;
