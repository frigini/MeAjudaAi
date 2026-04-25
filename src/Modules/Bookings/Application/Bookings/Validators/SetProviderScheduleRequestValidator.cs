using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Validators;

public class SetProviderScheduleRequestValidator : AbstractValidator<SetProviderScheduleRequest>
{
    public SetProviderScheduleRequestValidator()
    {
        RuleFor(x => x.Availabilities)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Propriedade 'Availabilities' é obrigatória.")
            .NotEmpty().WithMessage("A lista de disponibilidades não pode ser vazia.")
            .Must(x => x.Select(a => a.DayOfWeek).Distinct().Count() == x.Count())
            .WithMessage("A lista de disponibilidades contém dias da semana duplicados.");

        RuleForEach(x => x.Availabilities).ChildRules(availability => {
            availability.RuleFor(x => x).NotNull().WithMessage("Item de disponibilidade não pode ser nulo.");
            
            availability.RuleFor(x => x.Slots)
                .NotEmpty().WithMessage(x => $"A lista de horários para {x.DayOfWeek} não pode ser vazia.");

            availability.RuleForEach(x => x.Slots).ChildRules(slot => {
                slot.RuleFor(x => x.End)
                    .GreaterThan(x => x.Start)
                    .WithMessage((s, end) => $"Horário inválido: o término ({end}) deve ser após o início ({s.Start}).");
            });
        });
    }
}
