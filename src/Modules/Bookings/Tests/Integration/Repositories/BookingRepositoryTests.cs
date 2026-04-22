using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Repositories;

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
    public async Task AddAsync_ShouldPersistBooking()
    {
        // Arrange
        var booking = CreateBooking();

        // Act
        await _repository.AddAsync(booking);

        // Assert
        var savedBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
        savedBooking.Should().NotBeNull();
        savedBooking!.ProviderId.Should().Be(booking.ProviderId);
        savedBooking.Status.Should().Be(EBookingStatus.Pending);
        savedBooking.Date.Should().Be(booking.Date);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldSucceed_WhenNoOverlapsExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2);
        
        var existingBooking = Booking.Create(
            providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(12, 0)));
        await _repository.AddAsync(existingBooking);

        var newBooking = Booking.Create(
            providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(12, 0), new TimeOnly(13, 0))); // Adjacente

        // Act
        var result = await _repository.AddIfNoOverlapAsync(newBooking);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldFail_WhenOverlapsExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2);
        
        var existingBooking = Booking.Create(
            providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(12, 0)));
        await _repository.AddAsync(existingBooking);

        var newBooking = Booking.Create(
            providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(11, 0), new TimeOnly(13, 0))); // Sobrepõe

        // Act
        var result = await _repository.AddIfNoOverlapAsync(newBooking);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldHandleConcurrency_AllowingOnlyOneSucceed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        
        var booking1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), tomorrow,
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        
        var booking2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), tomorrow,
            TimeSlot.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)));

        // Act
        // Para testar concorrência real, usamos contextos separados
        var options = CreateDbContextOptions<BookingsDbContext>();
        
        await using var ctx1 = new BookingsDbContext(options);
        await using var ctx2 = new BookingsDbContext(options);
        
        var repo1 = new BookingRepository(ctx1, _loggerMock.Object);
        var repo2 = new BookingRepository(ctx2, _loggerMock.Object);

        var task1 = repo1.AddIfNoOverlapAsync(booking1);
        var task2 = repo2.AddIfNoOverlapAsync(booking2);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.Count(r => r.IsSuccess).Should().Be(1);
        results.Count(r => r.IsFailure).Should().Be(1);
        
        var finalCount = await _context.Bookings.CountAsync(b => b.ProviderId == providerId && b.Date == tomorrow);
        finalCount.Should().Be(1);
    }

    private static Booking CreateBooking()
    {
        return Booking.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
    }
}
