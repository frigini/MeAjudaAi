using MeAjudaAi.Modules.Communications.Domain.Entities;

namespace MeAjudaAi.Modules.Communications.Domain.Repositories;

/// <summary>
/// Repositório de templates de e-mail.
/// </summary>
public interface IEmailTemplateRepository
{
    /// <summary>
    /// Adiciona um novo template.
    /// </summary>
    Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna um template ativo pelo TemplateKey e Language.
    /// Prefere OverrideKey quando disponível.
    /// </summary>
    Task<EmailTemplate?> GetActiveByKeyAsync(
        string templateKey,
        string language = "pt-BR",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os templates de uma determinada chave.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetAllByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os templates registrados.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um template pelo ID (protegido se for template de sistema).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
