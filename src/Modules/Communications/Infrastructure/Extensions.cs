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
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo Communications.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo Communications.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddServices(configuration, environment);
        services.AddEventHandlers();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Communications, Schemas.Communications, DatabaseRoleConstants.Communications);

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPostgresContext<CommunicationsDbContext>(builder =>
        {
            builder.UseSnakeCaseNamingConvention();
        });

        // Unit of Work e Repositórios
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CommunicationsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Communications, (sp, key) => sp.GetRequiredService<CommunicationsDbContext>());

        services.AddScoped<IRepository<EmailTemplate, Guid>>(sp => sp.GetRequiredService<CommunicationsDbContext>());
        services.AddScoped<IRepository<CommunicationLog, Guid>>(sp => sp.GetRequiredService<CommunicationsDbContext>());

        // Repositórios específicos do módulo
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        // Consultas otimizadas
        services.AddScoped<IEmailTemplateQueries, DbContextEmailTemplateQueries>();
        services.AddScoped<ICommunicationLogQueries, DbContextCommunicationLogQueries>();
    }

    /// <summary>
    /// Configura os serviços de envio de mensagens (email, SMS, push).
    /// </summary>
    private static void AddServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var stubsEnabled = configuration.GetValue<bool?>("Communications:EnableStubs")
            ?? (environment.IsDevelopment() || environment.IsEnvironment("Testing"));

        if (stubsEnabled)
        {
            // Stubs para desenvolvimento/teste — usa TryAdd para permitir substituição
            services.TryAddScoped<IEmailSender, EmailSenderStub>();
            services.TryAddScoped<ISmsSender, SmsSenderStub>();
            services.TryAddScoped<IPushSender, PushSenderStub>();
        }
        else
        {
            // Provedores reais para produção
            // IMPORTANTE: Registre aqui suas implementações reais quando disponíveis.
            // Exemplo:
            //   services.AddScoped<IEmailSender, SendGridEmailSender>();
            //   services.AddScoped<ISmsSender, TwilioSmsSender>();
            //   services.AddScoped<IPushSender, FirebasePushSender>();
            //
            // Ou use um método de extensão dedicado:
            //   services.AddRealCommunicationProviders(configuration);
            //
            // Se nenhum provedor for registrado, o CommunicationsStartupValidator
            // irá falhar na inicialização com mensagem clara.
        }

        // Registrа validador de inicialização como hosted service para fail-fast com erro claro
        services.AddHostedService(sp =>
            new CommunicationsStartupValidator(
                stubsEnabled,
                sp.GetService<IEmailSender>(),
                sp.GetService<ISmsSender>(),
                sp.GetService<IPushSender>(),
                sp.GetRequiredService<ILogger<CommunicationsStartupValidator>>()));
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Communications.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        // Os event handlers são registrados via Application/Extensions.cs
    }
}
