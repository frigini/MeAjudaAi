using MeAjudaAi.Modules.Communications.Application;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Modules.Communications.Tests.Integration.Infrastructure;

public static class TestExtensions
{
    public static IServiceCollection AddCommunicationsTestInfrastructure(this IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddDbContext<CommunicationsDbContext>((sp, dbOptions) =>
        {
            dbOptions.UseNpgsql(SharedTestContainers.PostgreSql.GetConnectionString(), npgsqlOptions =>
            {
                if (!string.IsNullOrWhiteSpace(options.Database.Schema))
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                }
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CommunicationsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(MeAjudaAi.Shared.Database.Constants.ModuleKeys.Communications,
            (sp, key) => sp.GetRequiredService<CommunicationsDbContext>());

        services.AddScoped<IRepository<EmailTemplate, Guid>>(sp => sp.GetRequiredService<CommunicationsDbContext>());
        services.AddScoped<IEmailTemplateQueries, DbContextEmailTemplateQueries>();

        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        services.AddApplication();

        services.AddLocalization();
        services.AddLogging();

        return services;
    }
}
