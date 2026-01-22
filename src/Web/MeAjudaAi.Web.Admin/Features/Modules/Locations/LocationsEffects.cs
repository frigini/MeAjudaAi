using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Web.Admin.Extensions;
using MudBlazor;
using static MeAjudaAi.Web.Admin.Features.Modules.Locations.LocationsActions;

namespace MeAjudaAi.Web.Admin.Features.Modules.Locations;

/// <summary>
/// Effects Fluxor para operações assíncronas de cidades permitidas.
/// Executa chamadas à API e dispatcha actions de sucesso/falha.
/// </summary>
public sealed class LocationsEffects
{
    private readonly ILocationsApi _locationsApi;
    private readonly ISnackbar _snackbar;

    public LocationsEffects(ILocationsApi locationsApi, ISnackbar snackbar)
    {
        _locationsApi = locationsApi;
        _snackbar = snackbar;
    }

    /// <summary>Carrega todas as cidades permitidas do backend</summary>
    [EffectMethod]
    public async Task HandleLoadAllowedCitiesAction(LoadAllowedCitiesAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _locationsApi.GetAllAllowedCitiesAsync(action.OnlyActive, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                dispatcher.Dispatch(new LoadAllowedCitiesSuccessAction(result.Value));
            }
            else
            {
                var errorMessage = result.Error?.Message ?? "Erro desconhecido ao carregar cidades permitidas";
                dispatcher.Dispatch(new LoadAllowedCitiesFailureAction(errorMessage));
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao carregar cidades permitidas: {ex.Message}";
            dispatcher.Dispatch(new LoadAllowedCitiesFailureAction(errorMessage));
        }
    }

    /// <summary>Effect para excluir cidade permitida</summary>
    [EffectMethod]
    public async Task HandleDeleteAllowedCityAction(DeleteAllowedCityAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _locationsApi.DeleteAllowedCityAsync(action.CityId),
            operationName: "Excluir cidade",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new DeleteAllowedCitySuccessAction(action.CityId));
                _snackbar.Add("Cidade excluída com sucesso!", Severity.Success);
                dispatcher.Dispatch(new RemoveAllowedCityAction(action.CityId));
            },
            onError: ex =>
            {
                var userMessage = "Erro ao excluir cidade. Tente novamente.";
                dispatcher.Dispatch(new DeleteAllowedCityFailureAction(action.CityId, userMessage));
            });
    }

    /// <summary>Effect para alternar ativação de cidade</summary>
    [EffectMethod]
    public async Task HandleToggleCityActivationAction(ToggleCityActivationAction action, IDispatcher dispatcher)
    {
        var updateRequest = new UpdateAllowedCityRequestDto(
            action.City.City,
            action.City.State,
            action.City.Country,
            action.City.Latitude,
            action.City.Longitude,
            action.City.ServiceRadiusKm,
            action.Activate
        );

        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _locationsApi.UpdateAllowedCityAsync(action.CityId, updateRequest),
            operationName: action.Activate ? "Ativar cidade" : "Desativar cidade",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new ToggleCityActivationSuccessAction(action.CityId, action.Activate));
                _snackbar.Add($"Cidade {(action.Activate ? "ativada" : "desativada")} com sucesso!", Severity.Success);
                dispatcher.Dispatch(new UpdateCityActiveStatusAction(action.CityId, action.Activate));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new ToggleCityActivationFailureAction(action.CityId, ex.Message));
            });
    }
}
