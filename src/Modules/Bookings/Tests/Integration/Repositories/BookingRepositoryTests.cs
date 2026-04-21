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
    public async Task HasOverlapAsync_ShouldReturnTrue_WhenOverlapsExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.AddDays(1);
        
        var existingBooking = Booking.Create(
            providerId, Guid.NewGuid(), Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10), baseTime.AddHours(12)));
        
        await _repository.AddAsync(existingBooking);

        // Act
        var hasOverlap = await _repository.HasOverlapAsync(
            providerId, baseTime.AddHours(11), baseTime.AddHours(13));

        // Assert
        hasOverlap.Should().BeTrue();
    }

    [Fact]
    public async Task HasOverlapAsync_ShouldReturnFalse_WhenIntervalsAreAdjacent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.AddDays(1);
        
        var existingBooking = Booking.Create(
            providerId, Guid.NewGuid(), Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10), baseTime.AddHours(12)));
        
        await _repository.AddAsync(existingBooking);

        // Act & Assert
        // Caso 1: Novo agendamento termina exatamente quando o outro começa
        var overlapBefore = await _repository.HasOverlapAsync(providerId, baseTime.AddHours(9), baseTime.AddHours(10));
        overlapBefore.Should().BeFalse();

        // Caso 2: Novo agendamento começa exatamente quando o outro termina
        var overlapAfter = await _repository.HasOverlapAsync(providerId, baseTime.AddHours(12), baseTime.AddHours(13));
        overlapAfter.Should().BeFalse();
    }

    [Fact]
    public async Task HasOverlapAsync_ShouldIgnoreCancelledAndRejectedBookings()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.AddDays(1);
        
        var cancelledBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10), baseTime.AddHours(12)));
        cancelledBooking.Cancel("Test");

        var rejectedBooking = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(14), baseTime.AddHours(16)));
        rejectedBooking.Reject("Test");
        
        await _repository.AddAsync(cancelledBooking);
        await _repository.AddAsync(rejectedBooking);

        // Act
        var overlapWithCancelled = await _repository.HasOverlapAsync(providerId, baseTime.AddHours(10), baseTime.AddHours(11));
        var overlapWithRejected = await _repository.HasOverlapAsync(providerId, baseTime.AddHours(14), baseTime.AddHours(15));

        // Assert
        overlapWithCancelled.Should().BeFalse();
        overlapWithRejected.Should().BeFalse();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldPersist_WhenNoOverlap()
    {
        // Arrange
        var booking = CreateBooking();

        // Act
        var result = await _repository.AddIfNoOverlapAsync(booking);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saved = await _repository.GetByIdAsync(booking.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_ShouldHandleConcurrency_AllowingOnlyOneSucceed()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.AddDays(2).Date;
        
        var booking1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10), baseTime.AddHours(11)));
        
        var booking2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), 
            TimeSlot.Create(baseTime.AddHours(10).AddMinutes(30), baseTime.AddHours(11).AddMinutes(30)));

        // Act
        // Para testar concorrência real, usamos contextos separados
        var options = CreateDbContextOptions<BookingsDbContext>();
        
        using var ctx1 = new BookingsDbContext(options);
        using var ctx2 = new BookingsDbContext(options);
        
        var repo1 = new BookingRepository(ctx1);
        var repo2 = new BookingRepository(ctx2);

        var task1 = repo1.AddIfNoOverlapAsync(booking1);
        var task2 = repo2.AddIfNoOverlapAsync(booking2);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.Count(r => r.IsSuccess).Should().Be(1);
        results.Count(r => r.IsFailure).Should().Be(1);
        
        // Verifica persistência final
        var finalCount = await _context.Bookings.CountAsync(b => b.ProviderId == providerId);
        finalCount.Should().Be(1);
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
