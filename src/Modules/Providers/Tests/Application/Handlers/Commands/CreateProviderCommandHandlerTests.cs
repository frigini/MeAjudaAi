using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Commands;

using MeAjudaAi.Modules.Providers.Application.Queries;

// ...

public class CreateProviderCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<CreateProviderCommandHandler>> _loggerMock;
    private readonly CreateProviderCommandHandler _handler;

    public CreateProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<CreateProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new CreateProviderCommandHandler(_uowMock.Object, _providerQueriesMock.Object, _loggerMock.Object);
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

        _providerQueriesMock.Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

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

        _providerQueriesMock.Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Provider Name");
        result.Value.Type.Should().Be(EProviderType.Individual);

        _providerRepositoryMock.Verify(r => r.Add(It.IsAny<Provider>()), Times.Once);
        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _providerQueriesMock.Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.CreationError);
    }
}


