using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Repositories;

public class ProviderScheduleRepositoryTests : BaseDatabaseTest
{
    private ProviderScheduleRepository _repository = null!;
    private BookingsDbContext _context = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var options = CreateDbContextOptions<BookingsDbContext>();

        _context = new BookingsDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new ProviderScheduleRepository(_context);
    }

    public override async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSchedule_WithAvailabilities()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var schedule = ProviderSchedule.Create(providerId, "UTC");
        
        var slot = TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var availability = Availability.Create(DayOfWeek.Monday, [slot]);
        schedule.SetAvailability(availability);

        // Act
        await _repository.AddAsync(schedule);

        // Assert
        _context.ChangeTracker.Clear();
        var saved = await _context.ProviderSchedules
            .Include(ps => ps.Availabilities)
                .ThenInclude(a => a.Slots)
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId);

        saved.Should().NotBeNull();
        saved!.TimeZoneId.Should().Be("UTC");
        saved.Availabilities.Should().HaveCount(1);
        saved.Availabilities[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        saved.Availabilities[0].Slots.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByProviderIdAsync_ShouldReturnSchedule()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var schedule = ProviderSchedule.Create(providerId);
        await _repository.AddAsync(schedule);

        // Act
        _context.ChangeTracker.Clear();
        var result = await _repository.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }
}
