using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface ILocationsApi
{
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}")]
    Task<Result<IReadOnlyList<ModuleAllowedCityDto>>> GetAllAllowedCitiesAsync(
        [Query] bool onlyActive = true,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}/{{id}}")]
    Task<Result<ModuleAllowedCityDto>> GetAllowedCityByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}")]
    Task<Result<Guid>> CreateAllowedCityAsync(
        [Body] CreateAllowedCityRequestDto request,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}/{{id}}")]
    Task<Result<Unit>> UpdateAllowedCityAsync(
        Guid id,
        [Body] UpdateAllowedCityRequestDto request,
        CancellationToken cancellationToken = default);

    [Patch($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}/{{id}}")]
    Task<Result<Unit>> PatchAllowedCityAsync(
        Guid id,
        [Body] PatchAllowedCityRequestDto request,
        CancellationToken cancellationToken = default);

    [Delete($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}/{{id}}")]
    Task<Result<Unit>> DeleteAllowedCityAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.AdminAllowedCities}/state/{{state}}")]
    Task<Result<IReadOnlyList<ModuleAllowedCityDto>>> GetAllowedCitiesByStateAsync(
        string state,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Locations.Base}/search")]
    Task<List<LocationCandidate>> SearchAllowedCitiesAsync(
        [Query] string query,
        CancellationToken cancellationToken = default);
}