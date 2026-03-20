using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class ActivateProviderProfileCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<ActivateProviderProfileCommandHandler>> _loggerMock;
    private readonly ActivateProviderProfileCommandHandler _sut;

    public ActivateProviderProfileCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<ActivateProviderProfileCommandHandler>>();
        _sut = new ActivateProviderProfileCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidProviderId_ShouldActivateAndReturnSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new ActivateProviderProfileCommand(providerId);
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.DeactivateProfile(); // Ensure it starts inactive for the test
        provider.ClearDomainEvents();

        _providerRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(provider);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.IsActive.Should().BeTrue();
        _providerRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Provider>(p => p.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new ActivateProviderProfileCommand(Guid.NewGuid());

        _providerRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("not found");
        _providerRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var command = new ActivateProviderProfileCommand(Guid.NewGuid());
        var provider = new ProviderBuilder().WithId(command.ProviderId).Build();

        _providerRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(provider);
        _providerRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                               .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
