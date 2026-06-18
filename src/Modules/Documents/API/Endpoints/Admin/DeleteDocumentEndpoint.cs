using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela exclusão de um documento.
/// </summary>
public class DeleteDocumentEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de exclusão de documento.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{documentId:guid}", DeleteDocumentAsync)
            .WithName("DeleteDocument")
            .WithSummary("Excluir documento")
            .WithDescription("""
                Exclui um documento e seu blob associado do Azure Blob Storage.
                
                **Requisitos:**
                - Apenas administradores podem executar esta ação
                - O documento deve existir
                
                **Efeito:**
                - Remove o registro do documento do banco de dados
                - Remove o blob associado do Azure Blob Storage
                """)
            .RequireAdmin()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> DeleteDocumentAsync(
        Guid documentId,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = documentId.ToDeleteCommand();
        var result = await commandDispatcher.SendAsync<DeleteDocumentCommand, Result>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return result.Error.StatusCode switch
            {
                StatusCodes.Status404NotFound => Results.NotFound(Result<bool>.Failure(result.Error)),
                _ => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.StatusCode,
                    title: "Erro ao excluir documento")
            };
        }

        return Results.Ok(Result<bool>.Success(true));
    }
}
