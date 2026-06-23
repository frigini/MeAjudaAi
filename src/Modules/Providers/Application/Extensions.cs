using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.ModuleApi;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MeAjudaAi.Modules.Providers.Application;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Registra os serviços da camada de Application do módulo Providers.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Query Handlers - registro manual para garantir disponibilidade
        services.AddScoped<IQueryHandler<GetProvidersQuery, Result<PagedResult<ProviderDto>>>, GetProvidersQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>, GetProviderByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>, GetProviderByUserIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>, GetProviderByDocumentQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByIdsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByCityQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByStateQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByTypeQueryHandler>();
        services.AddScoped<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>, GetProvidersByVerificationStatusQueryHandler>();
        services.AddScoped<IQueryHandler<GetPublicProviderByIdOrSlugQuery, Result<PublicProviderDto?>>, GetPublicProviderByIdOrSlugQueryHandler>();

        // Command Handlers - registro manual para garantir disponibilidade
        services.AddScoped<ICommandHandler<CreateProviderCommand, Result<ProviderDto>>, CreateProviderCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterProviderCommand, Result<ProviderDto>>, RegisterProviderCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProviderProfileCommand, Result<ProviderDto>>, UpdateProviderProfileCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProviderCommand, Result>, DeleteProviderCommandHandler>();
        services.AddScoped<ICommandHandler<AddDocumentCommand, Result<ProviderDto>>, AddDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveDocumentCommand, Result<ProviderDto>>, RemoveDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<SetPrimaryDocumentCommand, Result<ProviderDto>>, SetPrimaryDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<AddQualificationCommand, Result<ProviderDto>>, AddQualificationCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveQualificationCommand, Result<ProviderDto>>, RemoveQualificationCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateVerificationStatusCommand, Result<ProviderDto>>, UpdateVerificationStatusCommandHandler>();
        services.AddScoped<ICommandHandler<AddServiceToProviderCommand, Result>, AddServiceToProviderCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveServiceFromProviderCommand, Result>, RemoveServiceFromProviderCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateProviderProfileCommand, Result>, ActivateProviderProfileCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateProviderProfileCommand, Result>, DeactivateProviderProfileCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProviderDeviceTokenCommand, Result>, UpdateProviderDeviceTokenCommandHandler>();
        services.AddScoped<ICommandHandler<ActivateProviderCommand, Result>, ActivateProviderCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteBasicInfoCommand, Result>, CompleteBasicInfoCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMyProviderProfileCommand, Result>, DeleteMyProviderProfileCommandHandler>();
        services.AddScoped<ICommandHandler<RejectProviderCommand, Result>, RejectProviderCommandHandler>();
        services.AddScoped<ICommandHandler<RequireBasicInfoCorrectionCommand, Result>, RequireBasicInfoCorrectionCommandHandler>();
        services.AddScoped<ICommandHandler<SuspendProviderCommand, Result>, SuspendProviderCommandHandler>();

        // Module API - registro da API pública para comunicação entre módulos
        services.AddScoped<IProvidersModuleApi, ProvidersModuleApi>();

        // Validators - registro dos validadores FluentValidation
        services.AddModuleValidators(Assembly.GetExecutingAssembly());

        return services;
    }
}
