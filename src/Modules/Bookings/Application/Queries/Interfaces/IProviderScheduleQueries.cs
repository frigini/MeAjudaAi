using MeAjudaAi.Modules.Bookings.Domain.Entities;

namespace MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;

public interface IProviderScheduleQueries
{
    Task<ProviderSchedule?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<ProviderSchedule?> GetByProviderIdReadOnlyAsync(Guid providerId, CancellationToken cancellationToken = default);
}
