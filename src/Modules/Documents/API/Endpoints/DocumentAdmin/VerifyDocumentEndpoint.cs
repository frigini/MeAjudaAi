using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.DocumentAdmin;

/// <summary>
/// Endpoint para aprovar ou rejeitar documentos após verificação manual.
/// </summary>
public class VerifyDocumentEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{documentId:guid}/verify", VerifyDocumentAsync)
            .WithName("VerifyDocument")
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
            .Produces(StatusCodes.Status403Forbidden);

    private static async Task<IResult> VerifyDocumentAsync(
        Guid documentId,
        VerifyDocumentRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        // Se IsVerified = true → Aprovar
        if (request.IsVerified)
        {
            var approveCommand = new ApproveDocumentCommand(documentId, request.VerificationNotes);
            var result = await commandDispatcher.SendAsync<ApproveDocumentCommand, Result>(approveCommand, cancellationToken);
            return Handle(result);
        }
        
        // Se IsVerified = false → Rejeitar
        var rejectionReason = request.VerificationNotes ?? "Documento rejeitado durante verificação";
        var rejectCommand = new RejectDocumentCommand(documentId, rejectionReason);
        var rejectResult = await commandDispatcher.SendAsync<RejectDocumentCommand, Result>(rejectCommand, cancellationToken);
        return Handle(rejectResult);
    }
}
