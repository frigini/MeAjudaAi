using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure;

/// <summary>
/// Classe base para testes de integração específicos do módulo Users.
/// </summary>
public abstract class UsersIntegrationTestBase : BaseIntegrationTest
{
    /// <summary>
    /// Configurações padrão para testes do módulo Users
    /// </summary>
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"test_db_{GetType().Name.ToUpperInvariant()}",
                Username = "test_user",
                Password = "test_password",
                Schema = "users"
            },
            Cache = new TestCacheOptions
            {
                Enabled = true // Usa o Redis compartilhado
            },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };
    }

    /// <summary>
    /// Configura serviços específicos do módulo Users
    /// </summary>
    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddUsersTestInfrastructure(options);
    }

    /// <summary>
    /// Setup específico do módulo Users (configurações adicionais se necessário)
    /// </summary>
    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        // Qualquer setup específico adicional do módulo Users pode ser feito aqui
        // As migrações são aplicadas automaticamente pelo sistema de auto-descoberta
        await Task.CompletedTask;
    }

    /// <summary>
    /// Cria um usuário para teste e persiste no banco de dados
    /// </summary>
    protected async Task<User> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var usernameVO = new Username(username);
        var emailVO = new Email(email);
        var keycloakId = $"keycloak_{UuidGenerator.NewId()}";

        var user = new User(usernameVO, emailVO, firstName, lastName, keycloakId);

        // Obter contexto
        var dbContext = GetService<UsersDbContext>();

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}
