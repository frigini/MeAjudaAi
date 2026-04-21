using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Tests.Integration.Repositories;

public class BookingRepositoryTests : BaseDatabaseTest
{
    private BookingRepository _repository = null!;
    private BookingsDbContext _context = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var options = CreateDbContextOptions<BookingsDbContext>();

        _context = new BookingsDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new BookingRepository(_context);
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
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBooking()
    {
        // Arrange
        var booking = CreateBooking();
        await _repository.AddAsync(booking);

        // Act
        var result = await _repository.GetByIdAsync(booking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task HasOverlapAsync_ShouldReturnTrue_WhenOverlapsExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.AddDays(1);
        
        var existingBooking = Booking.Create(
            providerId, 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10), baseTime.AddHours(12)));
        
        await _repository.AddAsync(existingBooking);

        // Act
        var hasOverlap = await _repository.HasOverlapAsync(
            providerId, 
            baseTime.AddHours(11), 
            baseTime.AddHours(13));

        // Assert
        hasOverlap.Should().BeTrue();
    }

    [Fact]
    public async Task HasOverlapAsync_ShouldReturnFalse_WhenNoOverlaps()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.AddDays(1);
        
        var existingBooking = Booking.Create(
            providerId, 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10), baseTime.AddHours(12)));
        
        await _repository.AddAsync(existingBooking);

        // Act
        var hasOverlap = await _repository.HasOverlapAsync(
            providerId, 
            baseTime.AddHours(13), 
            baseTime.AddHours(14));

        // Assert
        hasOverlap.Should().BeFalse();
    }

    private static Booking CreateBooking()
    {
        return Booking.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)));
    }
}
