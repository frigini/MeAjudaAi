using MeAjudaAi.Modules.Communications.Application.Services;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Services;

public class CommunicationsOutboxWorkerTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IOutboxMessageRepository> _repositoryMock;
    private readonly Mock<IOutboxProcessorService> _processorMock;
    private readonly Mock<ILogger<CommunicationsOutboxWorker>> _loggerMock;

    public CommunicationsOutboxWorkerTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IOutboxMessageRepository>();
        _processorMock = new Mock<IOutboxProcessorService>();
        _loggerMock = new Mock<ILogger<CommunicationsOutboxWorker>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IOutboxMessageRepository))).Returns(_repositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IOutboxProcessorService))).Returns(_processorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldResetStaleMessagesAndProcessPending()
    {
        // Arrange
        var worker = new CommunicationsOutboxWorker(_scopeFactoryMock.Object, _loggerMock.Object);
        var cts = new CancellationTokenSource();

        _repositoryMock.Setup(x => x.ResetStaleProcessingMessagesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _processorMock.Setup(x => x.ProcessPendingMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var task = worker.StartAsync(cts.Token);
        await Task.Delay(100); // Give it some time to run the first iteration
        await worker.StopAsync(cts.Token);

        // Assert
        _repositoryMock.Verify(x => x.ResetStaleProcessingMessagesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _processorMock.Verify(x => x.ProcessPendingMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldContinue()
    {
        // Arrange
        var worker = new CommunicationsOutboxWorker(_scopeFactoryMock.Object, _loggerMock.Object);
        var cts = new CancellationTokenSource();

        _repositoryMock.Setup(x => x.ResetStaleProcessingMessagesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var task = worker.StartAsync(cts.Token);
        await Task.Delay(100);
        await worker.StopAsync(cts.Token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
