using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Application.ModuleApi;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Moq;
using MeAjudaAi.Contracts.Shared;
using FluentAssertions;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.ModuleApi;

public class CommunicationsModuleApiTests
{
    private readonly Mock<IOutboxMessageRepository> _outboxRepositoryMock;
    private readonly Mock<IEmailTemplateRepository> _templateRepositoryMock;
    private readonly Mock<ICommunicationLogRepository> _logRepositoryMock;
    private readonly CommunicationsModuleApi _api;

    public CommunicationsModuleApiTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxMessageRepository>();
        _templateRepositoryMock = new Mock<IEmailTemplateRepository>();
        _logRepositoryMock = new Mock<ICommunicationLogRepository>();

        _api = new CommunicationsModuleApi(
            _outboxRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _logRepositoryMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_WithValidData_ShouldEnqueueMessage()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", "Body");

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
    [InlineData("test@test.com", "Subject", null)]
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
    public async Task SendSmsAsync_WithValidData_ShouldEnqueueMessage()
    {
        // Arrange
        var dto = new SmsMessageDto("123456", "Body");

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
        _logRepositoryMock.Verify(x => x.SearchAsync(
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
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
        _logRepositoryMock.Verify(x => x.SearchAsync(
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
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
        _templateRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
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
        _logRepositoryMock.Setup(x => x.SearchAsync(null, null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((logs, 1));

        // Act
        var result = await _api.GetLogsAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue()
    {
        // Act
        var result = await _api.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }
}
