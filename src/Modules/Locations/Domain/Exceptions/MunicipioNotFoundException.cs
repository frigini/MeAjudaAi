namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um município não é encontrado na API do IBGE.
/// Indica que a cidade pode não existir OU a API do IBGE não possui dados sobre ela.
/// Middleware deve fazer fallback para validação simples (string matching).
/// </summary>
public sealed class MunicipioNotFoundException : Exception
{
    public string CityName { get; }
    public string? StateSigla { get; }

    public MunicipioNotFoundException(string cityName, string? stateSigla = null)
        : base($"Município '{cityName}' não encontrado na API IBGE" +
               (string.IsNullOrEmpty(stateSigla) ? string.Empty : $" para o estado {stateSigla}"))
    {
        CityName = cityName;
        StateSigla = stateSigla;
    }

    public MunicipioNotFoundException(string cityName, string? stateSigla, Exception innerException)
        : base($"Município '{cityName}' não encontrado na API IBGE" +
               (string.IsNullOrEmpty(stateSigla) ? string.Empty : $" para o estado {stateSigla}"), innerException)
    {
        CityName = cityName;
        StateSigla = stateSigla;
    }
}
