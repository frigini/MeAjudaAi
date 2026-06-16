using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Validators;

public class ConfirmBookingCommandValidator : AbstractValidator<ConfirmBookingCommand>
{
    public ConfirmBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("O identificador da reserva é obrigatório.");
    }
}
