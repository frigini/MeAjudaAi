using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class RegisterProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<RegisterProviderCommandHandler>> _loggerMock;
    private readonly RegisterProviderCommandHandler _handler;

    public RegisterProviderCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<RegisterProviderCommandHandler>>();
        _handler = new RegisterProviderCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
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
                new Address("R", "1", null, "N", "C", "SP", "00000000")));
                
        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Existing Provider");
        
        _providerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
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

        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("New Provider");
        
        _providerRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Provider>(p => p.UserId == userId && p.Name == "New Provider"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
    }

    [Fact]
    public async Task HandleAsync_WhenDomainExceptionThrows_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RegisterProviderCommand(userId, "Test Provider", "test@test.com", "11999999999", EProviderType.Individual, "12345678901");

        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);
            
        _providerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TestDomainException("Invalid state"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_WhenGenericExceptionThrows_ShouldReturnFailure500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RegisterProviderCommand(userId, "Test Provider", "test@test.com", "11999999999", EProviderType.Individual, "12345678901");

        _providerRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unknown failure"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(500);
    }
}
