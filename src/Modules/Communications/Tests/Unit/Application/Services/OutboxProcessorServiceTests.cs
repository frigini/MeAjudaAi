using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Application.DTOs;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Application.Services.Outbox;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Tests.Unit.TestInfrastructure;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Services;

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

    private void SetupPendingMessages(params OutboxMessage[] messages)
    {
        _outboxRepositoryMock.Setup(x => x.GetPendingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage>(messages));
    }

    private void SetupEmailSender(bool success) =>
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);
    private void SetupSmsSender(bool success) =>
        _smsSenderMock.Setup(x => x.SendAsync(It.IsAny<SmsMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);

    private void SetupPushSender(bool success) =>
        _pushSenderMock.Setup(x => x.SendAsync(It.IsAny<PushNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(success);

    private void VerifySaveChanges() =>
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    private void VerifySuccessLog() =>
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => l.IsSuccess)), Times.Once);

    private void VerifyFailureLog() =>
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => !l.IsSuccess)), Times.Once);

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenNoMessages_ShouldReturnZero()
    {
        SetupPendingMessages();

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmailSuccess_ShouldMarkAsSentAndLog()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        VerifySuccessLog();
        VerifySaveChanges();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenDispatchFails_ShouldMarkAsFailedAndRetryLater()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .WithMaxRetries(3)
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(false);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        message.RetryCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenMaxRetriesReached_ShouldMarkAsFailedAndLog()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .WithMaxRetries(1)
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(false);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        VerifyFailureLog();
        VerifySaveChanges();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenOperationCanceledDuringDispatch_ShouldReturnZeroAndStayPending()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .Build();
        var cts = new CancellationTokenSource();

        SetupPendingMessages(message);
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(async (EmailMessage msg, CancellationToken ct) =>
            {
                cts.Cancel();
                await Task.Yield();
                throw new OperationCanceledException(ct);
            });

        var result = await _service.ProcessPendingMessagesAsync(cancellationToken: cts.Token);

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenUnknownChannel_ShouldFail()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel((ECommunicationChannel)99)
            .WithPayload("{}")
            .Build();
        SetupPendingMessages(message);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.ErrorMessage.Should().Contain("Unknown channel");
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenInvalidPayload_ShouldFail()
    {
        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>("invalid-json"))
            .Throws(new System.Text.Json.JsonException("Invalid JSON"));

        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("invalid-json")
            .Build();
        SetupPendingMessages(message);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenSmsSuccess_ShouldMarkAsSentAndLog()
    {
        var payload = _serializeMock.Object.Serialize(new { PhoneNumber = "+5511999999999", Body = "Hello" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Sms)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);
        SetupSmsSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        _smsSenderMock.Verify(x => x.SendAsync(It.IsAny<SmsMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifySuccessLog();
        VerifySaveChanges();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenPushSuccess_ShouldMarkAsSentAndLog()
    {
        var payload = _serializeMock.Object.Serialize(new { DeviceToken = "token123", Title = "Hi", Body = "Hello" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Push)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);
        SetupPushSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
        _pushSenderMock.Verify(x => x.SendAsync(It.IsAny<PushNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifySuccessLog();
        VerifySaveChanges();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenExceptionInDispatch_ShouldFailAndLog()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Hi", Body = "Hello" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .WithMaxRetries(1)
            .Build();
        SetupPendingMessages(message);
        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        VerifyFailureLog();
        VerifySaveChanges();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithHtmlBody_ShouldUseIt()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("html_body_payload")
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody == "<h1>H</h1>"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithOnlyBody_ShouldConvertToHtml()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("body_payload")
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody.Contains("<b>B</b>")), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmailWithTemplate_ShouldRenderTokens()
    {
        var templateKey = "welcome_template";
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("html_payload")
            .Build();
        SetupPendingMessages(message);

        var template = new EmailTemplateBuilder()
            .WithKey(templateKey)
            .WithSubject("Olá {{FirstName}}!")
            .WithHtmlBody("Hello {{FirstName}}!")
            .WithTextBody("Olá {{FirstName}}!")
            .Build();
        _emailTemplateQueriesMock.Setup(x => x.GetActiveByKeyAsync(templateKey, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m =>
            m.Subject == "Olá John!" &&
            m.HtmlBody == "Hello John!" &&
            m.TextBody == "Olá John!"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenTemplateDataMissing_ShouldRenderWithoutTokens()
    {
        var templateKey = "welcome_template";
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("html_payload")
            .Build();
        SetupPendingMessages(message);

        var template = new EmailTemplateBuilder()
            .WithKey(templateKey)
            .WithSubject("Olá {{Name}}!")
            .WithHtmlBody("Hello {{Name}}!")
            .WithTextBody("Olá {{Name}}!")
            .Build();
        _emailTemplateQueriesMock.Setup(x => x.GetActiveByKeyAsync(templateKey, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>("html_payload"))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("t@t.com")
                .WithSubject("S")
                .AsTemplate(templateKey)
                .Build());
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m =>
            m.Subject == "Olá {{Name}}!" &&
            m.HtmlBody == "Hello {{Name}}!" &&
            m.TextBody == "Olá {{Name}}!"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenFinalRetryFails_ShouldCreateFailureCommunicationLog()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .WithMaxRetries(1)
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(false);

        await _service.ProcessPendingMessagesAsync();

        message.Status.Should().Be(EOutboxMessageStatus.Failed);
        VerifyFailureLog();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenNonFinalRetryFails_ShouldNotCreateFailureCommunicationLog()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .WithMaxRetries(3)
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(false);

        await _service.ProcessPendingMessagesAsync();

        message.Status.Should().Be(EOutboxMessageStatus.Pending);
        _logRepositoryMock.Verify(x => x.Add(It.Is<CommunicationLog>(l => !l.IsSuccess)), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenInvalidEnvelope_ShouldFallbackToLegacyPayload()
    {
        var legacyPayload = "{\"To\":\"test@test.com\",\"Subject\":\"Hi\",\"Body\":\"Hello\"}";
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(legacyPayload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<MessageEnvelope>(It.IsAny<string>()))
            .Throws(new Exception("Invalid envelope"));
        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>(legacyPayload))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("test@test.com")
                .WithSubject("Hi")
                .WithHtmlBody("Hello")
                .Build());
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.Subject == "Hi"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenTemplateNotFound_ShouldUsePayloadBody()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Sub", TemplateKey = "nonexistent" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("test@test.com")
                .WithSubject("Sub")
                .AsTemplate("nonexistent_template")
                .Build());
        _emailTemplateQueriesMock.Setup(x => x.GetActiveByKeyAsync("nonexistent_template", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);
        SetupEmailSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody == string.Empty && m.TextBody == string.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenNoTemplateKey_ShouldUsePayloadBody()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Sub", HtmlBody = "<p>Direct</p>" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("test@test.com")
                .WithSubject("Sub")
                .WithHtmlBody("<p>Direct</p>")
                .Build());
        SetupEmailSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.HtmlBody == "<p>Direct</p>" && m.Subject == "Sub"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmptySubject_ShouldUseNoSubjectFallback()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "", HtmlBody = "<p>Hi</p>" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("test@test.com")
                .WithSubject("")
                .WithHtmlBody("<p>Hi</p>")
                .Build());
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.Subject == "(no subject)"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenSmsWithLegacyPayload_ShouldFallbackAndSend()
    {
        var legacyPayload = "{\"PhoneNumber\":\"+5511999999999\",\"Body\":\"Hello\"}";
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Sms)
            .WithPayload(legacyPayload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<MessageEnvelope>(It.IsAny<string>()))
            .Throws(new Exception("Invalid envelope"));
        _serializeMock.Setup(x => x.Deserialize<SmsOutboxPayload>(legacyPayload))
            .Returns(new SmsOutboxPayloadBuilder()
                .WithPhoneNumber("+5511999999999")
                .WithBody("Hello")
                .Build());
        SetupSmsSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenPushWithLegacyPayload_ShouldFallbackAndSend()
    {
        var legacyPayload = "{\"DeviceToken\":\"token\",\"Title\":\"Hi\",\"Body\":\"Hello\"}";
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Push)
            .WithPayload(legacyPayload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<MessageEnvelope>(It.IsAny<string>()))
            .Throws(new Exception("Invalid envelope"));
        _serializeMock.Setup(x => x.Deserialize<PushOutboxPayload>(legacyPayload))
            .Returns(new PushOutboxPayloadBuilder()
                .WithDeviceToken("token")
                .WithTitle("Hi")
                .WithBody("Hello")
                .Build());
        SetupPushSender(true);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenSmsPayloadInvalid_ShouldFail()
    {
        _serializeMock.Setup(x => x.Deserialize<SmsOutboxPayload>("invalid"))
            .Throws(new System.Text.Json.JsonException("Invalid JSON"));

        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Sms)
            .WithPayload("invalid")
            .Build();
        SetupPendingMessages(message);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenPushPayloadInvalid_ShouldFail()
    {
        _serializeMock.Setup(x => x.Deserialize<PushOutboxPayload>("invalid"))
            .Throws(new System.Text.Json.JsonException("Invalid JSON"));

        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Push)
            .WithPayload("invalid")
            .Build();
        SetupPendingMessages(message);

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Pending);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenOnSuccessLogSaveFails_ShouldStillComplete()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(true);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(1);
        message.Status.Should().Be(EOutboxMessageStatus.Sent);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenOnFailureLogSaveFails_ShouldStillComplete()
    {
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("payload")
            .WithMaxRetries(1)
            .Build();
        SetupPendingMessages(message);
        SetupEmailSender(false);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _service.ProcessPendingMessagesAsync();

        result.Should().Be(0);
        message.Status.Should().Be(EOutboxMessageStatus.Failed);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmailWithTextBodyOnly_ShouldSendTextBody()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Sub", TextBody = "Plain text" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("test@test.com")
                .WithSubject("Sub")
                .WithTextBody("Plain text")
                .Build());
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.TextBody == "Plain text" && m.HtmlBody == string.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenEmailWithFromAddress_ShouldPassFromToSender()
    {
        var payload = _serializeMock.Object.Serialize(new { To = "test@test.com", Subject = "Sub", HtmlBody = "<p>Hi</p>", From = "sender@test.com" });
        var message = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload(payload)
            .Build();
        SetupPendingMessages(message);

        _serializeMock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(new EmailOutboxPayloadBuilder()
                .WithTo("test@test.com")
                .WithSubject("Sub")
                .WithHtmlBody("<p>Hi</p>")
                .WithFrom("sender@test.com")
                .Build());
        SetupEmailSender(true);

        await _service.ProcessPendingMessagesAsync();

        _emailSenderMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.From == "sender@test.com"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
