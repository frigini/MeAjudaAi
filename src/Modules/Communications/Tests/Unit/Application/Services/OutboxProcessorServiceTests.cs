using MeAjudaAi.Modules.Communications.Application.Services;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using MeAjudaAi.Contracts.Shared;
using FluentAssertions;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Services;

public class OutboxProcessorServiceTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<ICommunicationLogRepository> _logRepositoryMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<ISmsSender> _smsSenderMock;
    private readonly Mock<IPushSender> _pushSenderMock;
    private readonly Mock<ILogger<OutboxProcessorService>> _loggerMock;
    private readonly OutboxProcessorService _service;

    public OutboxProcessorServiceTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _logRepositoryMock = new Mock<ICommunicationLogRepository>();
        _emailSenderMock = new Mock<IEmailSender>();
        _smsSenderMock = new Mock<ISmsSender>();
        _pushSenderMock = new Mock<IPushSender>();
        _loggerMock = new Mock<ILogger<OutboxProcessorService>>();

        _service = new OutboxProcessorService(
            _outboxRepositoryMock.Object,
            _logRepositoryMock.Object,
            _emailSenderMock.Object,
            _smsSenderMock.Object,
            _pushSenderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenNoMessages_ShouldReturnZero()
    {
        // Arrange
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage>());

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(0);
        _outboxRepositoryMock.Verify(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmailSuccess_ShouldMarkAsSentAndLog()
    {
        // Arrange
        var payload = JsonSerializer.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload);
        
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _logRepositoryMock.Verify(x => x.AddAsync(It.Is<CommunicationLog>(l => l.IsSuccess), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenDispatchFails_ShouldMarkAsFailedAndRetryLater()
    {
        // Arrange
        var payload = JsonSerializer.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload, maxRetries: 3);

        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        message.RetryCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenMaxRetriesReached_ShouldMarkAsFailedAndLog()
    {
        // Arrange
        var payload = JsonSerializer.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload, maxRetries: 1);

        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        _logRepositoryMock.Verify(x => x.AddAsync(It.Is<CommunicationLog>(l => !l.IsSuccess), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenOperationCanceledDuringDispatch_ShouldReturnZeroAndStayPending()
    {
        // Arrange
        var payload = JsonSerializer.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload);
        var cts = new CancellationTokenSource();

        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(async (EmailMessage msg, CancellationToken ct) => 
            {
                cts.Cancel(); // Cancel while dispatching
                await Task.Yield();
                throw new OperationCanceledException(ct);
            });

        // Act
        var result = await _service.ProcessPendingMessagesAsync(cancellationToken: cts.Token);

        // Assert
        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenUnknownChannel_ShouldFail()
    {
        // Arrange
        var message = OutboxMessage.Create((ECommunicationChannel)99, "{}");
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(0);
        message.ErrorMessage.Should().Contain("Unknown channel");
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenInvalidPayload_ShouldFail()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "invalid-json");
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithHtmlBody_ShouldUseIt()
    {
        // Arrange
        var payload = JsonSerializer.Serialize(new { To = "t@t.com", Subject = "S", HtmlBody = "<h1>H</h1>", TextBody = "T" });
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload);
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody == "<h1>H</h1>"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithOnlyBody_ShouldHtmlEncodeIt()
    {
        // Arrange
        var payload = JsonSerializer.Serialize(new { To = "t@t.com", Subject = "S", Body = "<b>B</b>" });
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload);
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody.Contains("&lt;b&gt;")), It.IsAny<CancellationToken>()));
    }
}
