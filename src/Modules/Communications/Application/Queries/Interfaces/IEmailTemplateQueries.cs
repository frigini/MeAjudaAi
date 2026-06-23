using MeAjudaAi.Modules.Communications.Domain.Entities;

namespace MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;

public interface IEmailTemplateQueries
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetActiveByKeyAsync(string templateKey, string language = "pt-BR", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailTemplate>> GetAllByKeyAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
}
