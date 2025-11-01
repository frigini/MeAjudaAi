using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
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
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Test Company",
            FantasyName: null,
            Description: null,
            ContactInfo: new ContactInfoDto(
                Email: "test@provider.com",
                PhoneNumber: "+55 11 99999-9999",
                Website: "https://www.provider.com"
            ),
            PrimaryAddress: new AddressDto(
                Street: "Test Street",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "Test City",
                State: "TS",
                ZipCode: "12345-678",
                Country: "Brasil"
            )
        );

        var command = new CreateProviderCommand(
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: businessProfileDto
        );

        _providerRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providerRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(command.UserId);
        result.Value.Name.Should().Be(command.Name);
        result.Value.Type.Should().Be(command.Type);

        _providerRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderAlreadyExists_ShouldReturnFailureResult()
    {
        // Arrange
        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Test Company",
            FantasyName: null,
            Description: null,
            ContactInfo: new ContactInfoDto(
                Email: "test@provider.com",
                PhoneNumber: "+55 11 99999-9999",
                Website: null
            ),
            PrimaryAddress: new AddressDto(
                Street: "Test Street",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "Test City",
                State: "TS",
                ZipCode: "12345-678",
                Country: "Brasil"
            )
        );

        var command = new CreateProviderCommand(
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: businessProfileDto
        );

        _providerRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderBuilder.Create());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Provider already exists");

        _providerRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Test Company",
            FantasyName: null,
            Description: null,
            ContactInfo: new ContactInfoDto(
                Email: "test@provider.com",
                PhoneNumber: "+55 11 99999-9999",
                Website: null
            ),
            PrimaryAddress: new AddressDto(
                Street: "Test Street",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "Test City",
                State: "TS",
                ZipCode: "12345-678",
                Country: "Brasil"
            )
        );

        var command = new CreateProviderCommand(
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: businessProfileDto
        );

        _providerRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _providerRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error creating provider");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task HandleAsync_WithInvalidName_ShouldReturnFailureResult(string? invalidName)
    {
        // Arrange
        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Test Company",
            FantasyName: null,
            Description: null,
            ContactInfo: new ContactInfoDto(
                Email: "test@provider.com",
                PhoneNumber: "+55 11 99999-9999",
                Website: null
            ),
            PrimaryAddress: new AddressDto(
                Street: "Test Street",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "Test City",
                State: "TS",
                ZipCode: "12345-678",
                Country: "Brasil"
            )
        );

        var command = new CreateProviderCommand(
            UserId: Guid.NewGuid(),
            Name: invalidName!,
            Type: EProviderType.Individual,
            BusinessProfile: businessProfileDto
        );

        _providerRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error creating provider");

        _providerRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
