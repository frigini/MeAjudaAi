using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Contracts;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Query Handlers - registro manual para garantir disponibilidade
        services.AddScoped<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>, GetProviderByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>, GetProviderByUserIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByIdsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByCityQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByStateQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByVerificationStatusQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByTypeQueryHandler>();

        // Command Handlers - registro manual para garantir disponibilidade  
        services.AddScoped<ICommandHandler<CreateProviderCommand, Result<ProviderDto>>, CreateProviderCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProviderProfileCommand, Result<ProviderDto>>, UpdateProviderProfileCommandHandler>();
        services.AddScoped<ICommandHandler<AddDocumentCommand, Result<ProviderDto>>, AddDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveDocumentCommand, Result<ProviderDto>>, RemoveDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<AddQualificationCommand, Result<ProviderDto>>, AddQualificationCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveQualificationCommand, Result<ProviderDto>>, RemoveQualificationCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateVerificationStatusCommand, Result<ProviderDto>>, UpdateVerificationStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProviderCommand, Result>, DeleteProviderCommandHandler>();

        // Module API - registro da API pública para comunicação entre módulos
        services.AddScoped<IProvidersModuleApi, ProvidersModuleApi>();

        return services;
    }
}