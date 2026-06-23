namespace MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;

public interface IPaymentsHealthQueries
{
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}