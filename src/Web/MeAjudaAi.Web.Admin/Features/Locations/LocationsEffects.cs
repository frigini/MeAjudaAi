using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using static MeAjudaAi.Web.Admin.Features.Locations.LocationsActions;

namespace MeAjudaAi.Web.Admin.Features.Locations;

/// <summary>
/// Effects Fluxor para operações assíncronas de cidades permitidas.
/// Executa chamadas à API e dispatcha actions de sucesso/falha.
/// </summary>
public sealed class LocationsEffects(ILocationsApi locationsApi, IDispatcher dispatcher)
{
    /// <summary>Carrega todas as cidades permitidas do backend</summary>
    [EffectMethod]
    public async Task HandleLoadAllowedCitiesAction(LoadAllowedCitiesAction action, CancellationToken cancellationToken)
    {
        var result = await locationsApi.GetAllAllowedCitiesAsync(action.OnlyActive, cancellationToken);

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
}
