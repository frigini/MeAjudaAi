using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresContext<CommunicationsDbContext>(builder => 
        {
            builder.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CommunicationsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Communications, (sp, key) => sp.GetRequiredService<CommunicationsDbContext>());

        services.AddScoped<IRepository<EmailTemplate, Guid>>(sp => sp.GetRequiredService<CommunicationsDbContext>());
        services.AddScoped<IRepository<CommunicationLog, Guid>>(sp => sp.GetRequiredService<CommunicationsDbContext>());

        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IEmailTemplateQueries, DbContextEmailTemplateQueries>();
        services.AddScoped<ICommunicationLogQueries, DbContextCommunicationLogQueries>();

        var stubsEnabled = configuration.GetValue("Communications:EnableStubs", true);
        
        if (stubsEnabled)
        {
            // Use TryAdd to allow real providers registered later to override these
            services.TryAddScoped<IEmailSender, EmailSenderStub>();
            services.TryAddScoped<ISmsSender, SmsSenderStub>();
            services.TryAddScoped<IPushSender, PushSenderStub>();
        }

        // Register startup validator as hosted service to fail fast with clear error
        services.AddHostedService(sp => 
            new CommunicationsStartupValidator(
                stubsEnabled,
                sp.GetService<IEmailSender>(),
                sp.GetService<ISmsSender>(),
                sp.GetService<IPushSender>(),
                sp.GetRequiredService<ILogger<CommunicationsStartupValidator>>()));

        return services;
    }
}