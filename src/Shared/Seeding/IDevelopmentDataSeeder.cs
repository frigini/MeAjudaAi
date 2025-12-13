namespace MeAjudaAi.Shared.Seeding;

/// <summary>
/// Interface para seeding de dados de desenvolvimento
/// </summary>
public interface IDevelopmentDataSeeder
{
    /// <summary>
    /// Executa seed de dados se o banco estiver vazio
    /// </summary>
    Task SeedIfEmptyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Força re-seed de dados (sobrescreve existentes)
    /// </summary>
    Task ForceSeedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o banco possui dados básicos
    /// </summary>
    Task<bool> HasDataAsync(CancellationToken cancellationToken = default);
}
