using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class SetProviderScheduleCommandHandlerTests : BookingsIntegrationTestBase
{
    private static DateTimeOffset GetNextWeekday(DayOfWeek day, int hour)
    {
        var today = DateTime.UtcNow.Date;
        int daysUntil = ((int)day - (int)today.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        var target = today.AddDays(daysUntil);
        return new DateTimeOffset(target.Year, target.Month, target.Day, hour, 0, 0, TimeSpan.Zero);
    }

    [Fact]
    public async Task SetSchedule_NewProvider_ShouldCreateSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());

        var start = GetNextWeekday(DayOfWeek.Monday, 8);
        var end = GetNextWeekday(DayOfWeek.Monday, 18);

        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(start, end)
            })
        };

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetProviderScheduleCommand, Result>>();
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var schedule = await db.ProviderSchedules.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProviderId == providerId);
        schedule.Should().NotBeNull();
    }

    [Fact]
    public async Task SetSchedule_NonExistingProvider_ShouldReturnNotFound()
    {
        // Arrange
        var start = GetNextWeekday(DayOfWeek.Monday, 8);
        var end = GetNextWeekday(DayOfWeek.Monday, 18);

        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(start, end)
            })
        };

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetProviderScheduleCommand, Result>>();
        var command = new SetProviderScheduleCommand(Guid.NewGuid(), availabilities, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
