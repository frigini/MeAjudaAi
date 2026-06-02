using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Domain.Entities;

namespace MeAjudaAi.Modules.Bookings.Application.Services;

public interface IBookingCommandService
{
    Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken cancellationToken = default);
}
