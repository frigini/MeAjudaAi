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

        if (configuration.GetValue("Communications:EnableStubs", true))
        {
            services.TryAddScoped<IEmailSender, EmailSenderStub>();
            services.TryAddScoped<ISmsSender, SmsSenderStub>();
            services.TryAddScoped<IPushSender, PushSenderStub>();
        }

        return services;
    }
}