using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public.Events;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints;

[ExcludeFromCodeCoverage]
public static class BookingsEndpoints
{
    public const string Tag = "Bookings";

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Bookings.Base, ModuleNames.Bookings);

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
