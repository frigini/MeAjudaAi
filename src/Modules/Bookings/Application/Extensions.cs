using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Events;
using MeAjudaAi.Modules.Bookings.Application.Handlers;
using MeAjudaAi.Modules.Bookings.Application.ModuleApi;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Modules.Bookings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingsModuleApi, BookingsModuleApi>();
        services.AddModuleValidators(Assembly.GetExecutingAssembly());

        // Comandos
        services.AddScoped<ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>, CreateBookingCommandHandler>();
        services.AddScoped<ICommandHandler<SetProviderScheduleCommand, Result>, SetProviderScheduleCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmBookingCommand, Result>, ConfirmBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CancelBookingCommand, Result>, CancelBookingCommandHandler>();
        services.AddScoped<ICommandHandler<RejectBookingCommand, Result>, RejectBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteBookingCommand, Result>, CompleteBookingCommandHandler>();
        
        // Consultas
        services.AddScoped<IQueryHandler<GetProviderAvailabilityQuery, Result<AvailabilityDto>>, GetProviderAvailabilityQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingByIdQuery, Result<ModuleBookingDto>>, GetBookingByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingsByClientQuery, Result<PagedResult<ModuleBookingDto>>>, GetBookingsByClientQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<ModuleBookingDto>>>, GetBookingsByProviderQueryHandler>();

        services.AddScoped<ProviderAuthorizationResolver>();

        // Manipuladores de Eventos (SSE Realtime)
        services.AddScoped<BookingRealtimeEventsHandler>();
        services.AddScoped<IEventHandler<BookingCreatedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingConfirmedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingCancelledDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingRejectedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingCompletedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());

        return services;
    }
}
