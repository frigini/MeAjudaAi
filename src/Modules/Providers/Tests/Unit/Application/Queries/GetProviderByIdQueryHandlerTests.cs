using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class GetProviderByIdQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProviderByIdQueryHandler>> _loggerMock;
    private readonly GetProviderByIdQueryHandler _handler;

    public GetProviderByIdQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProviderByIdQueryHandler>>();
        _handler = new GetProviderByIdQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ShouldReturnProviderSuccessfully()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderByIdQuery(providerId);
        var providerIdValueObject = new ProviderId(providerId);
        var provider = CreateValidProvider(providerIdValueObject);

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<ProviderId>(id => id.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);
        result.Value.Name.Should().Be(provider.Name);
        result.Value.Type.Should().Be(provider.Type);
        result.Value.VerificationStatus.Should().Be(provider.VerificationStatus);
        result.Value.BusinessProfile.Should().NotBeNull();
        result.Value.BusinessProfile.ContactInfo.Should().NotBeNull();
        result.Value.BusinessProfile.PrimaryAddress.Should().NotBeNull();

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderByIdQuery(providerId);

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
        result.Error.Message.Should().Be("Provider not found");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderByIdQuery(providerId);

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error getting provider");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Provider CreateValidProvider(ProviderId? providerId = null)
    {
        var userId = Guid.NewGuid();
        var name = "Test Provider";
        var type = EProviderType.Individual;

        var address = new Address(
            street: "Rua Teste",
            number: "123",
            neighborhood: "Centro",
            city: "São Paulo",
            state: "SP",
            zipCode: "01234-567",
            country: "Brasil");

        var contactInfo = new ContactInfo(
            email: "test@provider.com",
            phoneNumber: "+55 11 99999-9999",
            website: "https://www.provider.com");

        var businessProfile = new BusinessProfile(
            legalName: "Provider Test LTDA",
            contactInfo: contactInfo,
            primaryAddress: address);

        // Se um ProviderId específico foi fornecido, usa o construtor interno para testes
        if (providerId != null)
        {
            var testContactInfo = new ContactInfo("test@company.com", "123456789");
            var testAddress = new Address("Main St", "123", "Downtown", "Test City", "Test State", "12345");
            var testBusinessProfile = new BusinessProfile("Test Company", testContactInfo, testAddress);
            return new Provider(userId, name, type, testBusinessProfile);
        }

        // Caso contrário, usa o construtor público normal
        return new Provider(userId, name, type, businessProfile);
    }
}
