using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela solicitação de verificação manual de documento.
/// </summary>
public class RequestVerificationEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de solicitação de verificação.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{documentId:guid}/request-verification", RequestVerificationAsync)
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
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> RequestVerificationAsync(
        Guid documentId,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = documentId.ToRequestVerificationCommand();
        var result = await commandDispatcher.SendAsync<RequestVerificationCommand, Result>(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return Results.Accepted();
    }
}
