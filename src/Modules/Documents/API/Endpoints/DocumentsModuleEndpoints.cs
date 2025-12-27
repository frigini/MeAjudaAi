using MeAjudaAi.Modules.Documents.API.Endpoints.DocumentAdmin;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Modules.Documents.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Documents.
/// </summary>
public static class DocumentsModuleEndpoints
{
    /// <summary>
    /// Mapeia todos os endpoints do módulo Documents.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void MapDocumentsEndpoints(this WebApplication app)
    {
        // Usa o sistema unificado de versionamento via BaseEndpoint
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, "documents", "Documents")
            .RequireAuthorization(); // Aplica autorização global

        // Endpoints de gestão de documentos
        endpoints.MapEndpoint<UploadDocumentEndpoint>()
            .MapEndpoint<GetDocumentStatusEndpoint>()
            .MapEndpoint<GetProviderDocumentsEndpoint>()
            .MapEndpoint<RequestVerificationEndpoint>()
            .MapEndpoint<VerifyDocumentEndpoint>();
    }
}
