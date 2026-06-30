using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration;

public class SetProviderScheduleCommandHandlerTests : BookingsIntegrationTestBase
{
    [Fact]
    public async Task SetSchedule_NewProvider_ShouldCreateSchedule()
    {
        var providerId = Guid.NewGuid();
        GetMockProvidersApi().SeedProvider(providerId, Guid.NewGuid());

        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(new DateTimeOffset(2026, 7, 6, 8, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 7, 6, 18, 0, 0, TimeSpan.Zero))
            })
        };

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetProviderScheduleCommand, Result>>();
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

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
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(new DateTimeOffset(2026, 7, 6, 8, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 7, 6, 18, 0, 0, TimeSpan.Zero))
            })
        };

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetProviderScheduleCommand, Result>>();
        var command = new SetProviderScheduleCommand(Guid.NewGuid(), availabilities, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
