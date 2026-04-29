using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Events;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Shared.Streaming;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Events;

public class ProviderVerificationStatusUpdatedSseHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPublishVerificationStatusToHub()
    {
        var sseHubMock = new Mock<ISseHub<ProviderVerificationSseDto>>();
        sseHubMock
            .Setup(h => h.PublishAsync(It.IsAny<string>(), It.IsAny<ProviderVerificationSseDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logger = Mock.Of<ILogger<ProviderVerificationStatusUpdatedSseHandler>>();
        var sut = new ProviderVerificationStatusUpdatedSseHandler(sseHubMock.Object, logger);

        var providerId = Guid.NewGuid();
        var @event = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId,
            1,
            EVerificationStatus.Pending,
            EVerificationStatus.Verified,
            "admin");

        await sut.HandleAsync(@event);

        sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForProviderVerification(providerId),
            It.Is<ProviderVerificationSseDto>(d => d.ProviderId == providerId && d.Status == "Verified"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishRejectedStatus()
    {
        var sseHubMock = new Mock<ISseHub<ProviderVerificationSseDto>>();
        sseHubMock
            .Setup(h => h.PublishAsync(It.IsAny<string>(), It.IsAny<ProviderVerificationSseDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logger = Mock.Of<ILogger<ProviderVerificationStatusUpdatedSseHandler>>();
        var sut = new ProviderVerificationStatusUpdatedSseHandler(sseHubMock.Object, logger);

        var providerId = Guid.NewGuid();
        var @event = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId,
            1,
            EVerificationStatus.InProgress,
            EVerificationStatus.Rejected,
            "system");

        await sut.HandleAsync(@event);

        sseHubMock.Verify(h => h.PublishAsync(
            SseTopic.ForProviderVerification(providerId),
            It.Is<ProviderVerificationSseDto>(d => d.Status == "Rejected"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}