using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.ModuleApi;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts.Modules.Documents;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Documents.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // HttpContextAccessor necessário para verificações de autorização nos handlers
        services.AddHttpContextAccessor();

        // Command Handlers - registro manual
        services.AddScoped<ICommandHandler<UploadDocumentCommand, UploadDocumentResponse>, UploadDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<RequestVerificationCommand, Result>, RequestVerificationCommandHandler>();

        // Query Handlers - registro manual
        services.AddScoped<IQueryHandler<GetDocumentStatusQuery, DocumentDto?>, GetDocumentStatusQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>, GetProviderDocumentsQueryHandler>();

        // Module API - interface pública para outros módulos
        services.AddScoped<IDocumentsModuleApi, DocumentsModuleApi>();

        return services;
    }
}
