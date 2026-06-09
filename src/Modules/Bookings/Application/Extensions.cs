using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Modules.Bookings.Application.Events;
using MeAjudaAi.Modules.Bookings.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

using MeAjudaAi.Contracts.Modules.Bookings;
using MeAjudaAi.Modules.Bookings.Application.ModuleApi;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Handlers;
using MeAjudaAi.Modules.Bookings.Application.Queries;

namespace MeAjudaAi.Modules.Bookings.Application;

using MeAjudaAi.Shared.Behaviors;
using MeAjudaAi.Shared.Mediator;
using FluentValidation;
// ... (outros usings)

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingsModuleApi, BookingsModuleApi>();
        services.AddModuleValidators(Assembly.GetExecutingAssembly());
        
        // Registrar ValidationBehavior para todos os IRequest
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // ...
        services.AddScoped<ICommandHandler<CreateBookingCommand, Result<BookingDto>>, CreateBookingCommandHandler>();
        services.AddScoped<ICommandHandler<SetProviderScheduleCommand, Result>, SetProviderScheduleCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmBookingCommand, Result>, ConfirmBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CancelBookingCommand, Result>, CancelBookingCommandHandler>();
        services.AddScoped<ICommandHandler<RejectBookingCommand, Result>, RejectBookingCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteBookingCommand, Result>, CompleteBookingCommandHandler>();
        
        // Consultas
        services.AddScoped<IQueryHandler<GetProviderAvailabilityQuery, Result<AvailabilityDto>>, GetProviderAvailabilityQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingByIdQuery, Result<BookingDto>>, GetBookingByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingsByClientQuery, Result<PagedResult<BookingDto>>>, GetBookingsByClientQueryHandler>();
        services.AddScoped<IQueryHandler<GetBookingsByProviderQuery, Result<PagedResult<BookingDto>>>, GetBookingsByProviderQueryHandler>();

        services.AddScoped<ProviderAuthorizationResolver>();

        // Event Handlers (SSE Realtime)
        services.AddScoped<BookingRealtimeEventsHandler>();
        services.AddScoped<IEventHandler<BookingCreatedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingConfirmedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingCancelledDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingRejectedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());
        services.AddScoped<IEventHandler<BookingCompletedDomainEvent>>(sp => sp.GetRequiredService<BookingRealtimeEventsHandler>());

        return services;
    }
}

