using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;

using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class CreateProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<CreateProviderCommandHandler>> _loggerMock;
    private readonly CreateProviderCommandHandler _handler;

    public CreateProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<CreateProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new CreateProviderCommandHandler(_uowMock.Object, _loggerMock.Object);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);

        _providerRepositoryMock.Verify(
            x => x.Add(It.IsAny<Provider>()),
            Times.Once);
        _uowMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
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
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.AlreadyExists);

        _providerRepositoryMock.Verify(
            x => x.Add(It.IsAny<Provider>()),
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
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
