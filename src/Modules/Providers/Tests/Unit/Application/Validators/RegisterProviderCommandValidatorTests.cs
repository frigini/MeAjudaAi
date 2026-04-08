using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Validators;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Validators;

[Trait("Category", "Unit")]
public class RegisterProviderCommandValidatorTests
{
    private readonly RegisterProviderCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            Guid.NewGuid(), "João Silva", "joao@test.com",
            "11999999999", EProviderType.Individual, "12345678901");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "joao@test.com", "11999999999")] // nome vazio
    [InlineData("João", "email-invalido", "11999999999")] // email inválido
    [InlineData("João", "joao@test.com", "")] // telefone vazio
    public async Task Validate_WithInvalidData_ShouldFailValidation(string name, string email, string phone)
    {
        // Arrange
        var command = new RegisterProviderCommand(
            Guid.NewGuid(), name, email, phone,
            EProviderType.Individual, "12345678901");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
