using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Modules.Bookings.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints;

[ExcludeFromCodeCoverage]
public static class BookingsEndpoints
{
    public const string Route = "bookings";
    public const string Tag = "Bookings";

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, Route, Tag);

        group.MapEndpoint<CreateBookingEndpoint>()
             .MapEndpoint<ConfirmBookingEndpoint>()
             .MapEndpoint<CancelBookingEndpoint>()
             .MapEndpoint<RejectBookingEndpoint>()
             .MapEndpoint<CompleteBookingEndpoint>()
             .MapEndpoint<GetBookingByIdEndpoint>()
             .MapEndpoint<GetMyBookingsEndpoint>()
             .MapEndpoint<GetProviderBookingsEndpoint>()
             .MapEndpoint<GetProviderAvailabilityEndpoint>()
             .MapEndpoint<SetProviderScheduleEndpoint>();
    }
}
