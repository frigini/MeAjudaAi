using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.DocumentAdmin;

/// <summary>
/// Endpoint responsável pela solicitação de verificação manual de documento.
/// </summary>
public class RequestVerificationEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de solicitação de verificação.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{documentId:guid}/verify", RequestVerificationAsync)
            .WithName("RequestVerification")
            .WithSummary("Solicitar verificação manual")
            .WithDescription("""
                Solicita verificação manual de um documento quando OCR falha ou precisa validação adicional.
                
                **Quando usar:**
                - OCR não conseguiu extrair dados do documento
                - Documento foi rejeitado automaticamente mas precisa revisão
                - Necessidade de validação humana adicional
                
                **Resultado:**
                - Documento entra em fila de verificação manual
                - Status alterado para PendingVerification
                - Administrador será notificado para análise
                """)
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> RequestVerificationAsync(
        Guid documentId,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new RequestVerificationCommand(documentId);
        await commandDispatcher.SendAsync(command, cancellationToken);

        return Results.Accepted();
    }
}
