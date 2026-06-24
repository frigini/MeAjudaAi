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

public class UpdateProviderProfileCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<UpdateProviderProfileCommandHandler>> _loggerMock;
    private readonly UpdateProviderProfileCommandHandler _handler;

    public UpdateProviderProfileCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<UpdateProviderProfileCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new UpdateProviderProfileCommandHandler(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderExists_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderProfileCommand(
            providerId,
            "Updated Name",
            new BusinessProfileDto(
                "Legal Name",
                "Fantasy Name",
                "Description",
                new ContactInfoDto("test@example.com", "1234567890", "site.com"),
                new AddressDto("Street", "123", "Comp", "Neighborhood", "City", "ST", "12345678", "Country")
            ),
            new List<ProviderServiceDto>(),
            "Tester"
        );

        var existingProvider = new Provider(
            new ProviderId(providerId),
            Guid.NewGuid(),
            "Original Name",
            EProviderType.Individual,
            new BusinessProfile("Old Legal", new ContactInfo("old@email.com"), new Address("Rua", "1", "Bairro", "Cidade", "ES", "CEP"), "Old Fantasy", "Old Desc")
        );

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Updated Name");
        result.Value!.BusinessProfile.LegalName.Should().Be("Legal Name");

        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderProfileCommand(
            providerId,
            "Name",
            new BusinessProfileDto(
                "Legal", "Fantasy", "Desc",
                new ContactInfoDto("e@e.com", null, null),
                new AddressDto("S", "1", null, "N", "C", "S", "Z", "C")
            ),
            new List<ProviderServiceDto>(),
            "Tester"
        );

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ProviderNotFound);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderProfileCommand(
            providerId,
            "Name",
            new BusinessProfileDto(
                "Legal", "Fantasy", "Desc",
                new ContactInfoDto("e@e.com", null, null),
                new AddressDto("S", "1", null, "N", "C", "S", "Z", "C")
            ),
            new List<ProviderServiceDto>(),
            "Tester"
        );

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command, CancellationToken.None));
    }
}