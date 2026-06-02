using MeAjudaAi.Modules.Bookings.Domain.Entities;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;

public interface IProviderScheduleQueries
{
    Task<ProviderSchedule?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<ProviderSchedule?> GetByProviderIdReadOnlyAsync(Guid providerId, CancellationToken cancellationToken = default);
}
