using MeAjudaAi.Modules.Payments.Application.DTOs.Requests;
using MeAjudaAi.Modules.Payments.API.Mappers;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_CreateSubscriptionRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new CreateSubscriptionRequest(
            ProviderId: Guid.NewGuid(),
            PlanId: "plan_pro_monthly");

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(request.ProviderId);
        command.PlanId.Should().Be("plan_pro_monthly");
        command.IdempotencyKey.Should().BeNull();
    }

    [Fact]
    public void ToCommand_CreateSubscriptionRequest_WithIdempotencyKey_ShouldMapKey()
    {
        // Arrange
        var request = new CreateSubscriptionRequest(
            ProviderId: Guid.NewGuid(),
            PlanId: "plan_basic_yearly");
        var idempotencyKey = "idem-key-123";

        // Act
        var command = request.ToCommand(idempotencyKey);

        // Assert
        command.IdempotencyKey.Should().Be("idem-key-123");
    }

    [Fact]
    public void ToCommand_GetBillingPortalRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new GetBillingPortalRequest(
            ProviderId: Guid.NewGuid(),
            ReturnUrl: "https://example.com/billing");
        var finalReturnUrl = "https://example.com/billing";

        // Act
        var command = request.ToCommand(finalReturnUrl);

        // Assert
        command.Should().NotBeNull();
        command.ProviderId.Should().Be(request.ProviderId);
        command.ReturnUrl.Should().Be("https://example.com/billing");
    }

    [Fact]
    public void ToCommand_GetBillingPortalRequest_WithNullReturnUrl_ShouldMapFinalUrl()
    {
        // Arrange
        var request = new GetBillingPortalRequest(
            ProviderId: Guid.NewGuid(),
            ReturnUrl: null);
        var finalReturnUrl = "https://client.example.com";

        // Act
        var command = request.ToCommand(finalReturnUrl);

        // Assert
        command.ReturnUrl.Should().Be("https://client.example.com");
    }
}
