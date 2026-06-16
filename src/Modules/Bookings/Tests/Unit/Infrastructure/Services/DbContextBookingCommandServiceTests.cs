using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class DbContextBookingCommandServiceTests : BaseInMemoryDatabaseTest<BookingsDbContext>
{
    private readonly Mock<ILogger<DbContextBookingCommandService>> _loggerMock = new();
    private readonly DbContextBookingCommandService _service;

    public DbContextBookingCommandServiceTests() : base(options => new BookingsDbContext(options))
    {
        _service = new DbContextBookingCommandService(DbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_AddBooking_When_NoOverlapExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0)));

        // Act
        var result = await _service.AddIfNoOverlapAsync(booking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var savedBooking = await DbContext.Bookings.FindAsync(booking.Id);
        savedBooking.Should().NotBeNull();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_Fail_When_OverlapExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var existingBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0)));
        DbContext.Bookings.Add(existingBooking);
        await DbContext.SaveChangesAsync();

        var newBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(11, 0))); // Overlaps

        // Act
        var result = await _service.AddIfNoOverlapAsync(newBooking);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.Bookings.Overlap);
        (await DbContext.Bookings.AnyAsync(b => b.Id == newBooking.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_AddBooking_When_AdjacentSlots()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var existingBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(9, 0)));
        DbContext.Bookings.Add(existingBooking);
        await DbContext.SaveChangesAsync();

        var newBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))); // Adjacent

        // Act
        var result = await _service.AddIfNoOverlapAsync(newBooking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var savedBooking = await DbContext.Bookings.FindAsync(newBooking.Id);
        savedBooking.Should().NotBeNull();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_ThrowException_OnDatabaseError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0)));
        
        // Force DbContext error by making it throw on SaveChanges
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var mockContext = new Mock<BookingsDbContext>(options) { CallBase = true };
        
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));
        
        var serviceWithError = new DbContextBookingCommandService(mockContext.Object, _loggerMock.Object);

        // Act
        Func<Task> act = () => serviceWithError.AddIfNoOverlapAsync(booking);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_Throw_When_CancellationToken_Is_Cancelled()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0)));

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.AddIfNoOverlapAsync(booking, cts.Token));
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_Throw_When_NonConcurrency_DbException_Occurs()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeSlot.Create(new TimeOnly(8, 0), new TimeOnly(10, 0)));

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var mockContext = new Mock<BookingsDbContext>(options) { CallBase = true };

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PostgresException("Connection error", "Error", "22P02", null));

        var serviceWithError = new DbContextBookingCommandService(mockContext.Object, _loggerMock.Object);

        // Act & Assert
        Func<Task> act = () => serviceWithError.AddIfNoOverlapAsync(booking);
        await act.Should().ThrowAsync<PostgresException>();
    }
}
