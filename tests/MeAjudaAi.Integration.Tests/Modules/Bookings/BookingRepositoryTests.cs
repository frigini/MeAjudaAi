using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MeAjudaAi.Integration.Tests.Modules.Bookings;

public class BookingRepositoryTests : BaseDatabaseTest
{
    private BookingRepository _repository = null!;
    private BookingsDbContext _context = null!;
    private readonly Mock<ILogger<BookingRepository>> _loggerMock = new();

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var options = CreateDbContextOptions<BookingsDbContext>();

        _context = new BookingsDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new BookingRepository(_context, _loggerMock.Object);
    }

    public override async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task GetByProviderIdPagedAsync_ShouldApplyPaginationClamping()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        
        for (int i = 0; i < 5; i++)
        {
            var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, 
                TimeSlot.Create(new TimeOnly(10 + i, 0), new TimeOnly(11 + i, 0)));
            _context.Bookings.Add(booking);
        }
        await _context.SaveChangesAsync();

        // Act & Assert - Page < 1 should become 1
        var (items1, _) = await _repository.GetByProviderIdPagedAsync(providerId, null, null, 0, 10);
        items1.Should().HaveCount(5);

        // Act & Assert - PageSize > 100 should become 100
        var (items2, _) = await _repository.GetByProviderIdPagedAsync(providerId, null, null, 1, 1000);
        items2.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByProviderIdPagedAsync_ShouldApplyDateFilters()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);
        
        var b1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), today, 
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        var b2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), tomorrow, 
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        _context.Bookings.AddRange(b1, b2);
        await _context.SaveChangesAsync();

        // Act
        var (itemsToday, _) = await _repository.GetByProviderIdPagedAsync(providerId, today, today, 1, 10);
        var (itemsNone, _) = await _repository.GetByProviderIdPagedAsync(providerId, tomorrow, today, 1, 10); // Inverted

        // Assert
        itemsToday.Should().ContainSingle(b => b.Id == b1.Id);
        itemsNone.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveByProviderAndDateAsync_ShouldIgnoreInactiveStatuses()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var active = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, 
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        var cancelled = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, 
            TimeSlot.Create(new TimeOnly(12, 0), new TimeOnly(13, 0)));
        cancelled.Cancel("Test");

        var rejected = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, 
            TimeSlot.Create(new TimeOnly(14, 0), new TimeOnly(15, 0)));
        rejected.Reject("Test");

        _context.Bookings.AddRange(active, cancelled, rejected);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByProviderAndDateAsync(providerId, date);

        // Assert
        result.Should().ContainSingle(b => b.Id == active.Id);
        result.Should().NotContain(b => b.Id == cancelled.Id);
        result.Should().NotContain(b => b.Id == rejected.Id);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldBeIdempotent()
    {
        // Arrange
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));

        // Act
        var firstResult = await _repository.AddIfNoOverlapAsync(booking);
        var secondResult = await _repository.AddIfNoOverlapAsync(booking);

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        
        var count = await _context.Bookings.CountAsync(b => b.Id == booking.Id);
        count.Should().Be(1);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldSucceed_WhenEndEqualsNextStart()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        
        var existing = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        _context.Bookings.Add(existing);
        await _context.SaveChangesAsync();

        var next = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(11, 0), new TimeOnly(12, 0)));

        // Act
        var result = await _repository.AddIfNoOverlapAsync(next);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
