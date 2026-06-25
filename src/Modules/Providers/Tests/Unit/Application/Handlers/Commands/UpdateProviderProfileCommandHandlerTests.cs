using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
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
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);

        var businessProfileDto = new BusinessProfileDtoBuilder()
            .WithLegalName("Prestador Atualizado Ltda")
            .WithFantasyName("Prestador Atualizado")
            .WithDescription("Prestador de serviços especializados")
            .WithEmail("contato@prestador-atualizado.com")
            .Build();

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            Services: null,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
        result.Value.Name.Should().Be("Prestador Atualizado");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        var businessProfileDto = new BusinessProfileDtoBuilder()
            .WithLegalName("Prestador Ltda")
            .WithFantasyName("Prestador")
            .WithDescription("Descrição atualizada")
            .WithEmail("contato@prestador.com")
            .Build();

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            Services: null,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ProviderNotFound);

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidName_ShouldThrow(string invalidName)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);

        var businessProfileDto = new BusinessProfileDtoBuilder()
            .WithLegalName("Prestador Ltda")
            .WithFantasyName("Prestador")
            .WithDescription("Descrição válida")
            .WithEmail("contato@prestador.com")
            .Build();

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: invalidName,
            BusinessProfile: businessProfileDto,
            Services: null,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act & Assert
        await Assert.ThrowsAsync<ProviderDomainException>(() => _handler.HandleAsync(command, CancellationToken.None));

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        var businessProfileDto = new BusinessProfileDtoBuilder()
            .WithLegalName("Prestador Ltda")
            .WithFantasyName("Prestador")
            .WithDescription("Descrição")
            .WithEmail("contato@prestador.com")
            .Build();

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            Services: null,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithServices_ShouldUpdateServices()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        Provider provider = ProviderBuilder.Create().WithId(providerId);

        var businessProfileDto = new BusinessProfileDtoBuilder()
            .WithLegalName("Prestador")
            .WithFantasyName("Prestador")
            .WithDescription("Descrição")
            .WithEmail("test@test.com")
            .Build();

        var servicesList = new List<ProviderServiceDto>
        {
            new(Guid.NewGuid(), "Service A"),
            new(Guid.NewGuid(), "Service B")
        };

        var command = new UpdateProviderProfileCommand(
            ProviderId: providerId,
            Name: "Prestador Atualizado",
            BusinessProfile: businessProfileDto,
            Services: servicesList,
            UpdatedBy: updatedBy.ToString()
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        provider.Services.Should().HaveCount(2);
        provider.Services.Select(s => s.ServiceName).Should().Contain(new[] { "Service A", "Service B" });
        provider.Services.Select(s => s.ServiceId).Should().Contain(servicesList.Select(s => s.ServiceId));

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}