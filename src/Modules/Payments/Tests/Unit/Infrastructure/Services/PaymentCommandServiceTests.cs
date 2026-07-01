using System.Security.Cryptography;
using System.Text;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Services;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Stripe;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class PaymentCommandServiceTests : BaseInMemoryDatabaseTest<PaymentsDbContext>, IDisposable
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<PaymentCommandService>> _loggerMock;
    private readonly Mock<IRepository<InboxMessage, Guid>> _inboxRepositoryMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly PaymentCommandService _service;
    private readonly EnvironmentVariableRestorer _envRestorer;

    public PaymentCommandServiceTests()
        : base(options => new PaymentsDbContext(options))
    {
        _uowMock = new Mock<IUnitOfWork>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<PaymentCommandService>>();
        _inboxRepositoryMock = new Mock<IRepository<InboxMessage, Guid>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();
        _envRestorer = new EnvironmentVariableRestorer();

        _configurationMock.Setup(x => x["Stripe:WebhookSecret"]).Returns("whsec_test_secret");

        _uowMock.Setup(x => x.GetRepository<InboxMessage, Guid>()).Returns(_inboxRepositoryMock.Object);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new PaymentCommandService(
            _uowMock.Object,
            DbContext,
            _configurationMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);
    }

    public new void Dispose()
    {
        _envRestorer.Restore();
        GC.SuppressFinalize(this);
    }

    private static string GenerateStripeSignature(string payload, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        return $"t={timestamp},v1={signature}";
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldReturnBadRequest_WhenPayloadIsEmpty()
    {
        var result = await _service.HandleStripeWebhookAsync("", "sig_test", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldReturnInternalError_WhenWebhookSecretMissing()
    {
        _configurationMock.Setup(x => x["Stripe:WebhookSecret"]).Returns((string?)null);

        var service = new PaymentCommandService(
            _uowMock.Object,
            DbContext,
            _configurationMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);

        var payload = """{"type": "checkout.session.completed", "id": "evt_test"}""";
        var result = await service.HandleStripeWebhookAsync(payload, "sig_test", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldReturnBadRequest_WhenSignatureIsInvalid()
    {
        var payload = """{"type": "checkout.session.completed", "id": "evt_test_invalid"}""";
        var invalidSignature = "t=1234567890,v1=invalid_signature_that_will_fail_verification";

        var result = await _service.HandleStripeWebhookAsync(payload, invalidSignature, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldReturnBadRequest_WhenJsonIsMalformed()
    {
        var malformedPayload = "{ invalid json }";
        var signature = "sig_test";

        var result = await _service.HandleStripeWebhookAsync(malformedPayload, signature, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldSucceed_WhenSignatureIsValid()
    {
        var webhookSecret = "whsec_test_secret_for_valid_signature";
        _configurationMock.Setup(x => x["Stripe:WebhookSecret"]).Returns(webhookSecret);

        var service = new PaymentCommandService(
            _uowMock.Object,
            DbContext,
            _configurationMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);

        var eventId = "evt_valid_" + Guid.NewGuid().ToString("N");
        var payload = $$"""
        {
            "id": "{{eventId}}",
            "type": "checkout.session.completed",
            "created": 1234567890,
            "data": {
                "object": {
                    "id": "cs_test",
                    "object": "checkout.session",
                    "customer": "cus_test"
                }
            }
        }
        """;

        var signature = GenerateStripeSignature(payload, webhookSecret);

        var result = await service.HandleStripeWebhookAsync(payload, signature, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveInboxMessageAsync_ShouldReturnSuccess_WhenEventAlreadyExists()
    {
        var externalEventId = "evt_duplicate_" + Guid.NewGuid();
        var existingMessage = new InboxMessage(
            "checkout.session.completed",
            """{"id": "evt_duplicate"}""",
            externalEventId);

        DbContext.InboxMessages.Add(existingMessage);
        await DbContext.SaveChangesAsync();

        var result = await _service.SaveInboxMessageAsync(
            "checkout.session.completed",
            """{"id": "evt_duplicate"}""",
            externalEventId,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveInboxMessageAsync_ShouldSaveNewEvent()
    {
        var externalEventId = "evt_new_" + Guid.NewGuid();

        var result = await _service.SaveInboxMessageAsync(
            "invoice.paid",
            """{"id": "evt_new"}""",
            externalEventId,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _inboxRepositoryMock.Verify(x => x.Add(It.IsAny<InboxMessage>()), Times.Once);
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldBypassSignature_WhenEmptySignatureAndBypassEnvironment()
    {
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            var service = new PaymentCommandService(
                _uowMock.Object,
                DbContext,
                _configurationMock.Object,
                _loggerMock.Object,
                _localizerMock.Object);

            var eventId = "evt_mock_" + Guid.NewGuid().ToString("N");
            var payload = $$"""
            {
                "id": "{{eventId}}",
                "type": "checkout.session.completed",
                "created": 1234567890
            }
            """;

            var result = await service.HandleStripeWebhookAsync(payload, "", CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_ShouldFail_WhenBypassEnvironmentButInvalidMockPayload()
    {
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            var service = new PaymentCommandService(
                _uowMock.Object,
                DbContext,
                _configurationMock.Object,
                _loggerMock.Object,
                _localizerMock.Object);

            var invalidPayload = "{ invalid json for mock }";

            var result = await service.HandleStripeWebhookAsync(invalidPayload, "", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    private sealed class EnvironmentVariableRestorer
    {
        private readonly HashSet<string> _modifiedVariables = new();

        public void SetVariable(string name, string value)
        {
            if (!_modifiedVariables.Contains(name))
            {
                _modifiedVariables.Add(name);
            }
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Restore()
        {
            foreach (var name in _modifiedVariables)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }
}
