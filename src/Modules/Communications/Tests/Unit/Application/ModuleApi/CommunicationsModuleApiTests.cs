using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Application.DTOs;
using MeAjudaAi.Modules.Communications.Application.ModuleApi;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.ModuleApi;

public class CommunicationsModuleApiTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IEmailTemplateQueries> _templateQueriesMock;
    private readonly Mock<ICommunicationLogQueries> _logQueriesMock;
    private readonly Mock<ISerializer> _serializerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<CommunicationsModuleApi>> _loggerMock;
    private readonly CommunicationsModuleApi _api;

    public CommunicationsModuleApiTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _templateQueriesMock = new Mock<IEmailTemplateQueries>();
        _logQueriesMock = new Mock<ICommunicationLogQueries>();
        _serializerMock = new Mock<ISerializer>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<CommunicationsModuleApi>>();

        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{\"to\":\"test@test.com\"}");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<SmsOutboxPayload>())).Returns("{\"phone\":\"123456\"}");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<PushOutboxPayload>())).Returns("{\"token\":\"token\"}");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<MessageEnvelope>())).Returns("{\"v\":1,\"p\":\"{}\"}");

        _api = new CommunicationsModuleApi(
            _outboxRepositoryMock.Object,
            _templateQueriesMock.Object,
            _logQueriesMock.Object,
            _serializerMock.Object,
            _serviceProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_WithValidData_ShouldEnqueueMessage()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", "Body");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m => m.Channel == ECommunicationChannel.Email), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "Subject", "Body")]
    [InlineData("test@test.com", null, "Body")]
    public async Task SendEmailAsync_WithInvalidData_ShouldReturnFailure(string? to, string? subject, string? body)
    {
        // Arrange
        var dto = new EmailMessageDto(to!, subject!, body!);

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_WithoutTemplateKeyAndNoBody_ShouldFail()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", null);

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("corpo do e-mail");
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_WithTemplateKeyAndNoBody_ShouldSucceed()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", null, TemplateKey: "welcome");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithTemplateKeyAndTemplateData_ShouldEnqueueTemplatePayload()
    {
        // Arrange
        var templateData = new Dictionary<string, string> { { "Name", "John" } };
        var dto = new EmailMessageDto("test@test.com", "Subject", null, TemplateKey: "welcome", TemplateData: templateData);
        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_WithValidData_ShouldEnqueueMessage()
    {
        // Arrange
        var dto = new SmsMessageDto("123456", "Body");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<SmsOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendSmsAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m => m.Channel == ECommunicationChannel.Sms), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendPushAsync_WithValidData_ShouldEnqueueMessage()
    {
        // Arrange
        var dto = new PushMessageDto("token", "Title", "Body");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<PushOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendPushAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m => m.Channel == ECommunicationChannel.Push), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithNullDto_ShouldReturnFailure()
    {
        // Act
        var result = await _api.SendEmailAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_WithInvalidPriority_ShouldReturnFailure()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", "Body");

        // Act
        var result = await _api.SendEmailAsync(dto, (ECommunicationPriority)999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "Body")]
    [InlineData("123", null)]
    public async Task SendSmsAsync_WithInvalidData_ShouldReturnFailure(string? phone, string? message)
    {
        // Arrange
        var dto = new SmsMessageDto(phone!, message!);

        // Act
        var result = await _api.SendSmsAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendSmsAsync_WithNullDto_ShouldReturnFailure()
    {
        // Act
        var result = await _api.SendSmsAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SendPushAsync_WithNullDto_ShouldReturnFailure()
    {
        // Act
        var result = await _api.SendPushAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_WithInvalidPriority_ShouldReturnFailure()
    {
        // Arrange
        var dto = new SmsMessageDto("123", "Body");

        // Act
        var result = await _api.SendSmsAsync(dto, (ECommunicationPriority)999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "Title", "Body")]
    [InlineData("token", null, "Body")]
    [InlineData("token", "Title", null)]
    public async Task SendPushAsync_WithInvalidData_ShouldReturnFailure(string? token, string? title, string? body)
    {
        // Arrange
        var dto = new PushMessageDto(token!, title!, body!);

        // Act
        var result = await _api.SendPushAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendPushAsync_WithInvalidPriority_ShouldReturnFailure()
    {
        // Arrange
        var dto = new PushMessageDto("token", "Title", "Body");

        // Act
        var result = await _api.SendPushAsync(dto, (ECommunicationPriority)999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetLogsAsync_WithNullQuery_ShouldReturnFailure()
    {
        // Act
        var result = await _api.GetLogsAsync(null!);

        result.IsSuccess.Should().BeFalse();
        _logQueriesMock.Verify(x => x.SearchAsync(
            It.IsAny<CommunicationLogQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _logQueriesMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(0, 10)]
    public async Task GetLogsAsync_WithInvalidPaginationValues_ShouldReturnFailure(int page, int size)
    {
        // Arrange
        var query = new CommunicationLogQuery { PageNumber = page, PageSize = size };

        // Act
        var result = await _api.GetLogsAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logQueriesMock.Verify(x => x.SearchAsync(
            It.IsAny<CommunicationLogQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTemplatesAsync_ShouldReturnDtos()
    {
        // Arrange
        var templates = new List<EmailTemplate>
        {
            EmailTemplate.Create("key1", "Sub1", "Html1", "Text1")
        };
        _templateQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act
        var result = await _api.GetTemplatesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Key.Should().Be("key1");
    }

    [Fact]
    public async Task GetLogsAsync_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new CommunicationLogQuery { PageNumber = 1, PageSize = 10 };
        var logs = new List<CommunicationLog>
        {
            CommunicationLog.CreateSuccess("corr1", ECommunicationChannel.Email, "rec1", 1)
        };
        _logQueriesMock.Setup(x => x.SearchAsync(It.IsAny<CommunicationLogQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((logs, 1));

        // Act
        var result = await _api.GetLogsAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(1);
    }

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_WhenHealthServiceUnavailableAndDbWorks_ShouldReturnTrue()
    {
        // Arrange
        _serviceProviderMock.Setup(static x => x.GetService(typeof(HealthCheckService))).Returns((HealthCheckService?)null);
        _templateQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<EmailTemplate> { EmailTemplate.Create("t", "s", "h", "t") });

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthServiceReturnsHealthyAndDbWorks_ShouldReturnTrue()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero));

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns(healthCheckServiceMock.Object);
        _templateQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<EmailTemplate> { EmailTemplate.Create("t", "s", "h", "t") });

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthServiceReturnsUnhealthy_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        var entries = new Dictionary<string, HealthReportEntry>
        {
            { "db", new HealthReportEntry(HealthStatus.Unhealthy, "Error", TimeSpan.Zero, null, null) }
        };
        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReport(entries, HealthStatus.Unhealthy, TimeSpan.Zero));

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns(healthCheckServiceMock.Object);

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenDbOperationFails_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns((HealthCheckService?)null);
        _templateQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns(healthCheckServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _api.IsAvailableAsync(cts.Token));
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthServiceReturnsDegraded_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        var entries = new Dictionary<string, HealthReportEntry>
        {
            { "db", new HealthReportEntry(HealthStatus.Degraded, "Slow", TimeSpan.Zero, null, null) }
        };
        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthReport(entries, HealthStatus.Degraded, TimeSpan.Zero));

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns(healthCheckServiceMock.Object);

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenNoTemplatesFound_ShouldStillReturnTrue()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns((HealthCheckService?)null);
        _templateQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<EmailTemplate>());

        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenBasicOperationsCancelled_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService))).Returns((HealthCheckService?)null);
        var cts = new CancellationTokenSource();
        _templateQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                cts.Cancel();
                await Task.Yield();
                throw new OperationCanceledException(ct);
            });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _api.IsAvailableAsync(cts.Token));
    }

    [Fact]
    public async Task GetLogsAsync_WhenSearchReturnsNullItems_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new CommunicationLogQuery { PageNumber = 1, PageSize = 10 };
        _logQueriesMock.Setup(x => x.SearchAsync(It.IsAny<CommunicationLogQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<CommunicationLog>)null!, 0));

        // Act
        var result = await _api.GetLogsAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlBody_ShouldCreateHtmlPayload()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", "<p>Html</p>") { IsHtml = true };
        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _serializerMock.Verify(x => x.Serialize(It.Is<EmailOutboxPayload>(p =>
            p.HtmlBody == "<p>Html</p>" && p.TextBody == null && p.To == "test@test.com" && p.Subject == "Subject")), Times.Once);
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithTextBody_ShouldCreateTextPayload()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", "Plain text") { IsHtml = false };
        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _serializerMock.Verify(x => x.Serialize(It.Is<EmailOutboxPayload>(p =>
            p.TextBody == "Plain text" && p.HtmlBody == null && p.To == "test@test.com" && p.Subject == "Subject")), Times.Once);
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithTemplateKeyButEmptyBody_ShouldSucceed()
    {
        // Arrange
        var templateData = new Dictionary<string, string> { { "Name", "John" } };
        var dto = new EmailMessageDto("test@test.com", "Subject", "", TemplateKey: "welcome", TemplateData: templateData);
        _serializerMock.Setup(x => x.Serialize(It.IsAny<EmailOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendEmailAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_WithValidDataAndHighPriority_ShouldEnqueueWithPriority()
    {
        // Arrange
        var dto = new SmsMessageDto("123456", "Body");
        _serializerMock.Setup(x => x.Serialize(It.IsAny<SmsOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendSmsAsync(dto, ECommunicationPriority.High);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m => m.Priority == ECommunicationPriority.High), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendPushAsync_WithExtraData_ShouldPassToPayload()
    {
        // Arrange
        var extraData = new Dictionary<string, string> { { "key", "value" } };
        var dto = new PushMessageDto("token", "Title", "Body") { ExtraData = extraData };
        _serializerMock.Setup(x => x.Serialize(It.IsAny<PushOutboxPayload>())).Returns("{}");

        // Act
        var result = await _api.SendPushAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _serializerMock.Verify(x => x.Serialize(It.Is<PushOutboxPayload>(p =>
            p.DeviceToken == "token" && p.Title == "Title" && p.Body == "Body" &&
            p.Data != null && p.Data["key"] == "value")), Times.Once);
        _outboxRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ModuleName_ShouldReturnCommunications()
    {
        _api.ModuleName.Should().Be("Communications");
    }

    [Fact]
    public async Task ApiVersion_ShouldReturn1_0()
    {
        _api.ApiVersion.Should().Be("1.0");
    }

    #endregion
}
