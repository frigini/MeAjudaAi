using MeAjudaAi.Modules.Payments.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Application")]
public class PaymentsOptionsValidatorTests
{
    private readonly Mock<IConfiguration> _configurationMock;

    public PaymentsOptionsValidatorTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("https://meajudaai.com");
    }

    [Fact]
    public void Validate_ShouldReturnSkip_WhenNameIsNotPaymentsSection()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions();

        var result = validator.Validate("SomeOtherSection", options);

        result.Should().Be(ValidateOptionsResult.Skip);
    }

    [Fact]
    public void Validate_ShouldNotSkip_WhenNameIsEmpty()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions();

        var result = validator.Validate("", options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenInBypassEnvironmentAndOptionsEmpty()
    {
        var originalEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            var validator = new PaymentsOptionsValidator(_configurationMock.Object);
            var options = new PaymentsOptions();

            var result = validator.Validate(PaymentsOptions.SectionName, options);

            result.Succeeded.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalEnv);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenAllRequiredOptionsAreConfigured()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions
        {
            SuccessUrl = "https://meajudaai.com/success",
            CancelUrl = "https://meajudaai.com/cancel",
            Plans = new Dictionary<string, PlanOptions>
            {
                ["basic"] = new PlanOptions { StripePriceId = "price_basic", Amount = 9.99m }
            }
        };

        var result = validator.Validate(PaymentsOptions.SectionName, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFailureWithMultipleErrors_WhenMultipleOptionsMissing()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions();

        var result = validator.Validate(PaymentsOptions.SectionName, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Failures.Should().Contain(e => e.Contains("SuccessUrl"));
        result.Failures.Should().Contain(e => e.Contains("CancelUrl"));
        result.Failures.Should().Contain(e => e.Contains("Plans"));
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenSuccessUrlMissing()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions
        {
            CancelUrl = "https://meajudaai.com/cancel",
            Plans = new Dictionary<string, PlanOptions>
            {
                ["basic"] = new PlanOptions { StripePriceId = "price_basic", Amount = 9.99m }
            }
        };

        var result = validator.Validate(PaymentsOptions.SectionName, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(e => e.Contains("SuccessUrl"));
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenCancelUrlMissing()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions
        {
            SuccessUrl = "https://meajudaai.com/success",
            Plans = new Dictionary<string, PlanOptions>
            {
                ["basic"] = new PlanOptions { StripePriceId = "price_basic", Amount = 9.99m }
            }
        };

        var result = validator.Validate(PaymentsOptions.SectionName, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(e => e.Contains("CancelUrl"));
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenPlansIsEmpty()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions
        {
            SuccessUrl = "https://meajudaai.com/success",
            CancelUrl = "https://meajudaai.com/cancel",
            Plans = new Dictionary<string, PlanOptions>()
        };

        var result = validator.Validate(PaymentsOptions.SectionName, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(e => e.Contains("Plans"));
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenClientBaseUrlMissing()
    {
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns((string?)null);
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions
        {
            SuccessUrl = "https://meajudaai.com/success",
            CancelUrl = "https://meajudaai.com/cancel",
            Plans = new Dictionary<string, PlanOptions>
            {
                ["basic"] = new PlanOptions { StripePriceId = "price_basic", Amount = 9.99m }
            }
        };

        var result = validator.Validate(PaymentsOptions.SectionName, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(e => e.Contains("ClientBaseUrl"));
    }

    [Fact]
    public void Validate_ShouldPassForNullName_WhenSectionName()
    {
        var validator = new PaymentsOptionsValidator(_configurationMock.Object);
        var options = new PaymentsOptions
        {
            SuccessUrl = "https://meajudaai.com/success",
            CancelUrl = "https://meajudaai.com/cancel",
            Plans = new Dictionary<string, PlanOptions>
            {
                ["basic"] = new PlanOptions { StripePriceId = "price_basic", Amount = 9.99m }
            }
        };

        var result = validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }
}
