using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Handlers;

public class SetProviderScheduleCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IProviderScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IProvidersModuleApi> _providersApiMock = new();
    private readonly Mock<ILogger<SetProviderScheduleCommandHandler>> _loggerMock = new();
    private readonly SetProviderScheduleCommandHandler _sut;

    public SetProviderScheduleCommandHandlerTests()
    {
        _sut = new SetProviderScheduleCommandHandler(
            _scheduleRepoMock.Object,
            _providersApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Succeed_When_Valid()
    {
        // Organizar
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(8, 0), new TimeOnly(12, 0)) 
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderSchedule.Create(providerId));

        // Agir
        var result = await _sut.HandleAsync(command);

        // Assertar
        result.IsSuccess.Should().BeTrue();
        _scheduleRepoMock.Verify(x => x.UpdateAsync(It.IsAny<ProviderSchedule>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProviderNotFound()
    {
        // Organizar
        var providerId = Guid.NewGuid();
        var command = new SetProviderScheduleCommand(providerId, [], Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Agir
        var result = await _sut.HandleAsync(command);

        // Assertar
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error!.Message.Should().Be("Prestador não encontrado.");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_ProvidersApi_Returns_Failure()
    {
        // Organizar
        var providerId = Guid.NewGuid();
        var command = new SetProviderScheduleCommand(providerId, [], Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(Error.Internal("Api failure")));

        // Agir
        var result = await _sut.HandleAsync(command);

        // Assertar
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Api failure");
    }

    [Fact]
    public async Task HandleAsync_Should_Call_AddAsync_When_Schedule_Does_Not_Exist()
    {
        // Organizar
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(8, 0), new TimeOnly(12, 0)) 
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _scheduleRepoMock.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProviderSchedule?)null);

        // Agir
        var result = await _sut.HandleAsync(command);

        // Assertar
        result.IsSuccess.Should().BeTrue();
        _scheduleRepoMock.Verify(x => x.AddAsync(It.IsAny<ProviderSchedule>(), It.IsAny<CancellationToken>()), Times.Once);
        _scheduleRepoMock.Verify(x => x.UpdateAsync(It.IsAny<ProviderSchedule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_TimeSlot_Is_Invalid()
    {
        // Organizar
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
        {
            new(DayOfWeek.Monday, new List<TimeSlotDto> 
            { 
                new(new TimeOnly(12, 0), new TimeOnly(8, 0)) // Início > Fim
            })
        };
        
        var command = new SetProviderScheduleCommand(providerId, availabilities, Guid.NewGuid());

        _providersApiMock.Setup(x => x.ProviderExistsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Agir
        var result = await _sut.HandleAsync(command);

        // Assertar
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Os dados de horário fornecidos são inválidos. Verifique sobreposições ou horários negativos.");
    }

    [Fact]
    public async Task HandleAsync_Should_Fail_When_TimeSlots_Overlap()
    {
        // Organizar
        var providerId = Guid.NewGuid();
        var availabilities = new List<AvailabilityDto>
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

        // Agir
        var result = await _sut.HandleAsync(command);

        // Assertar
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Os dados de horário fornecidos são inválidos. Verifique sobreposições ou horários negativos.");
    }
}
