using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Queries;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class SetProviderScheduleCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IProviderScheduleQueries> _scheduleQueriesMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRepository<ProviderSchedule, Guid>> _repoMock = new();
    private readonly Mock<IProvidersModuleApi> _providersApiMock = new();
    private readonly Mock<ILogger<SetProviderScheduleCommandHandler>> _loggerMock = new();
    private readonly SetProviderScheduleCommandHandler _sut;

    public SetProviderScheduleCommandHandlerTests()
    {
        _uowMock.Setup(x => x.GetRepository<ProviderSchedule, Guid>()).Returns(_repoMock.Object);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new SetProviderScheduleCommandHandler(
            _scheduleQueriesMock.Object,
            _uowMock.Object,
            _providersApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Succeed_When_Valid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<ProviderScheduleDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(8, 0), new TimeOnly(12, 0)) 
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _scheduleQueriesMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderSchedule.Create(providerId));

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
    public async Task HandleAsync_Should_Fail_When_Availabilities_Is_Null()
    {
        var providerId = Guid.NewGuid();
        var command = new SetProviderScheduleCommand(providerId, null!, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await _sut.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("A lista de disponibilidades não pode ser nula.");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_Availabilities_Contains_Null()
    {
        var providerId = Guid.NewGuid();
        var availabilities = new List<ProviderScheduleDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> { new(new TimeOnly(8, 0), new TimeOnly(12, 0)) }),
            null!
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await _sut.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Uma das disponibilidades fornecidas é nula.");
    }

    [Fact]
    public async Task HandleAsync_Should_Call_Add_When_Schedule_Does_Not_Exist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var availabilities = new List<ProviderScheduleDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(8, 0), new TimeOnly(12, 0)) 
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
        var availabilities = new List<ProviderScheduleDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(12, 0), new TimeOnly(8, 0)) // Início > Fim
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
        var availabilities = new List<ProviderScheduleDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(8, 0), new TimeOnly(12, 0)),
                new(new TimeOnly(11, 0), new TimeOnly(14, 0)) // Sobreposição
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
}



