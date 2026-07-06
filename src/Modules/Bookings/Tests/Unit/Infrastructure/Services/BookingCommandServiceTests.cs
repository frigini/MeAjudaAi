using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Services;
using MeAjudaAi.Modules.Bookings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Integration")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Infrastructure")]
[Collection("BookingsIntegrationTests")]
public class BookingCommandServiceTests : BookingsIntegrationTestBase
{
    private readonly Mock<ILogger<BookingCommandService>> _loggerMock = new();
    private Mock<IStringLocalizer<Strings>> _localizerMock;
    private BookingCommandService _service;

    protected override async Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        await base.OnModuleInitializeAsync(serviceProvider);
        _localizerMock = MockLocalizerBuilder.Create()
            .WithSimpleKey("BookingAlreadyExistsForTimeSlot", "Já existe agendamento para este horário.")
            .WithSimpleKey("BookingConcurrencyConflict", "Conflito de concorrência ao salvar agendamento.")
            .Build();
        var context = serviceProvider.GetRequiredService<BookingsDbContext>();
        _service = new BookingCommandService(context, _loggerMock.Object, _localizerMock.Object);
    }

    private BookingsDbContext CreateContext(out IServiceScope scope)
    {
        scope = CreateScope();
        return scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
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
        var verifyContext = CreateContext(out var verifyScope);
        using (verifyScope)
        {
            var savedBooking = await verifyContext.Bookings.FindAsync(booking.Id);
            savedBooking.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_Fail_When_OverlapExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var context = CreateContext(out var scope);
        using (scope)
        {
            var existingBooking = new BookingBuilder()
                .WithProviderId(providerId)
                .WithClientId(Guid.NewGuid())
                .WithServiceId(Guid.NewGuid())
                .WithDate(date)
                .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(10, 0)).Build())
                .Build();
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();
        }

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
        var assertContext = CreateContext(out var assertScope);
        using (assertScope)
        {
            (await assertContext.Bookings.AnyAsync(b => b.Id == newBooking.Id)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_Should_AddBooking_When_AdjacentSlots()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var context = CreateContext(out var scope);
        using (scope)
        {
            var existingBooking = new BookingBuilder()
                .WithProviderId(providerId)
                .WithClientId(Guid.NewGuid())
                .WithServiceId(Guid.NewGuid())
                .WithDate(date)
                .WithTimeSlot(new TimeSlotBuilder().WithStart(new TimeOnly(8, 0)).WithEnd(new TimeOnly(9, 0)).Build())
                .Build();
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();
        }

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
        var assertContext = CreateContext(out var assertScope);
        using (assertScope)
        {
            var savedBooking = await assertContext.Bookings.FindAsync(newBooking.Id);
            savedBooking.Should().NotBeNull();
        }
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
        var mockContext = new Mock<BookingsDbContext>(new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options) { CallBase = true };
        
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

        var mockContext = new Mock<BookingsDbContext>(new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase("ErrorTest_" + Guid.NewGuid())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options) { CallBase = true };

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PostgresException("Connection error", "Error", "22P02", null!));

        var serviceWithError = new BookingCommandService(mockContext.Object, _loggerMock.Object, _localizerMock.Object);

        // Act & Assert
        Func<Task> act = () => serviceWithError.AddIfNoOverlapAsync(booking);
        await act.Should().ThrowAsync<PostgresException>();
    }
}
