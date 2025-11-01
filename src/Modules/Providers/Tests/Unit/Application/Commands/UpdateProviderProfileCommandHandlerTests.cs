using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class UpdateProviderProfileCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<UpdateProviderProfileCommandHandler>> _loggerMock;
    private readonly UpdateProviderProfileCommandHandler _handler;

    public UpdateProviderProfileCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<UpdateProviderProfileCommandHandler>>();
        _handler = new UpdateProviderProfileCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);

        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Prestador Atualizado Ltda",
            FantasyName: "Prestador Atualizado",
            Description: "Prestador de serviços especializados",
            ContactInfo: new ContactInfoDto(
                Email: "contato@prestador-atualizado.com",
                PhoneNumber: "(11) 99999-8888",
                Website: "https://www.exemplo-atualizado.com"
            ),
            PrimaryAddress: new AddressDto(
                Street: "Rua Atualizada",
                Number: "456",
                Complement: "Sala 10",
                Neighborhood: "Bairro Novo",
                City: "São Paulo",
                State: "SP",
                ZipCode: "01234-567",
                Country: "Brasil"
            )
        );

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
        result.Value.Name.Should().Be("Prestador Atualizado");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Prestador Ltda",
            FantasyName: "Prestador",
            Description: "Descrição atualizada",
            ContactInfo: new ContactInfoDto(
                Email: "contato@prestador.com",
                PhoneNumber: "(11) 99999-9999",
                Website: "https://www.exemplo.com"
            ),
            PrimaryAddress: new AddressDto(
                Street: "Rua Exemplo",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "São Paulo",
                State: "SP",
                ZipCode: "01234-567",
                Country: "Brasil"
            )
        );

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Provider not found");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidName_ShouldReturnFailureResult(string invalidName)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);

        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Prestador Ltda",
            FantasyName: "Prestador",
            Description: "Descrição válida",
            ContactInfo: new ContactInfoDto(
                Email: "contato@prestador.com",
                PhoneNumber: "(11) 99999-9999",
                Website: "https://www.exemplo.com"
            ),
            PrimaryAddress: new AddressDto(
                Street: "Rua Exemplo",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "São Paulo",
                State: "SP",
                ZipCode: "01234-567",
                Country: "Brasil"
            )
        );

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: invalidName,
            BusinessProfile: businessProfileDto,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error updating provider profile");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        var businessProfileDto = new BusinessProfileDto(
            LegalName: "Prestador Ltda",
            FantasyName: "Prestador",
            Description: "Descrição",
            ContactInfo: new ContactInfoDto(
                Email: "contato@prestador.com",
                PhoneNumber: "(11) 99999-9999",
                Website: "https://www.exemplo.com"
            ),
            PrimaryAddress: new AddressDto(
                Street: "Rua Exemplo",
                Number: "123",
                Complement: null,
                Neighborhood: "Centro",
                City: "São Paulo",
                State: "SP",
                ZipCode: "01234-567",
                Country: "Brasil"
            )
        );

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error updating provider profile");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
