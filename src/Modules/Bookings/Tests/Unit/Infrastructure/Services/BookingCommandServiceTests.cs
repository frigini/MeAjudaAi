using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
public class BookingCommandServiceTests : BaseSqliteInMemoryDatabaseTest<BookingsDbContext>
{
    private readonly Mock<ILogger<BookingCommandService>> _loggerMock = new();
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly BookingCommandService _service;

    public BookingCommandServiceTests() : base(options => new BookingsDbContext(options))
    {
        _localizerMock = MockLocalizerBuilder.Create()
            .WithSimpleKey("BookingAlreadyExistsForTimeSlot", "Já existe agendamento para este horário.")
            .WithSimpleKey("BookingConcurrencyConflict", "Conflito de concorrência ao salvar agendamento.")
            .Build();
        _service = new BookingCommandService(DbContext, _loggerMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_AddBooking_When_NoOverlapExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();

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
        var existingBooking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();
        DbContext.Bookings.Add(existingBooking);
        await DbContext.SaveChangesAsync();

        var newBooking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(9, 0)).WithEnd(new TimeOnly(11, 0)).Build())
            .Build();

        // Act
        var result = await _service.AddIfNoOverlapAsync(newBooking);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.Bookings.Overlap);
        result.Error.Message.Should().Be("Já existe agendamento para este horário.");
        (await DbContext.Bookings.AnyAsync(b => b.Id == newBooking.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_AddBooking_When_AdjacentSlots()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var existingBooking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(9, 0)).Build())
            .Build();
        DbContext.Bookings.Add(existingBooking);
        await DbContext.SaveChangesAsync();

        var newBooking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(date)
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(9, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();

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
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();
        
        // Force DbContext error by making it throw on SaveChanges
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var mockContext = new Mock<BookingsDbContext>(options) { CallBase = true };
        
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));
        
        var serviceWithError = new BookingCommandService(mockContext.Object, _loggerMock.Object, _localizerMock.Object);

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
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();

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
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var mockContext = new Mock<BookingsDbContext>(options) { CallBase = true };

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PostgresException("Connection error", "Error", "22P02", null!));

        var serviceWithError = new BookingCommandService(mockContext.Object, _loggerMock.Object, _localizerMock.Object);

        // Act & Assert
        Func<Task> act = () => serviceWithError.AddIfNoOverlapAsync(booking);
        await act.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_Fail_WithLocalizedMessage_OnConcurrencyConflict()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var booking = new BookingBuilder()
            .WithProviderId(providerId)
            .WithClientId(Guid.NewGuid())
            .WithServiceId(Guid.NewGuid())
            .WithDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
            .Build();

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var mockContext = new Mock<BookingsDbContext>(options) { CallBase = true };

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PostgresException("Unique violation", "Error", "23505", null!));

        var serviceWithError = new BookingCommandService(mockContext.Object, _loggerMock.Object, _localizerMock.Object);

        // Act
        var result = await serviceWithError.AddIfNoOverlapAsync(booking);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.Bookings.ConcurrencyConflict);
        result.Error.Message.Should().Be("Conflito de concorrência ao salvar agendamento.");
    }
}