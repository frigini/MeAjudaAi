using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Validators;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("O identificador do prestador é obrigatório.");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("O identificador do cliente é obrigatório.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("O identificador do serviço é obrigatório.");

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start).WithMessage("A data de término deve ser posterior à data de início.");

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("O identificador de correlação é obrigatório.");
    }
}
