using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Locations.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando já existe uma cidade permitida com mesmo nome e estado.
/// </summary>
public sealed class DuplicateAllowedCityException(string cityName, string stateSigla)
    : BadRequestException($"Cidade '{cityName}-{stateSigla}' já cadastrada")
{
    public string CityName { get; } = cityName;
    public string StateSigla { get; } = stateSigla;
}
