using FluentValidation;
using MeAjudaAi.Modules.Users.Application.Commands;

namespace MeAjudaAi.Modules.Users.Application.Validators;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
