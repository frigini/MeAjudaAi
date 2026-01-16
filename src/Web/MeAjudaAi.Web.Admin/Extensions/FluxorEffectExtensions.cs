using Fluxor;
using MeAjudaAi.Web.Admin.Services.Resilience;
using MudBlazor;
using Polly.CircuitBreaker;
using System.Net;

namespace MeAjudaAi.Web.Admin.Extensions;

/// <summary>
/// Extensões para lidar com erros de API nos efeitos do Fluxor
/// </summary>
public static class FluxorEffectExtensions
{
    /// <summary>
    /// Executa uma ação de API com tratamento de erros e notificações
    /// </summary>
    public static async Task<TResult?> ExecuteApiCallAsync<TResult>(
        this IDispatcher dispatcher,
        Func<Task<TResult>> apiCall,
        ISnackbar snackbar,
        string? operationName = null,
        Action<TResult>? onSuccess = null,
        Action<Exception>? onError = null)
    {
        try
        {
            var result = await apiCall();
            onSuccess?.Invoke(result);
            return result;
        }
        catch (BrokenCircuitException ex)
        {
            snackbar.Add(
                ApiErrorMessages.CircuitBreakerOpen,
                Severity.Error,
                config => config.Icon = Icons.Material.Filled.CloudOff);
            
            onError?.Invoke(ex);
            return default;
        }
        catch (TimeoutException ex)
        {
            snackbar.Add(
                ApiErrorMessages.Timeout,
                Severity.Warning,
                config => config.Icon = Icons.Material.Filled.HourglassEmpty);
            
            onError?.Invoke(ex);
            return default;
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            var message = ApiErrorMessages.GetFriendlyMessage(ex.StatusCode.Value, operationName);
            
            var severity = ex.StatusCode.Value switch
            {
                HttpStatusCode.BadRequest or HttpStatusCode.Conflict => Severity.Warning,
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Severity.Error,
                _ => Severity.Error
            };

            snackbar.Add(message, severity);
            onError?.Invoke(ex);
            return default;
        }
        catch (HttpRequestException ex)
        {
            snackbar.Add(
                ApiErrorMessages.NetworkError,
                Severity.Error,
                config => config.Icon = Icons.Material.Filled.WifiOff);
            
            onError?.Invoke(ex);
            return default;
        }
        catch (Exception ex)
        {
            snackbar.Add(
                ApiErrorMessages.GetFriendlyMessage(ex, operationName),
                Severity.Error);
            
            onError?.Invoke(ex);
            return default;
        }
    }

    /// <summary>
    /// Executa uma ação de API void com tratamento de erros
    /// </summary>
    public static async Task<bool> ExecuteApiCallAsync(
        this IDispatcher dispatcher,
        Func<Task> apiCall,
        ISnackbar snackbar,
        string? operationName = null,
        Action? onSuccess = null,
        Action<Exception>? onError = null)
    {
        try
        {
            await apiCall();
            onSuccess?.Invoke();
            return true;
        }
        catch (BrokenCircuitException ex)
        {
            snackbar.Add(
                ApiErrorMessages.CircuitBreakerOpen,
                Severity.Error,
                config => config.Icon = Icons.Material.Filled.CloudOff);
            
            onError?.Invoke(ex);
            return false;
        }
        catch (TimeoutException ex)
        {
            snackbar.Add(
                ApiErrorMessages.Timeout,
                Severity.Warning,
                config => config.Icon = Icons.Material.Filled.HourglassEmpty);
            
            onError?.Invoke(ex);
            return false;
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            var message = ApiErrorMessages.GetFriendlyMessage(ex.StatusCode.Value, operationName);
            
            var severity = ex.StatusCode.Value switch
            {
                HttpStatusCode.BadRequest or HttpStatusCode.Conflict => Severity.Warning,
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Severity.Error,
                _ => Severity.Error
            };

            snackbar.Add(message, severity);
            onError?.Invoke(ex);
            return false;
        }
        catch (HttpRequestException ex)
        {
            snackbar.Add(
                ApiErrorMessages.NetworkError,
                Severity.Error,
                config => config.Icon = Icons.Material.Filled.WifiOff);
            
            onError?.Invoke(ex);
            return false;
        }
        catch (Exception ex)
        {
            snackbar.Add(
                ApiErrorMessages.GetFriendlyMessage(ex, operationName),
                Severity.Error);
            
            onError?.Invoke(ex);
            return false;
        }
    }
}
