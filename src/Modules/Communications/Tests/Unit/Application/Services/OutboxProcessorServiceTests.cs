using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Application.Services.Outbox;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Services;

public class SerializerMockBuilder
{
    public Mock<ISerializer> Mock { get; } = new Mock<ISerializer>();

    public SerializerMockBuilder SetupDefault()
    {
        Mock.Setup(x => x.Serialize(It.IsAny<It.IsAnyType>())).Returns("dummy_payload");

        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(new EmailOutboxPayload("test@test.com", "Hi", "Hello", null, null, null));
        Mock.Setup(x => x.Deserialize<SmsOutboxPayload>(It.IsAny<string>()))
            .Returns(new SmsOutboxPayload("+5511999999999", "Hello"));
        Mock.Setup(x => x.Deserialize<PushOutboxPayload>(It.IsAny<string>()))
            .Returns(new PushOutboxPayload("token123", "Hi", "Hello", null));

        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("html_payload"))
            .Returns(new EmailOutboxPayload("t@t.com", "S", null, null, null, null, "welcome_template", new Dictionary<string, string> { { "FirstName", "John" } }));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("body_payload"))
            .Returns(new EmailOutboxPayload("t@t.com", "S", null, "T", "<b>B</b>", null, null, null));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("raw_body_payload"))
            .Returns(new EmailOutboxPayload("t@t.com", "S", "Raw Body", null, null, null, "non_existent", null));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("html_body_payload"))
            .Returns(new EmailOutboxPayload("t@t.com", "S", "<h1>H</h1>", "T", null, null, null, null));

        return this;
    }

    public ISerializer Build() => Mock.Object;
}


public class OutboxProcessorServiceTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<CommunicationLog, Guid>> _logRepositoryMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<ISmsSender> _smsSenderMock;
    private readonly Mock<IPushSender> _pushSenderMock;
    private readonly Mock<ILogger<OutboxProcessorService>> _loggerMock;
    private readonly Mock<ISerializer> _serializeMock;
    private readonly Mock<IEmailTemplateQueries> _emailTemplateQueriesMock;
    private readonly OutboxProcessorService _service;

    public OutboxProcessorServiceTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _logRepositoryMock = new Mock<IRepository<CommunicationLog, Guid>>();
        _uowMock.Setup(u => u.GetRepository<CommunicationLog, Guid>()).Returns(_logRepositoryMock.Object);
        _emailSenderMock = new Mock<IEmailSender>();
        _smsSenderMock = new Mock<ISmsSender>();
        _pushSenderMock = new Mock<IPushSender>();
        _loggerMock = new Mock<ILogger<OutboxProcessorService>>();
        _emailTemplateQueriesMock = new Mock<IEmailTemplateQueries>();
        
        var builder = new SerializerMockBuilder().SetupDefault();
        _serializeMock = builder.Mock;

        _service = new OutboxProcessorService(
            _outboxRepositoryMock.Object,
            _uowMock.Object,
            _emailSenderMock.Object,
            _smsSenderMock.Object,
            _pushSenderMock.Object,
            builder.Build(),
            _emailTemplateQueriesMock.Object,
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
        var payload = "serialized-email-payload";
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
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => l.IsSuccess)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenDispatchFails_ShouldMarkAsFailedAndRetryLater()
    {
        // Arrange
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
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
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
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
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => !l.IsSuccess)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenOperationCanceledDuringDispatch_ShouldReturnZeroAndStayPending()
    {
        // Arrange
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
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
    public async Task ProcessPendingMessagesAsync_WhenSmsSuccess_ShouldMarkAsSentAndLog()
    {
        // Arrange
        var payload = _serializeMock.Object.Serialize(new { PhoneNumber = "+5511999999999", Body = "Hello" });
        var message = OutboxMessage.Create(ECommunicationChannel.Sms, payload);
        
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _smsSenderMock.Setup(x => x.SendAsync(It.IsAny<SmsMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        _smsSenderMock.Verify(x => x.SendAsync(It.IsAny<SmsMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => l.IsSuccess)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenPushSuccess_ShouldMarkAsSentAndLog()
    {
        // Arrange
        var payload = _serializeMock.Object.Serialize(new { DeviceToken = "token123", Title = "Hi", Body = "Hello" });
        var message = OutboxMessage.Create(ECommunicationChannel.Push, payload);
        
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _pushSenderMock.Setup(x => x.SendAsync(It.IsAny<PushNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        _pushSenderMock.Verify(x => x.SendAsync(It.IsAny<PushNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => l.IsSuccess)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenExceptionInDispatch_ShouldFailAndLog()
    {
        // Arrange
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        // Usar maxRetries: 1 para que a falha atinja o limite e gere o log
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload, maxRetries: 1);

        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _service.ProcessPendingMessagesAsync();

        // Assert
        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => !l.IsSuccess)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithHtmlBody_ShouldUseIt()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "html_body_payload");
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
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "body_payload");
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody.Contains("&lt;b&gt;")), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmailWithTemplate_ShouldRenderTokens()
    {
        // Arrange
        var templateKey = "welcome_template";
        var payload = "html_payload";
        var message = OutboxMessage.Create(ECommunicationChannel.Email, payload);
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        var template = EmailTemplate.Create(templateKey, "S", "Hello {{FirstName}}!", "Hello {{FirstName}}!");
        _emailTemplateQueriesMock.Setup(x => x.GetActiveByKeyAsync(templateKey, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody == "Hello John!"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenTemplateNotFound_ShouldFallbackToRawPayload()
    {
        // Arrange
        var message = OutboxMessage.Create(ECommunicationChannel.Email, "raw_body_payload");
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { message });

        _emailTemplateQueriesMock.Setup(x => x.GetActiveByKeyAsync("non_existent", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody.Contains("Raw Body")), It.IsAny<CancellationToken>()));
    }
}

