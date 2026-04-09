using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddCommunicationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddPostgresContext<CommunicationsDbContext>();

        // Repositories
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<ICommunicationLogRepository, CommunicationLogRepository>();

        // Senders Stubs
        services.AddScoped<IEmailSender, EmailSenderStub>();
        services.AddScoped<ISmsSender, SmsSenderStub>();
        services.AddScoped<IPushSender, PushSenderStub>();

        return services;
    }
}
