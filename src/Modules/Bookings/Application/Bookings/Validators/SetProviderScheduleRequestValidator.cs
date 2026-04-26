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
            .Must(x => {
                var days = new HashSet<DayOfWeek>();
                foreach (var a in x)
                {
                    if (a != null && !days.Add(a.DayOfWeek)) return false;
                }
                return true;
            })
            .WithMessage("A lista de disponibilidades contém dias da semana duplicados.");

        RuleForEach(x => x.Availabilities)
            .NotNull().WithMessage("Item de disponibilidade não pode ser nulo.")
            .ChildRules(availability => {
                availability.RuleFor(x => x.Slots)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(x => $"A lista de horários para {x.DayOfWeek} não pode ser vazia.")
                    .Must((availabilityDto, slots) => {
                        var list = slots.ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            for (int j = i + 1; j < list.Count; j++)
                            {
                                // Simple overlap check: (StartA < EndB) && (StartB < EndA)
                                if (list[i].Start < list[j].End && list[j].Start < list[i].End)
                                    return false;
                            }
                        }
                        return true;
                    }).WithMessage((availabilityDto, slots) => {
                        var list = slots.ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            for (int j = i + 1; j < list.Count; j++)
                            {
                                if (list[i].Start < list[j].End && list[j].Start < list[i].End)
                                    return $"A lista de horários para {availabilityDto.DayOfWeek} contém sobreposições entre os horários {i+1} ({list[i].Start}-{list[i].End}) e {j+1} ({list[j].Start}-{list[j].End}).";
                            }
                        }
                        return $"A lista de horários para {availabilityDto.DayOfWeek} contém sobreposições.";
                    });

                availability.RuleForEach(x => x.Slots).ChildRules(slot => {
                    slot.RuleFor(x => x.End)
                        .GreaterThan(x => x.Start)
                        .WithMessage((s, end) => $"Horário inválido: o término ({end}) deve ser após o início ({s.Start}).");
                });
            });
    }
}
