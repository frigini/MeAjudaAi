using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;
using DTOs = MeAjudaAi.Modules.Providers.Application.DTOs;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class CreateProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<CreateProviderCommandHandler>> _loggerMock;
    private readonly CreateProviderCommandHandler _handler;

    public CreateProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<CreateProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new CreateProviderCommandHandler(_uowMock.Object, _providerQueriesMock.Object, _loggerMock.Object);
    }

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

        _providerQueriesMock
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test Provider");

        _providerRepositoryMock.Verify(
            x => x.Add(It.Is<Provider>(p => p.UserId == userId)),
            Times.Once);

        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
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

        _providerQueriesMock
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.AlreadyExists);

        _providerRepositoryMock.Verify(
            x => x.Add(It.IsAny<Provider>()),
            Times.Never);

        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProviderCommand(
            UserId: userId,
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: CreateTestBusinessProfile());

        _providerQueriesMock
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    private static DTOs.BusinessProfileDto CreateTestBusinessProfile()
    {
        return new DTOs.BusinessProfileDto(
            LegalName: "Test Legal Name",
            FantasyName: "Test Fantasy Name",
            Description: "Test Description",
            ContactInfo: new DTOs.ContactInfoDto(
                Email: "test@provider.com",
                PhoneNumber: "+5511999999999",
                Website: "https://test.com"),
            PrimaryAddress: new DTOs.AddressDto(
                Street: "Test Street",
                Number: "123",
                Complement: null,
                Neighborhood: "Test Neighborhood",
                City: "Test City",
                State: "TS",
                ZipCode: "12345678",
                Country: "Brasil")
        );
    }
}