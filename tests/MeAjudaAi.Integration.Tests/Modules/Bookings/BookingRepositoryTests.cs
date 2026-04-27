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
    public async Task GetByProviderIdPagedAsync_ShouldHaveDeterministicOrdering()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2026, 5, 20);
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(11, 0);
        
        // Criamos agendamentos com exatamente o mesmo dia e horário
        var b1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, TimeSlot.Create(startTime, endTime));
        var b2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, TimeSlot.Create(startTime, endTime));
        var b3 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, TimeSlot.Create(startTime, endTime));

        _context.Bookings.AddRange(b1, b2, b3);
        await _context.SaveChangesAsync();

        // Act
        var (items, _) = await _repository.GetByProviderIdPagedAsync(providerId, null, null, 1, 10);

        // Assert
        // A ordenação deve seguir Date DESC, Start DESC, Id ASC (alfabético/GUID)
        var sortedIds = new[] { b1.Id, b2.Id, b3.Id }.OrderBy(id => id).ToList();
        items.Select(i => i.Id).Should().Equal(sortedIds);
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

    [Fact]
    public async Task AddIfNoOverlapAsync_WithNonUtcTimeZones_ShouldDetectOverlapCorrectly()
    {
        // Este teste simula a lógica do CreateBookingCommandHandler usando um fuso específico
        // Arrange
        var providerId = Guid.NewGuid();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); // -03:00/-02:00
        
        // 10:00 AM Local em São Paulo no dia 20/05/2026 (Inverno -03:00)
        // 10:00 Local = 13:00 UTC
        var startUtc1 = new DateTimeOffset(2026, 5, 20, 13, 0, 0, TimeSpan.Zero);
        var endUtc1 = startUtc1.AddHours(1);

        // Convertemos para os valores do agregado
        var localStart1 = TimeZoneInfo.ConvertTime(startUtc1, tz);
        var localEnd1 = TimeZoneInfo.ConvertTime(endUtc1, tz);
        var date1 = DateOnly.FromDateTime(localStart1.DateTime);
        var slot1 = TimeSlot.Create(TimeOnly.FromDateTime(localStart1.DateTime), TimeOnly.FromDateTime(localEnd1.DateTime));
        
        var booking1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date1, slot1);
        
        // Persiste o primeiro
        (await _repository.AddIfNoOverlapAsync(booking1)).IsSuccess.Should().BeTrue();

        // Tenta um segundo agendamento que sobrepõe (ex: 10:30 Local = 13:30 UTC)
        var startUtc2 = new DateTimeOffset(2026, 5, 20, 13, 30, 0, TimeSpan.Zero);
        var endUtc2 = startUtc2.AddHours(1);
        
        var localStart2 = TimeZoneInfo.ConvertTime(startUtc2, tz);
        var localEnd2 = TimeZoneInfo.ConvertTime(endUtc2, tz);
        var date2 = DateOnly.FromDateTime(localStart2.DateTime);
        var slot2 = TimeSlot.Create(TimeOnly.FromDateTime(localStart2.DateTime), TimeOnly.FromDateTime(localEnd2.DateTime));
        
        var booking2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date2, slot2);

        // Act
        var result = await _repository.AddIfNoOverlapAsync(booking2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("booking_overlap");
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_OnDSTTransition_ShouldHandleCorrectly()
    {
        // Arrange
        // Em 2024, PST (Pacific) volta o relógio em 3 de Novembro (ambiguidade 01:00-02:00)
        var providerId = Guid.NewGuid();
        var date = new DateOnly(2024, 11, 3);
        
        // Primeiro agendamento: 01:00 às 01:30 (PST)
        var slot1 = TimeSlot.Create(new TimeOnly(1, 0), new TimeOnly(1, 30));
        var booking1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, slot1);
        (await _repository.AddIfNoOverlapAsync(booking1)).IsSuccess.Should().BeTrue();

        // Segundo agendamento: 01:15 às 01:45 (Conflito direto no local time, independente do offset)
        var slot2 = TimeSlot.Create(new TimeOnly(1, 15), new TimeOnly(1, 45));
        var booking2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), date, slot2);

        // Act
        var result = await _repository.AddIfNoOverlapAsync(booking2);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_WhenBookingStraddlesMidnight_ShouldHandleCorrectly()
    {
        // Embora nossa regra de negócio atual (no Aggregate) proíba agendamentos que cruzam meia-noite,
        // o repositório deve ser capaz de lidar com isso se for persistido.
        // Na verdade, oAggregate lança exceção, então testamos se dois agendamentos em dias adjacentes NÃO conflitam.
        
        // Arrange
        var providerId = Guid.NewGuid();
        var day1 = new DateOnly(2026, 5, 20);
        var day2 = new DateOnly(2026, 5, 21);
        
        // Agendamento no final do dia 1
        var booking1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), day1, 
            TimeSlot.Create(new TimeOnly(23, 0), new TimeOnly(23, 59, 59)));
        
        // Agendamento no início do dia 2
        var booking2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), day2, 
            TimeSlot.Create(new TimeOnly(0, 0), new TimeOnly(1, 0)));

        // Act
        var res1 = await _repository.AddIfNoOverlapAsync(booking1);
        var res2 = await _repository.AddIfNoOverlapAsync(booking2);

        // Assert
        res1.IsSuccess.Should().BeTrue();
        res2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddIfNoOverlapAsync_WithNonUtcTimeZones_ShouldAllowDifferentLocalDays()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); // UTC-3
        
        // 23:00 Local em SP em 20/05/2026 = 02:00 UTC em 21/05/2026
        var startUtc1 = new DateTimeOffset(2026, 5, 21, 2, 0, 0, TimeSpan.Zero);
        var localStart1 = TimeZoneInfo.ConvertTime(startUtc1, tz);
        var booking1 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), 
            DateOnly.FromDateTime(localStart1.DateTime), 
            TimeSlot.Create(new TimeOnly(23, 0), new TimeOnly(23, 30)));
        
        // 01:00 Local em SP em 21/05/2026 = 04:00 UTC em 21/05/2026
        // Mesmo dia UTC, mas dias locais DIFERENTES (20 vs 21)
        var startUtc2 = new DateTimeOffset(2026, 5, 21, 4, 0, 0, TimeSpan.Zero);
        var localStart2 = TimeZoneInfo.ConvertTime(startUtc2, tz);
        var booking2 = Booking.Create(providerId, Guid.NewGuid(), Guid.NewGuid(), 
            DateOnly.FromDateTime(localStart2.DateTime), 
            TimeSlot.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)));

        // Act
        var res1 = await _repository.AddIfNoOverlapAsync(booking1);
        var res2 = await _repository.AddIfNoOverlapAsync(booking2);

        // Assert
        res1.IsSuccess.Should().BeTrue();
        res2.IsSuccess.Should().BeTrue();
        booking1.Date.Should().NotBe(booking2.Date);
    }
}
