using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Locations.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands do módulo Locations.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia CreateAllowedCityRequest para CreateAllowedCityCommand.
    /// </summary>
    /// <param name="request">Requisição de criação de cidade permitida</param>
    /// <returns>CreateAllowedCityCommand com propriedades mapeadas</returns>
    /// <remarks>
    /// A validação de entrada do usuário deve ser feita via FluentValidation antes de chegar neste ponto.
    /// </remarks>
    public static CreateAllowedCityCommand ToCommand(this CreateAllowedCityRequest request)
    {
        return new CreateAllowedCityCommand(
            request.CityName,
            request.StateSigla,
            request.IbgeCode,
            request.Latitude,
            request.Longitude,
            request.ServiceRadiusKm,
            request.IsActive
        );
    }

    /// <summary>
    /// Mapeia UpdateAllowedCityRequest para UpdateAllowedCityCommand.
    /// </summary>
    /// <param name="request">Requisição de atualização de cidade permitida</param>
    /// <param name="id">ID da cidade permitida a ser atualizada</param>
    /// <returns>UpdateAllowedCityCommand com propriedades mapeadas</returns>
    public static UpdateAllowedCityCommand ToCommand(this UpdateAllowedCityRequest request, Guid id)
    {
        return new UpdateAllowedCityCommand
        {
            Id = id,
            CityName = request.CityName,
            StateSigla = request.StateSigla,
            IbgeCode = request.IbgeCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ServiceRadiusKm = request.ServiceRadiusKm,
            IsActive = request.IsActive
        };
    }

    /// <summary>
    /// Mapeia um Guid para DeleteAllowedCityCommand.
    /// </summary>
    /// <param name="id">ID da cidade permitida a ser excluída</param>
    /// <returns>DeleteAllowedCityCommand com ID mapeado</returns>
    public static DeleteAllowedCityCommand ToDeleteCommand(this Guid id)
    {
        return new DeleteAllowedCityCommand { Id = id };
    }
}
