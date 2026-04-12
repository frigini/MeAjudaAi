using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.Modules.Communications.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddCommunicationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistência
        services.AddPostgresContext<CommunicationsDbContext>(builder => 
        {
            builder.UseSnakeCaseNamingConvention();
        });

        // Repositórios
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<ICommunicationLogRepository, CommunicationLogRepository>();

        // Stubs de remetentes (ativados via feature flag para dev/testes)
        // Por padrão, ativa se não houver configuração para evitar crash na injeção de dependência do OutboxProcessorService
        if (configuration.GetValue("Communications:EnableStubs", true))
        {
            services.TryAddScoped<IEmailSender, EmailSenderStub>();
            services.TryAddScoped<ISmsSender, SmsSenderStub>();
            services.TryAddScoped<IPushSender, PushSenderStub>();
        }

        return services;
    }
}
