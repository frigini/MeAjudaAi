using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public.Events;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Bookings.
/// </summary>
[ExcludeFromCodeCoverage]
public static class BookingsEndpoints
{
    public const string Tag = "Bookings";

    /// <summary>
    /// Mapeia todos os endpoints do módulo Bookings.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Bookings.Base, Tag);

        group.MapEndpoint<CreateBookingEndpoint>()
             .MapEndpoint<ConfirmBookingEndpoint>()
             .MapEndpoint<CancelBookingEndpoint>()
             .MapEndpoint<RejectBookingEndpoint>()
             .MapEndpoint<CompleteBookingEndpoint>()
             .MapEndpoint<GetBookingByIdEndpoint>()
             .MapEndpoint<GetMyBookingsEndpoint>()
             .MapEndpoint<GetProviderBookingsEndpoint>()
             .MapEndpoint<GetProviderAvailabilityEndpoint>()
             .MapEndpoint<SetProviderScheduleEndpoint>()
             .MapEndpoint<GetBookingEventsEndpoint>();
    }
}
