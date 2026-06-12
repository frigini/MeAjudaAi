using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Validators;

public class CompleteBookingCommandValidator : AbstractValidator<CompleteBookingCommand>
{
    public CompleteBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("O identificador da reserva é obrigatório.");
    }
}
