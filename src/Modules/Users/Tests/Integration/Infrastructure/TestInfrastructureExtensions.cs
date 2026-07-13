using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Users;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.Modules.Users.Tests.Integration.Infrastructure;

public static class UsersTestInfrastructureExtensions
{
    public static IServiceCollection AddUsersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        services.AddCommonModuleTestInfrastructure<UsersDbContext>(
            options,
            migrationsAssembly: "MeAjudaAi.Modules.Users.Infrastructure",
            configureDbContext: dbOptions => dbOptions.UseSnakeCaseNamingConvention());

        services.AddUsersTestMocks(options?.ExternalServices);

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUserQueries, MeAjudaAi.Modules.Users.Infrastructure.Queries.DbContextUserQueries>();

        services.AddApplication();

        return services;
    }

    private static IServiceCollection AddUsersTestMocks(
        this IServiceCollection services,
        TestExternalServicesOptions? options)
    {
        if (options?.UseKeycloakMock != false)
        {
            services.Replace(ServiceDescriptor.Scoped<IKeycloakService, MockKeycloakService>());
            services.Replace(ServiceDescriptor.Scoped<IUserDomainService, MockUserDomainService>());
            services.Replace(ServiceDescriptor.Scoped<IAuthenticationDomainService, MockAuthenticationDomainService>());
        }

        if (options?.UseMessageBusMock != false)
        {
            services.AddTestMessageBus();
        }

        return services;
    }
}
