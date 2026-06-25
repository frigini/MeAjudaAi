using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class ActivateProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<IDocumentsModuleApi> _documentsModuleApiMock;
    private readonly Mock<ILogger<ActivateProviderCommandHandler>> _loggerMock;
    private readonly ActivateProviderCommandHandler _handler;

    public ActivateProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _documentsModuleApiMock = new Mock<IDocumentsModuleApi>();
        _loggerMock = new Mock<ILogger<ActivateProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new ActivateProviderCommandHandler(
            _uowMock.Object,
            _documentsModuleApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidatedDocuments_ShouldActivateProvider()
    {
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.CompleteBasicInfo("admin@test.com");
        var command = new ActivateProviderCommand(providerId, "admin@test.com");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _documentsModuleApiMock.Setup(x => x.HasRequiredDocumentsAsync(providerId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Success(true));
        _documentsModuleApiMock.Setup(x => x.HasVerifiedDocumentsAsync(providerId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Success(true));
        _documentsModuleApiMock.Setup(x => x.HasPendingDocumentsAsync(providerId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Success(false));
        _documentsModuleApiMock.Setup(x => x.HasRejectedDocumentsAsync(providerId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Success(false));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.Active);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}