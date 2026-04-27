using FluentValidation;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;

public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("O motivo do cancelamento é obrigatório.")
            .MaximumLength(500).WithMessage("O motivo do cancelamento não pode exceder 500 caracteres.");
    }
}
