using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Modules.Bookings.Application;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Queries;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using MeAjudaAi.Shared.Caching.Interfaces;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Providers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.ServiceCatalogs;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;

public static class BookingsTestInfrastructureExtensions
{
    public static IServiceCollection AddBookingsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);

        services.AddLocalization();
        services.AddTestLogging();
        services.AddTestCache(options.Cache);
        services.AddSingleton<ICacheService, TestCacheService>();

        services.AddDbContext<BookingsDbContext>(dbOptions =>
        {
            var connection = SharedTestContainers.PostgreSql.GetConnectionString();
            dbOptions.UseNpgsql(connection, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(BookingsDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
            }).ConfigureWarnings(x =>
            {
                x.Ignore(RelationalEventId.PendingModelChangesWarning);
            });
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Bookings, (sp, key) => sp.GetRequiredService<BookingsDbContext>());

        services.AddScoped<IRepository<Booking, Guid>>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddScoped<IRepository<ProviderSchedule, Guid>>(sp => sp.GetRequiredService<BookingsDbContext>());

        services.AddScoped<IBookingQueries, DbContextBookingQueries>();
        services.AddScoped<IProviderScheduleQueries, DbContextProviderScheduleQueries>();
        services.AddScoped<IBookingCommandService, BookingCommandService>();

        services.AddTestMessageBus();

        services.TryAddSingleton<IProvidersModuleApi, MockProvidersModuleApi>();
        services.TryAddSingleton<IServiceCatalogsModuleApi, MockServiceCatalogsModuleApi>();

        services.AddCommands();
        services.AddQueries();
        services.AddApplication();

        return services;
    }
}
