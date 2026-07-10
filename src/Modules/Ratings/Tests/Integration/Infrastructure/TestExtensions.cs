using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Ratings.Application;
using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Handlers.Commands;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Idempotency;
using MeAjudaAi.Modules.Ratings.Infrastructure.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.Infrastructure;

public static class RatingsTestInfrastructureExtensions
{
    public static IServiceCollection AddRatingsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);

        services.AddLocalization();
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        services.AddDbContext<RatingsDbContext>((sp, dbOptions) =>
        {
            dbOptions.UseNpgsql(SharedTestContainers.PostgreSql.GetConnectionString(), npgsqlOptions =>
            {
                if (!string.IsNullOrWhiteSpace(options.Database.Schema))
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                }
            })
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RatingsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Ratings, (sp, key) => sp.GetRequiredService<RatingsDbContext>());

        services.AddScoped<IRepository<Review, ReviewId>>(sp => sp.GetRequiredService<RatingsDbContext>());

        services.AddScoped<IIdempotencyRepository>(sp => new RatingsIdempotencyRepository(sp.GetRequiredService<RatingsDbContext>()));

        services.AddScoped<IReviewQueries, DbContextReviewQueries>();

        services.AddScoped<CreateReviewCommandHandler>();
        services.AddScoped<ICommandHandler<CreateReviewCommand, Guid>>(sp => sp.GetRequiredService<CreateReviewCommandHandler>());

        services.AddTestMessageBus();

        services.TryAddSingleton<IBookingsModuleApi, MockBookingsModuleApi>();

        services.AddApplication();

        return services;
    }
}

internal class MockBookingsModuleApi : IBookingsModuleApi
{
    private readonly Dictionary<(Guid ClientId, Guid ProviderId), bool> _completedBookings = new();

    public string ModuleName => "bookings";
    public string ApiVersion => "1.0";

    public void SeedCompletedBooking(Guid clientId, Guid providerId)
    {
        _completedBookings[(clientId, providerId)] = true;
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<Result<bool>> HasCompletedBookingAsync(Guid clientId, Guid providerId, CancellationToken cancellationToken = default)
    {
        var exists = _completedBookings.TryGetValue((clientId, providerId), out var completed) && completed;
        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result<ModuleBookingDto?>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ModuleBookingDto?>.Failure("Not implemented in tests"));
    }

    public Task<Result<IReadOnlyList<ModuleBookingDto>>> GetProviderBookingsAsync(Guid providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<ModuleBookingDto>>.Failure("Not implemented in tests"));
    }
}
