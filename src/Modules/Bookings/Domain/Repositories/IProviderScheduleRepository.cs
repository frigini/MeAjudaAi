using MeAjudaAi.Modules.Bookings.Domain.Entities;

namespace MeAjudaAi.Modules.Bookings.Domain.Repositories;

public interface IProviderScheduleRepository
{
    Task<ProviderSchedule?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<ProviderSchedule?> GetByProviderIdReadOnlyAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task AddAsync(ProviderSchedule schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProviderSchedule schedule, CancellationToken cancellationToken = default);
}
