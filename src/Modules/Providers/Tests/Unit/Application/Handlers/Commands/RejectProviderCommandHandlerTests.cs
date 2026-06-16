using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class RejectProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<RejectProviderCommandHandler>> _loggerMock;
    private readonly RejectProviderCommandHandler _handler;

    public RejectProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<RejectProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new RejectProviderCommandHandler(
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldRejectProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithVerificationStatus(EVerificationStatus.Pending)
            .Build();

        var command = new RejectProviderCommand(
            providerId,
            "admin@test.com",
            "Documents not valid");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.Rejected);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Documents not valid");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Fornecedor não encontrado");

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            reason!);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Motivo da rejeição é obrigatório");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyRejectedBy_ShouldReturnFailure(string? rejectedBy)
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            rejectedBy!,
            "Documents not valid");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Responsável pela rejeição é obrigatório");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Documents not valid");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Falha ao rejeitar o fornecedor");
    }
}



