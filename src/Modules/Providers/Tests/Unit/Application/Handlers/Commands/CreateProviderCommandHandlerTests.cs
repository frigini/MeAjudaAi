using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class CreateProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<CreateProviderCommandHandler>> _loggerMock;
    private readonly CreateProviderCommandHandler _handler;

    public CreateProviderCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<CreateProviderCommandHandler>>();
        _handler = new CreateProviderCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    private static BusinessProfileDto CreateTestBusinessProfile() =>
        new BusinessProfileDto(
            "Test Business",
            "12345678000100",
            null,
            new ContactInfoDto("test@test.com", null, null),
            new AddressDto("Main St", "100", null, "Downtown", "City", "State", "12345", "Country"));

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            UserId: userId,
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: CreateTestBusinessProfile());

        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);

        _providerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            UserId: userId,
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: CreateTestBusinessProfile());

        var existingProvider = new ProviderBuilder()
            .WithUserId(userId)
            .WithName("Existing")
            .WithType(EProviderType.Individual)
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("already exists");

        _providerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            UserId: userId,
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: CreateTestBusinessProfile());

        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
