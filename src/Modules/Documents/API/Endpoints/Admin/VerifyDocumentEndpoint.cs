using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

/// <summary>
/// Endpoint para aprovar ou rejeitar documentos após verificação manual.
/// </summary>
public class VerifyDocumentEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{documentId:guid}/verify", VerifyDocumentAsync)
            .WithName(ApiEndpoints.Documents.Names.Verify)
            .WithSummary("Aprovar ou rejeitar documento")
            .WithDescription("""
                Aprova ou rejeita um documento após verificação manual.
                
                **Aprovar documento:**
                ```json
                {
                  "IsVerified": true,
                  "VerificationNotes": "Documento válido e legível"
                }
                ```
                
                **Rejeitar documento:**
                ```json
                {
                  "IsVerified": false,
                  "VerificationNotes": "Documento ilegível ou inválido"
                }
                ```
                
                **Requisitos:**
                - Documento deve estar em status PendingVerification
                - Apenas administradores podem executar esta ação
                """)
            .RequireAdmin()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> VerifyDocumentAsync(
        Guid documentId,
        VerifyDocumentRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        // Se IsVerified = true → Aprovar
        if (request.IsVerified)
        {
            var approveCommand = request.ToApproveCommand(documentId);
            var result = await commandDispatcher.SendAsync<ApproveDocumentCommand, Result>(approveCommand, cancellationToken);
            return Handle(result);
        }
        
        // Se IsVerified = false → Rejeitar
        var rejectCommand = request.ToRejectCommand(documentId);
        var rejectResult = await commandDispatcher.SendAsync<RejectDocumentCommand, Result>(rejectCommand, cancellationToken);
        return Handle(rejectResult);
    }
}
