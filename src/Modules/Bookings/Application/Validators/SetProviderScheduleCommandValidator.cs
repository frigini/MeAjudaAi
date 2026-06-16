using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Commands;

namespace MeAjudaAi.Modules.Bookings.Application.Validators;

public class SetProviderScheduleCommandValidator : AbstractValidator<SetProviderScheduleCommand>
{
    public SetProviderScheduleCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("O identificador do prestador é obrigatório.");

        RuleFor(x => x.Availabilities)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("A propriedade 'Availabilities' é obrigatória.")
            .NotEmpty().WithMessage("A lista de disponibilidades não pode ser vazia.");

        RuleForEach(x => x.Availabilities)
            .NotNull().WithMessage("Item de disponibilidade não pode ser nulo.")
            .ChildRules(availability =>
            {
                availability.RuleFor(x => x.DayOfWeek)
                    .IsInEnum().WithMessage("O dia da semana deve ser um valor válido.");

                availability.RuleFor(x => x.Slots)
                    .Cascade(CascadeMode.Stop)
                    .NotNull().WithMessage(x => $"A lista de horários para {x.DayOfWeek} não pode ser nula.")
                    .NotEmpty().WithMessage(x => $"A lista de horários para {x.DayOfWeek} não pode ser vazia.");

                availability.RuleForEach(x => x.Slots)
                    .ChildRules(slot =>
                    {
                        slot.RuleFor(x => x.End)
                            .GreaterThan(x => x.Start)
                            .WithMessage((s, end) => $"Horário inválido: o término ({end:HH:mm}) deve ser após o início ({s.Start:HH:mm}).");
                    });
            });
    }
}
