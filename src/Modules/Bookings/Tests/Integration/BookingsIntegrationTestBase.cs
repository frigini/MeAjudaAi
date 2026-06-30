using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public abstract class BookingsIntegrationTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"bookings_test_{GetType().Name}",
                Username = "test_user",
                Password = "test_password",
                Schema = "bookings"
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddBookingsTestInfrastructure(options);
    }

    protected MockProvidersModuleApi GetMockProvidersApi() =>
        GetService<IProvidersModuleApi>() as MockProvidersModuleApi
        ?? throw new InvalidOperationException("IProvidersModuleApi is not MockProvidersModuleApi");

    protected MockServiceCatalogsModuleApi GetMockServiceCatalogsApi() =>
        GetService<IServiceCatalogsModuleApi>() as MockServiceCatalogsModuleApi
        ?? throw new InvalidOperationException("IServiceCatalogsModuleApi is not MockServiceCatalogsModuleApi");

    protected async Task<Booking> CreateBookingAsync(
        Guid providerId, Guid clientId, Guid serviceId,
        DateOnly? date = null, TimeOnly? start = null, TimeOnly? end = null,
        CancellationToken cancellationToken = default)
    {
        var bookingDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var startTime = start ?? new TimeOnly(10, 0);
        var endTime = end ?? new TimeOnly(11, 0);
        var slot = TimeSlot.Create(startTime, endTime);
        var booking = Booking.Create(providerId, clientId, serviceId, bookingDate, slot);

        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(cancellationToken);
        return booking;
    }

    protected async Task<ProviderSchedule> CreateScheduleAsync(
        Guid providerId, string timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
    {
        var schedule = ProviderSchedule.Create(providerId, timeZoneId);

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var slots = new[] { TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)) };
            schedule.SetAvailability(Availability.Create(day, slots));
        }

        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        context.ProviderSchedules.Add(schedule);
        await context.SaveChangesAsync(cancellationToken);
        return schedule;
    }
}
