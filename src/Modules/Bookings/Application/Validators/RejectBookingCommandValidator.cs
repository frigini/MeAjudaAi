using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Validators;

public class RejectBookingCommandValidator : AbstractValidator<RejectBookingCommand>
{
    public RejectBookingCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("O motivo da rejeição é obrigatório.")
            .MaximumLength(500).WithMessage("O motivo da rejeição não pode exceder 500 caracteres.");
    }
}
