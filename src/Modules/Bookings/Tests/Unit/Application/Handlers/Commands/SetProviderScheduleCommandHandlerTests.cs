using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Handlers.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Validators;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Bookings;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers.Commands;

public class SetProviderScheduleCommandHandlerTests
{
    private readonly Mock<IProviderScheduleQueries> _scheduleQueriesMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRepository<ProviderSchedule, Guid>> _repoMock = new();
    private readonly Mock<IProvidersModuleApi> _providersApiMock = new();
    private readonly Mock<ILogger<SetProviderScheduleCommandHandler>> _loggerMock = new();
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock = new();
    private readonly SetProviderScheduleCommandHandler _sut;

    public SetProviderScheduleCommandHandlerTests()
    {
        _uowMock.Setup(x => x.GetRepository<ProviderSchedule, Guid>()).Returns(_repoMock.Object);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _localizerMock.Setup(x => x[It.Is<string>(s => s == "ProviderNotFound")])
            .Returns(new LocalizedString("ProviderNotFound", "Prestador não encontrado."));
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "ProviderScheduleDataInvalid")])
            .Returns(new LocalizedString("ProviderScheduleDataInvalid", "Os dados de horário fornecidos são inválidos. Verifique sobreposições ou horários negativos."));

        _sut = new SetProviderScheduleCommandHandler(
            _scheduleQueriesMock.Object,
            _uowMock.Object,
            _providersApiMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Succeed_When_Valid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto> 
            { 
                new(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(12)) 
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderScheduleBuilder().WithProviderId(providerId).Build());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.Add(It.IsAny<ProviderSchedule>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetProviderScheduleCommand(providerId, [], Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error!.Message.Should().Be("Prestador não encontrado.");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProvidersApi_Returns_Failure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetProviderScheduleCommand(providerId, [], Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(Error.Internal("Api failure")));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Api failure");
        result.Error!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void Should_Fail_When_Availabilities_Is_Null()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetProviderScheduleCommand(providerId, null!, Guid.NewGuid());

        // Act
        var validator = new SetProviderScheduleCommandValidator();
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetProviderScheduleCommand.Availabilities));
    }

    [Fact]
    public void Should_Fail_When_Availabilities_Contains_Null()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto> 
            { 
                new(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(12)) 
            }),
            null!
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        // Act
        var validator = new SetProviderScheduleCommandValidator();
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Item de disponibilidade não pode ser nulo.");
    }

    [Fact]
    public async Task HandleAsync_Should_Call_Add_When_Schedule_Does_Not_Exist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto> 
            { 
                new(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(12)) 
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(x => x.Add(It.IsAny<ProviderSchedule>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_TimeSlot_Is_Invalid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto> 
            { 
                new(DateTimeOffset.UtcNow.Date.AddHours(12), DateTimeOffset.UtcNow.Date.AddHours(8)) // Início > Fim
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Os dados de horário fornecidos são inválidos. Verifique sobreposições ou horários negativos.");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_TimeSlots_Overlap()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto> 
            { 
                new(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(12)),
                new(DateTimeOffset.UtcNow.Date.AddHours(11), DateTimeOffset.UtcNow.Date.AddHours(14)) // Sobreposição
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Os dados de horário fornecidos são inválidos. Verifique sobreposições ou horários negativos.");
    }

    [Fact]
    public async Task HandleAsync_WhenConflictOnAdd_ShouldFallbackToUpdate()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(12))
            })
        };

        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var existingSchedule = new ProviderScheduleBuilder()
            .WithProviderId(providerId)
            .WithTimeZoneId("UTC")
            .Build();
        _scheduleQueriesMock.SetupSequence(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null)
            .ReturnsAsync(existingSchedule);

        _uowMock.SetupSequence(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("conflict"))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesThrows_ShouldReturnInternalError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<AvailableSlotDto>
            {
                new(DateTimeOffset.UtcNow.Date.AddHours(8), DateTimeOffset.UtcNow.Date.AddHours(12))
            })
        };

        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderScheduleBuilder().WithProviderId(providerId).Build());

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.HandleAsync(command));
    }
}