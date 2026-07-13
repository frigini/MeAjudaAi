using MeAjudaAi.Modules.Providers.Application;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Tests.Integration.Infrastructure;

[ExcludeFromCodeCoverage]
public static class TestExtensions
{
    public static IServiceCollection AddProvidersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        services.AddCommonModuleTestInfrastructure<ProvidersDbContext>(options);

        services.AddScoped<IProviderQueries, DbContextProviderQueries>();
        services.AddScoped<IIdempotencyRepository>(sp => new ProviderIdempotencyRepository(sp.GetRequiredService<ProvidersDbContext>()));

        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        services.AddApplication();

        return services;
    }
}
