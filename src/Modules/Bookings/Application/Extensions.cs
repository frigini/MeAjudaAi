using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateBookingCommand, Result<BookingDto>>, CreateBookingCommandHandler>();
        services.AddScoped<ICommandHandler<SetProviderScheduleCommand, Result>, SetProviderScheduleCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmBookingCommand, Result>, ConfirmBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CancelBookingCommand, Result>, CancelBookingCommandHandler>();
        
        services.AddScoped<IQueryHandler<GetProviderAvailabilityQuery, Result<AvailabilityDto>>, GetProviderAvailabilityQueryHandler>();

        return services;
    }
}
