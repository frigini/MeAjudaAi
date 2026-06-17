using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries;

namespace MeAjudaAi.Modules.Bookings.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands e Queries do módulo Bookings.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia CreateBookingRequestDto para CreateBookingCommand.
    /// </summary>
    public static CreateBookingCommand ToCommand(this CreateBookingRequestDto request, Guid clientId, Guid correlationId)
    {
        return new CreateBookingCommand(
            request.ProviderId,
            clientId,
            request.ServiceId,
            request.Start,
            request.End,
            correlationId);
    }

    /// <summary>
    /// Mapeia CancelBookingRequestDto para CancelBookingCommand.
    /// </summary>
    public static CancelBookingCommand ToCommand(
        this CancelBookingRequestDto request,
        Guid bookingId,
        bool isAdmin,
        Guid? providerId,
        Guid? userId,
        Guid correlationId)
    {
        return new CancelBookingCommand(
            bookingId,
            request.Reason,
            isAdmin,
            providerId,
            userId,
            correlationId);
    }

    /// <summary>
    /// Mapeia RejectBookingRequestDto para RejectBookingCommand.
    /// </summary>
    public static RejectBookingCommand ToCommand(
        this RejectBookingRequestDto request,
        Guid bookingId,
        bool isAdmin,
        Guid? providerId,
        Guid correlationId)
    {
        return new RejectBookingCommand(
            bookingId,
            request.Reason,
            isAdmin,
            providerId,
            correlationId);
    }

    /// <summary>
    /// Mapeia parâmetros para ConfirmBookingCommand.
    /// </summary>
    public static ConfirmBookingCommand ToConfirmCommand(
        this Guid bookingId,
        bool isAdmin,
        Guid? providerId,
        Guid correlationId)
    {
        return new ConfirmBookingCommand(
            bookingId,
            isAdmin,
            providerId,
            correlationId);
    }

    /// <summary>
    /// Mapeia parâmetros para CompleteBookingCommand.
    /// </summary>
    public static CompleteBookingCommand ToCompleteCommand(
        this Guid bookingId,
        bool isAdmin,
        Guid? providerId,
        Guid correlationId)
    {
        return new CompleteBookingCommand(
            bookingId,
            isAdmin,
            providerId,
            correlationId);
    }

    /// <summary>
    /// Mapeia SetProviderScheduleRequestDto para SetProviderScheduleCommand.
    /// </summary>
    public static SetProviderScheduleCommand ToCommand(
        this SetProviderScheduleRequestDto request,
        Guid targetProviderId,
        Guid correlationId)
    {
        return new SetProviderScheduleCommand(
            targetProviderId,
            request.Availabilities,
            correlationId);
    }

    /// <summary>
    /// Mapeia parâmetros para GetBookingsByClientQuery.
    /// </summary>
    public static GetBookingsByClientQuery ToQuery(
        this Guid clientId,
        Guid correlationId,
        int page,
        int pageSize,
        DateTime? from,
        DateTime? to)
    {
        return new GetBookingsByClientQuery(
            clientId,
            correlationId,
            page,
            pageSize,
            from,
            to);
    }

    /// <summary>
    /// Mapeia parâmetros para GetBookingsByProviderQuery.
    /// </summary>
    public static GetBookingsByProviderQuery ToProviderQuery(
        this Guid providerId,
        Guid correlationId,
        int page,
        int pageSize,
        DateTime? from,
        DateTime? to)
    {
        return new GetBookingsByProviderQuery(
            providerId,
            correlationId,
            page,
            pageSize,
            from,
            to);
    }

    /// <summary>
    /// Mapeia parâmetros para GetBookingByIdQuery.
    /// </summary>
    public static GetBookingByIdQuery ToQuery(
        this Guid bookingId,
        Guid? userId,
        Guid? providerId,
        bool isSystemAdmin,
        Guid correlationId)
    {
        return new GetBookingByIdQuery(
            bookingId,
            userId,
            providerId,
            isSystemAdmin,
            correlationId);
    }

    /// <summary>
    /// Mapeia parâmetros para GetProviderAvailabilityQuery.
    /// </summary>
    public static GetProviderAvailabilityQuery ToAvailabilityQuery(
        this Guid providerId,
        DateOnly date,
        Guid correlationId)
    {
        return new GetProviderAvailabilityQuery(
            providerId,
            date,
            correlationId);
    }
}
