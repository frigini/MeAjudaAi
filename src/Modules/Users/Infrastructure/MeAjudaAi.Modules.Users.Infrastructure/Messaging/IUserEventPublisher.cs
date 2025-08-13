using MeAjudaAi.Modules.Users.Domain.Entities;

namespace MeAjudaAi.Modules.Users.Infrastructure.Messaging;

public interface IUserEventPublisher
{
    Task PublishUserRegisteredAsync(User user, CancellationToken cancellationToken = default);
    Task PublishUserRoleChangedAsync(User user, string previousRole, string newRole, CancellationToken cancellationToken = default);
    Task PublishUserLockedOutAsync(User user, string reason, CancellationToken cancellationToken = default);
}