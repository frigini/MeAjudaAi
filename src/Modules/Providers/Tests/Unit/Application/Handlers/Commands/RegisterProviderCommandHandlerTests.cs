using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;

using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class RegisterProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<RegisterProviderCommandHandler>> _loggerMock;
    private readonly RegisterProviderCommandHandler _handler;

    public RegisterProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<RegisterProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new RegisterProviderCommandHandler(_uowMock.Object, _providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderAlreadyExists_ShouldReturnSuccessWithExistingProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RegisterProviderCommand(
            UserId: userId,
            Name: "Test Provider",
            Email: "test@test.com",
            PhoneNumber: "11999999999",
            Type: EProviderType.Individual,
            DocumentNumber: "12345678901");

        var existingProvider = new Provider(userId, "Existing Provider", EProviderType.Individual,
            new BusinessProfile("Legal", new ContactInfo("test@test.com", "11999999999"),
                new Address("Rua", "1", "Bairro", "Cidade", "SP", "00000-000")));

        _providerQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Existing Provider");

        _providerRepositoryMock.Verify(
            x => x.Add(It.IsAny<Provider>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateProviderAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RegisterProviderCommand(
            UserId: userId,
            Name: "New Provider",
            Email: "new@test.com",
            PhoneNumber: "11988888888",
            Type: EProviderType.Individual,
            DocumentNumber: "12345678901");

        _providerQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("New Provider");

        _providerRepositoryMock.Verify(
            x => x.Add(It.Is<Provider>(p => p.UserId == userId && p.Name == "New Provider")),
            Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

