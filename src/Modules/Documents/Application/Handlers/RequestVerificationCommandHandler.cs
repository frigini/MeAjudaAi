using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class RequestVerificationCommandHandler : ICommandHandler<RequestVerificationCommand>
{
    public Task HandleAsync(RequestVerificationCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Implementar lógica de solicitação de verificação manual
        // Por enquanto, apenas retorna sucesso
        return Task.CompletedTask;
    }
}
