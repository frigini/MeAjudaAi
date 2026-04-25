using FluentValidation;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Validators;

public class SetProviderScheduleRequestValidator : AbstractValidator<SetProviderScheduleRequest>
{
    public SetProviderScheduleRequestValidator()
    {
        RuleFor(x => x.Availabilities)
            .NotEmpty().WithMessage("A lista de disponibilidades não pode ser vazia.")
            .Must(x => x != null).WithMessage("Propriedade 'Availabilities' é obrigatória.");

        RuleForEach(x => x.Availabilities).ChildRules(availability =>
        {
            availability.RuleFor(x => x).NotNull().WithMessage("Item de disponibilidade não pode ser nulo.");
            
            availability.RuleFor(x => x.Slots)
                .NotEmpty().WithMessage(x => $"A lista de horários para {x.DayOfWeek} não pode ser vazia.");

            availability.RuleForEach(x => x.Slots).SetValidator(new InlineValidator<TimeSlotDto> {
                v => v.RuleFor(x => x.End)
                    .GreaterThan(x => x.Start)
                    .WithMessage((slot, end) => $"Horário inválido: o término ({end}) deve ser após o início ({slot.Start}).")
            });
        });

        RuleFor(x => x.Availabilities)
            .Must(x => x == null || x.Select(a => a.DayOfWeek).Distinct().Count() == x.Count())
            .WithMessage("A lista de disponibilidades contém dias da semana duplicados.");
    }
}
