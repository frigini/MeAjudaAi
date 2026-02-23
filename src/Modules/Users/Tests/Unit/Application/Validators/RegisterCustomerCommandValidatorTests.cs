using FluentValidation.TestHelper;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Validators;
using MeAjudaAi.Shared.Utilities.Constants;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class RegisterCustomerCommandValidatorTests
{
    private readonly RegisterCustomerCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new RegisterCustomerCommand("", "test@test.com", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Short()
    {
        // MinLength is FirstNameMinLength (2) + LastNameMinLength (2) = 4
        var command = new RegisterCustomerCommand("abc", "test@test.com", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Nome deve ter pelo menos {ValidationConstants.UserLimits.FirstNameMinLength + ValidationConstants.UserLimits.LastNameMinLength} caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "invalid-email", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "short", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
