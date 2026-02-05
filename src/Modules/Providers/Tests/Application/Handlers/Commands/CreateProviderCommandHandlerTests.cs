using FluentAssertions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Commands;

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

    [Fact]
    public async Task HandleAsync_WhenProviderAlreadyExistsForUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            userId,
            "Provider Name",
            EProviderType.Individual,
            new BusinessProfileDto(
                "Legal Name",
                "Fantasy Name",
                "Description",
                new ContactInfoDto("test@email.com", "1234567890", "site.com"),
                new AddressDto("Street", "123", "Comp", "Neighborhood", "City", "ST", "12345678", "Country")
            )
        );

        var existingProvider = new Provider(
            new ProviderId(Guid.NewGuid()),
            userId,
            "Existing Name",
            EProviderType.Individual,
            new BusinessProfile("Existing Legal", new ContactInfo("exist@email.com"), new Address("Rua", "1", "Bairro", "Cidade", "ES", "CEP"))
        );

        _providerRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.AlreadyExists);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldCreateProviderAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            userId,
            "Provider Name",
            EProviderType.Individual,
            new BusinessProfileDto(
                "Legal Name",
                "Fantasy Name",
                "Description",
                new ContactInfoDto("test@email.com", "1234567890", "site.com"),
                new AddressDto("Street", "123", "Comp", "Neighborhood", "City", "ST", "12345678", "Country")
            )
        );

        _providerRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Provider Name");
        result.Value.Type.Should().Be(EProviderType.Individual);
        
        _providerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            userId,
            "Provider Name",
            EProviderType.Individual,
            new BusinessProfileDto(
                "Legal Name",
                "Fantasy Name",
                "Description",
                new ContactInfoDto("test@email.com", "1234567890", "site.com"),
                new AddressDto("Street", "123", "Comp", "Neighborhood", "City", "ST", "12345678", "Country")
            )
        );

        _providerRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.CreationError);
    }
}
