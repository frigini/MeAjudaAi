using Fluxor;
using static MeAjudaAi.Web.Admin.Features.Providers.ProvidersActions;

namespace MeAjudaAi.Web.Admin.Features.Providers;

/// <summary>
/// Reducers do Fluxor para a feature de Providers.
/// Funções puras que modificam o estado em resposta a actions.
/// </summary>
public static class ProvidersReducers
{
    /// <summary>
    /// Reducer para LoadProvidersAction: marca estado como loading
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceLoadProvidersAction(ProvidersState state, LoadProvidersAction action)
    {
        return state with 
        { 
            IsLoading = true, 
            ErrorMessage = null 
        };
    }

    /// <summary>
    /// Reducer para LoadProvidersSuccessAction: atualiza lista de providers
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceLoadProvidersSuccessAction(ProvidersState state, LoadProvidersSuccessAction action)
    {
        return state with
        {
            IsLoading = false,
            Providers = action.Providers,
            TotalCount = action.TotalCount,
            CurrentPage = action.CurrentPage,
            PageSize = action.PageSize,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Reducer para LoadProvidersFailureAction: armazena mensagem de erro
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceLoadProvidersFailureAction(ProvidersState state, LoadProvidersFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage,
            Providers = []
        };
    }

    /// <summary>
    /// Reducer para NextPageAction: incrementa página se possível
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceNextPageAction(ProvidersState state, NextPageAction action)
    {
        if (!state.HasNextPage)
            return state;

        return state with { CurrentPage = state.CurrentPage + 1 };
    }

    /// <summary>
    /// Reducer para PreviousPageAction: decrementa página se possível
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReducePreviousPageAction(ProvidersState state, PreviousPageAction action)
    {
        if (!state.HasPreviousPage)
            return state;

        return state with { CurrentPage = state.CurrentPage - 1 };
    }

    /// <summary>
    /// Reducer para GoToPageAction: vai para página específica
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceGoToPageAction(ProvidersState state, GoToPageAction action)
    {
        if (action.PageNumber < 1 || action.PageNumber > state.TotalPages)
            return state;

        return state with { CurrentPage = action.PageNumber };
    }

    /// <summary>
    /// Reducer para DeleteProviderAction: marca estado como deletando
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceDeleteProviderAction(ProvidersState state, DeleteProviderAction action)
    {
        return state with
        {
            IsDeleting = true,
            DeletingProviderId = action.ProviderId,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Reducer para DeleteProviderSuccessAction: limpa estado de deleção
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceDeleteProviderSuccessAction(ProvidersState state, DeleteProviderSuccessAction action)
    {
        return state with
        {
            IsDeleting = false,
            DeletingProviderId = null,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Reducer para DeleteProviderFailureAction: armazena erro e limpa estado de deleção
    /// </summary>
    [ReducerMethod]
    public static ProvidersState ReduceDeleteProviderFailureAction(ProvidersState state, DeleteProviderFailureAction action)
    {
        return state with
        {
            IsDeleting = false,
            DeletingProviderId = null,
            ErrorMessage = action.ErrorMessage
        };
    }
}
