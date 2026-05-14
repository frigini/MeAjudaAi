using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;

public static class TestInfrastructureExtensions
{
    public static IServiceCollection AddServiceCatalogsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);

        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, TestCacheService>();

        services.AddTestDatabase<ServiceCatalogsDbContext>(
            options.Database,
            "MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");

        services.AddDbContext<ServiceCatalogsDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        });

        if (options.ExternalServices.UseMessageBusMock)
        {
            services.AddTestMessageBus();
        }

        // Add CQRS specific infrastructure
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IServiceCategoryQueries, DbContextServiceCategoryQueries>();
        services.AddScoped<IServiceQueries, DbContextServiceQueries>();

        // Registra command handlers
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory.CreateServiceCategoryCommand, MeAjudaAi.Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryDto>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory.CreateServiceCategoryCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.CreateServiceCommand, MeAjudaAi.Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceDto>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service.CreateServiceCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory.UpdateServiceCategoryCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory.UpdateServiceCategoryCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.UpdateServiceCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service.UpdateServiceCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory.DeleteServiceCategoryCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory.DeleteServiceCategoryCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.DeleteServiceCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service.DeleteServiceCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory.ActivateServiceCategoryCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory.ActivateServiceCategoryCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.ActivateServiceCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service.ActivateServiceCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory.DeactivateServiceCategoryCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory.DeactivateServiceCategoryCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.DeactivateServiceCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service.DeactivateServiceCommandHandler>();
        services.AddScoped<MeAjudaAi.Shared.Commands.ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.ChangeServiceCategoryCommand, MeAjudaAi.Contracts.Functional.Result>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service.ChangeServiceCategoryCommandHandler>();

        // Registra query handlers
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory.GetAllServiceCategoriesQuery, MeAjudaAi.Contracts.Functional.Result<System.Collections.Generic.IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryDto>>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory.GetAllServiceCategoriesQueryHandler>();
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory.GetServiceCategoryByIdQuery, MeAjudaAi.Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryDto?>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory.GetServiceCategoryByIdQueryHandler>();
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory.GetServiceCategoriesWithCountQuery, MeAjudaAi.Contracts.Functional.Result<System.Collections.Generic.IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryWithCountDto>>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory.GetServiceCategoriesWithCountQueryHandler>();
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service.GetAllServicesQuery, MeAjudaAi.Contracts.Functional.Result<System.Collections.Generic.IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceListDto>>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service.GetAllServicesQueryHandler>();
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service.GetServiceByIdQuery, MeAjudaAi.Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceDto?>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service.GetServiceByIdQueryHandler>();
        services.AddScoped<MeAjudaAi.Shared.Queries.IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service.GetServicesByCategoryQuery, MeAjudaAi.Contracts.Functional.Result<System.Collections.Generic.IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceListDto>>>, MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service.GetServicesByCategoryQueryHandler>();

        // Registra domain event handlers
        services.AddScoped<MeAjudaAi.Shared.Events.IEventHandler<MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service.ServiceActivatedDomainEvent>, MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers.ServiceActivatedDomainEventHandler>();
        services.AddScoped<MeAjudaAi.Shared.Events.IEventHandler<MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service.ServiceDeactivatedDomainEvent>, MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers.ServiceDeactivatedDomainEventHandler>();

        services.AddApplication();

        return services;
    }
}
