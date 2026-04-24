using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Outbox;

public class OutboxProcessorBaseTests
{
    private readonly Mock<IOutboxRepository<OutboxMessage>> _repositoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly TestOutboxProcessor _processor;

    public OutboxProcessorBaseTests()
    {
        _repositoryMock = new Mock<IOutboxRepository<OutboxMessage>>();
        _loggerMock = new Mock<ILogger>();
        _processor = new TestOutboxProcessor(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenSuccessfulDispatch_ShouldMarkAsSent()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P");
        _repositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _processor.DispatchResultToReturn = TestOutboxProcessor.DispatchResult.Success();

        // Act
        var result = await _processor.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        _processor.OnSuccessCalled.Should().BeTrue();
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2)); // Mark processing + Mark Sent
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenDispatchFails_ShouldMarkAsFailed()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P", maxRetries: 1);
        _repositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _processor.DispatchResultToReturn = TestOutboxProcessor.DispatchResult.Failure("Error");

        // Act
        await _processor.ProcessPendingMessagesAsync();

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        _processor.OnFailureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenExceptionOccurs_ShouldHandleAndMarkAsFailed()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P", maxRetries: 3);
        _repositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _processor.ShouldThrowException = true;

        // Act
        await _processor.ProcessPendingMessagesAsync();

        // Assert
        message.Status.Should().Be(EOutboxMessageStatus.Pending); // Stay pending because retry count < max
        message.RetryCount.Should().Be(1);
        _processor.OnFailureCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenCancelled_ShouldResetProcessingMessagesToPending()
    {
        // Arrange
        var message = OutboxMessage.Create("T", "P");
        _repositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        var cts = new CancellationTokenSource();
        _processor.CancelTokenSource = cts;

        // Capturar a sequência de status da mensagem durante as chamadas de SaveChangesAsync
        var capturedStatuses = new List<EOutboxMessageStatus>();
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct => capturedStatuses.Add(message.Status))
            .Returns(Task.CompletedTask);

        // Act
        var act = () => _processor.ProcessPendingMessagesAsync(cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        
        capturedStatuses.Should().Contain(EOutboxMessageStatus.Processing, "a mensagem deve ter passado pelo estado Processing antes do cancelamento");
        message.Status.Should().Be(EOutboxMessageStatus.Pending, "a mensagem deve ter sido resetada para Pending após o cancelamento");
        
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2), 
            "deve salvar pelo menos duas vezes: uma para Processing e outra para o reset após falha/cancelamento");
    }

    // Concrete implementation for testing the abstract base
    private class TestOutboxProcessor(IOutboxRepository<OutboxMessage> repository, ILogger logger) 
        : OutboxProcessorBase<OutboxMessage>(repository, logger)
    {
        public DispatchResult DispatchResultToReturn { get; set; } = DispatchResult.Success();
        public bool ShouldThrowException { get; set; }
        public bool OnSuccessCalled { get; private set; }
        public bool OnFailureCalled { get; private set; }
        public CancellationTokenSource? CancelTokenSource { get; set; }

        protected override Task<DispatchResult> DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                CancelTokenSource.Token.ThrowIfCancellationRequested();
            }
            if (ShouldThrowException) throw new Exception("Unexpected");
            return Task.FromResult(DispatchResultToReturn);
        }

        protected override Task OnSuccessAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            OnSuccessCalled = true;
            return Task.CompletedTask;
        }

        protected override Task OnFailureAsync(OutboxMessage message, string? error, CancellationToken cancellationToken)
        {
            OnFailureCalled = true;
            return Task.CompletedTask;
        }
    }
}
