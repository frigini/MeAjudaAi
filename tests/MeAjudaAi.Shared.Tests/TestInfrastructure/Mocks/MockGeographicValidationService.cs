using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;

/// <summary>
/// Implementação mock de IGeographicValidationService para testes de integração.
/// Valida cidades usando comparação de string simples (case-insensitive) contra cidades permitidas.
/// </summary>
public class MockGeographicValidationService : IGeographicValidationService
{
    private static readonly string[] DefaultCities = { "Muriaé", "Itaperuna", "Linhares" };
    private readonly HashSet<string> _allowedCities;

    /// <summary>
    /// Cria um novo serviço mock com as cidades piloto padrão.
    /// </summary>
    public MockGeographicValidationService()
    {
        _allowedCities = new HashSet<string>(DefaultCities, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Cria um novo serviço mock com cidades permitidas personalizadas.
    /// </summary>
    public MockGeographicValidationService(IEnumerable<string> allowedCities)
    {
        _allowedCities = new HashSet<string>(allowedCities, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida se uma cidade está na lista permitida usando comparação case-insensitive.
    /// Implementação simplificada para testes - não chama a API do IBGE.
    /// </summary>
    /// <param name="cityName">Nome da cidade a validar.</param>
    /// <param name="stateSigla">Sigla opcional do estado (ex: "MG", "RJ").</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task<bool> ValidateCityAsync(
        string cityName,
        string? stateSigla,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return Task.FromResult(false);

        // Verificação simples: cidade está na lista permitida (case-insensitive)
        return Task.FromResult(_allowedCities.Contains(cityName));
    }

    /// <summary>
    /// Configura o mock para permitir uma cidade específica.
    /// </summary>
    /// <returns>Esta instância para encadeamento fluente.</returns>
    public MockGeographicValidationService AllowCity(string cityName)
    {
        _allowedCities.Add(cityName);
        return this;
    }

    /// <summary>
    /// Configura o mock para bloquear uma cidade específica.
    /// </summary>
    /// <returns>Esta instância para encadeamento fluente.</returns>
    public MockGeographicValidationService BlockCity(string cityName)
    {
        _allowedCities.Remove(cityName);
        return this;
    }

    /// <summary>
    /// Reseta o mock para as cidades piloto padrão.
    /// </summary>
    /// <returns>Esta instância para encadeamento fluente.</returns>
    public MockGeographicValidationService Reset()
    {
        _allowedCities.Clear();
        foreach (var city in DefaultCities)
        {
            _allowedCities.Add(city);
        }
        return this;
    }
}
