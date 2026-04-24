using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class EnvironmentHelpersTests
{
    [Theory]
    [InlineData("Testing", null, true)]
    [InlineData("Development", null, true)]
    [InlineData("Integration", "true", true)]
    [InlineData("Production", "true", false)] // Deve ser falso em produção mesmo com INTEGRATION_TESTS=true
    [InlineData("Production", null, false)]
    [InlineData("", null, false)]
    [InlineData(null, null, false)]
    public void IsSecurityBypassEnvironment_Should_ReturnExpectedValue(string? envName, string? integrationTests, bool expected)
    {
        // Arrange
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", envName);
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", integrationTests);

        try
        {
            // Act
            var result = EnvironmentHelpers.IsSecurityBypassEnvironment();

            // Assert
            result.Should().Be(expected);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", null);
        }
    }
}
