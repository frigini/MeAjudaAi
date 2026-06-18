using MeAjudaAi.Modules.Documents.API.Endpoints.Admin;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Documents.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DocumentsEndpoints
{
    public const string Tag = "Documents";

    /// <summary>
    /// Mapeia todos os endpoints do módulo Documents.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Documents.Base, Tag)
            .RequireAuthorization(); // Aplica autorização global

        endpoints.MapEndpoint<UploadDocumentEndpoint>()
            .MapEndpoint<GetDocumentByIdEndpoint>()
            .MapEndpoint<GetProviderDocumentsEndpoint>()
            .MapEndpoint<RequestVerificationEndpoint>()
            .MapEndpoint<VerifyDocumentEndpoint>()
            .MapEndpoint<DeleteDocumentEndpoint>();
    }
}
