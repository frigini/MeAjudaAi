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
        // MinLength é a soma de FirstNameMinLength (2) e LastNameMinLength (2) = 4
        var command = new RegisterCustomerCommand("abc", "test@test.com", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage($"Nome deve ter pelo menos {ValidationConstants.UserLimits.FirstNameMinLength + ValidationConstants.UserLimits.LastNameMinLength} caracteres");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Contains_Numbers()
    {
        var command = new RegisterCustomerCommand("Jean 123", "test@test.com", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome deve conter apenas letras e espaços");
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
    public void Should_Have_Error_When_Password_Missing_Uppercase()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma minúscula e um número");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Number()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "Password!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "", "Password123!", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "", "123456789", true, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Terms_Not_Accepted()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "Password123!", "123456789", false, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TermsAccepted);
    }

    [Fact]
    public void Should_Have_Error_When_Privacy_Policy_Not_Accepted()
    {
        var command = new RegisterCustomerCommand("Jean Valjean", "test@test.com", "Password123!", "123456789", true, false);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AcceptedPrivacyPolicy);
    }
}
