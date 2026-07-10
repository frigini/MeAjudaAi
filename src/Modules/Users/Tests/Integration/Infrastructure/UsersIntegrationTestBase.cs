using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Tests.Integration.Infrastructure;

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
        var keycloakId = UuidGenerator.NewId().ToString();

        var user = User.Create(usernameVO, emailVO, firstName, lastName, keycloakId).Value;

        var dbContext = GetService<UsersDbContext>();

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}
