using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.ModuleApi;
using MeAjudaAi.Modules.Documents.Application.Options;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Documents.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpContextAccessor necessário para verificações de autorização nos handlers
        services.AddHttpContextAccessor();

        // Configurações de upload de documentos
        services.AddSingleton(_ =>
        {
            var options = new DocumentUploadOptions();
            configuration.GetSection(DocumentUploadOptions.SectionName).Bind(options);
            return options;
        });

        // Command Handlers - registro manual
        services.AddScoped<ICommandHandler<UploadDocumentCommand, UploadDocumentResponse>, UploadDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<RequestVerificationCommand, Result>, RequestVerificationCommandHandler>();
        services.AddScoped<ICommandHandler<ApproveDocumentCommand, Result>, ApproveDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<RejectDocumentCommand, Result>, RejectDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteDocumentCommand, Result>, DeleteDocumentCommandHandler>();

        // Query Handlers - registro manual
        services.AddScoped<IQueryHandler<GetDocumentByIdQuery, DocumentDto?>, GetDocumentByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>, GetProviderDocumentsQueryHandler>();

        // Module API - interface pública para outros módulos
        services.AddScoped<IDocumentsModuleApi, DocumentsModuleApi>();

        return services;
    }
}
