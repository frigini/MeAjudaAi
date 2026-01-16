# Error Handling Guide - MeAjudaAi Admin Portal

## Overview

This guide documents the standardized error handling system implemented in the Admin Portal, including error code mapping, retry mechanisms, correlation IDs, and user-friendly error messages.

## Architecture

### Error Handling Services

1. **ErrorHandlingService** - Maps backend error codes to Portuguese messages, implements retry logic
2. **ErrorLoggingService** - Logs errors with correlation IDs and stack traces
3. **ErrorBoundary** - Catches component render errors globally
4. **LiveRegionService** - Announces errors to screen readers

## Error Code Mapping

### Provider Errors

| Error Code | User Message (PT-BR) | Retry? |
|------------|---------------------|--------|
| PROVIDER_NOT_FOUND | Provedor não encontrado. | ❌ |
| PROVIDER_ALREADY_EXISTS | Já existe um provedor com este documento. | ❌ |
| PROVIDER_INVALID_DOCUMENT | Documento inválido. Verifique CPF/CNPJ. | ❌ |
| PROVIDER_ALREADY_VERIFIED | Este provedor já está verificado. | ❌ |
| PROVIDER_VERIFICATION_FAILED | Falha na verificação do provedor. | ✅ |

### Document Errors

| Error Code | User Message (PT-BR) | Retry? |
|------------|---------------------|--------|
| DOCUMENT_NOT_FOUND | Documento não encontrado. | ❌ |
| DOCUMENT_UPLOAD_FAILED | Falha no upload. Tente novamente. | ✅ |
| DOCUMENT_INVALID_FORMAT | Formato inválido. Use PDF, JPG ou PNG. | ❌ |
| DOCUMENT_TOO_LARGE | Arquivo muito grande. Máximo: 10 MB. | ❌ |
| DOCUMENT_ALREADY_VERIFIED | Este documento já foi verificado. | ❌ |

### Network/Transient Errors

| Error Code | User Message (PT-BR) | Retry? |
|------------|---------------------|--------|
| NETWORK_ERROR | Erro de conexão. Verifique sua internet. | ✅ |
| SERVER_ERROR | Erro no servidor. Tente novamente. | ✅ |
| TIMEOUT | Requisição demorou muito. Tente novamente. | ✅ |
| SERVICE_UNAVAILABLE | Serviço temporariamente indisponível. | ✅ |

## Usage Patterns

### 1. Handling API Errors in Effects

**Before (manual error handling)**:
```csharp
[EffectMethod]
public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
{
    var result = await _providersApi.GetProvidersAsync(action.PageNumber, action.PageSize);
    
    if (!result.IsSuccess)
    {
        _snackbar.Add("Erro ao carregar provedores", Severity.Error);
        dispatcher.Dispatch(new LoadProvidersFailureAction("Erro"));
        return;
    }
    
    // Success handling...
}
```

**After (standardized error handling)**:
```csharp
[EffectMethod]
public async Task HandleLoadProvidersAction(LoadProvidersAction action, IDispatcher dispatcher)
{
    // Automatic retry for transient failures (3 attempts with exponential backoff)
    var result = await _errorHandler.ExecuteWithRetryAsync(
        action: () => _providersApi.GetProvidersAsync(action.PageNumber, action.PageSize),
        operation: "Carregar provedores",
        maxAttempts: 3);

    if (result.IsSuccess)
    {
        dispatcher.Dispatch(new LoadProvidersSuccessAction(result.Value));
    }
    else
    {
        var errorMessage = _errorHandler.HandleApiError(result, "carregar provedores");
        _snackbar.Add(errorMessage, Severity.Error);
        dispatcher.Dispatch(new LoadProvidersFailureAction(errorMessage));
    }
}
```

**Benefits**:
- ✅ User-friendly Portuguese messages
- ✅ Automatic retry for transient failures
- ✅ Correlation IDs for error tracking
- ✅ Screen reader announcements
- ✅ Consistent error UX across app

### 2. Retry Logic with Exponential Backoff

```csharp
var result = await _errorHandler.ExecuteWithRetryAsync(
    action: () => _providersApi.CreateProviderAsync(request),
    operation: "Criar provedor",
    maxAttempts: 3);
```

**Retry Strategy**:
- **Attempt 1**: Immediate (0s delay)
- **Attempt 2**: 1s delay (2^0 seconds)
- **Attempt 3**: 2s delay (2^1 seconds)
- **Attempt 4**: 4s delay (2^2 seconds)

Only retries transient errors:
- NETWORK_ERROR
- TIMEOUT
- SERVICE_UNAVAILABLE
- SERVER_ERROR

Non-retryable errors (validation, not found, etc.) fail immediately.

### 3. Manual Error Mapping

```csharp
// Get user-friendly message from error code
var message = _errorHandler.GetUserFriendlyMessage("PROVIDER_NOT_FOUND");
// Returns: "Provedor não encontrado."

// With fallback for unknown codes
var message = _errorHandler.GetUserFriendlyMessage("CUSTOM_ERROR", "Technical error message");
// Returns: "Ocorreu um erro inesperado. Nossa equipe foi notificada."
```

### 4. HTTP Status Code Mapping

```csharp
var message = _errorHandler.GetMessageFromHttpStatus(404);
// Returns: "Recurso não encontrado."

var message = _errorHandler.GetMessageFromHttpStatus(500);
// Returns: "Erro interno do servidor."
```

### 5. Display Error to User

```csharp
// With error code
_errorHandler.DisplayError("Falha ao salvar", "PROVIDER_001");
// Screen reader announces: "Falha ao salvar (Código: PROVIDER_001)"

// Without error code
_errorHandler.DisplayError("Operação cancelada");
// Screen reader announces: "Operação cancelada"
```

## Correlation IDs

Every error is logged with a unique correlation ID for tracking across frontend and backend.

**Frontend Logging**:
```csharp
_logger.LogWarning(
    "API error during {Operation}. CorrelationId: {CorrelationId}. ErrorCode: {ErrorCode}",
    "Carregar provedores",
    correlationId,
    errorCode);
```

**Example Log Output**:
```
warn: MeAjudaAi.Web.Admin.Services.ErrorHandlingService[0]
      API error during Carregar provedores. CorrelationId: 7f8a3b2c1d9e4f5a6b7c8d9e0f1a2b3c. ErrorCode: TIMEOUT
```

**User-Facing Error UI**:
```razor
<MudAlert Severity="Severity.Info">
    <b>ID do Erro:</b> 7f8a3b2c1d9e4f5a6b7c8d9e0f1a2b3c
    <small>Por favor, informe este código ao suporte técnico.</small>
</MudAlert>
```

## Error Severity Levels

### Critical (Component Crashes)
- Caught by ErrorBoundary
- Full-page error UI displayed
- Correlation ID shown
- Stack trace available (debug mode)
- Recovery options: Retry, Reload, Go Home

### Warning (API Failures)
- Caught in Effects
- Snackbar notification
- Screen reader announcement
- Automatic retry for transient errors
- User can continue using app

### Info (Validation Errors)
- Inline field validation
- Form-level error summary
- Screen reader validation announcements
- User corrects data inline

## Testing Error Handling

### Simulate Network Error

```csharp
// In Effect test
var mockApi = new Mock<IProvidersApi>();
mockApi.Setup(x => x.GetProvidersAsync(It.IsAny<int>(), It.IsAny<int>()))
       .ThrowsAsync(new HttpRequestException("Network error"));

// ErrorHandlingService will retry 3 times
```

### Simulate Backend Error Code

```csharp
mockApi.Setup(x => x.CreateProviderAsync(It.IsAny<CreateProviderRequest>()))
       .ReturnsAsync(Response<Guid>.Failure(
           ErrorInfo.Create("PROVIDER_ALREADY_EXISTS", "Provider exists")));

// ErrorHandlingService maps to: "Já existe um provedor com este documento."
```

### Test Retry Logic

```csharp
var attempt = 0;
mockApi.Setup(x => x.GetProvidersAsync(It.IsAny<int>(), It.IsAny<int>()))
       .ReturnsAsync(() =>
       {
           attempt++;
           if (attempt < 3)
               return Response<PagedResult<ModuleProviderDto>>.Failure(
                   ErrorInfo.Create("TIMEOUT", "Request timeout"));
           
           return Response<PagedResult<ModuleProviderDto>>.Success(pagedResult);
       });

// Succeeds on 3rd attempt after 2 retries (1s + 2s delays)
```

## Best Practices

### DO:
✅ Use ErrorHandlingService in all Effects  
✅ Map backend error codes to Portuguese messages  
✅ Enable retry for transient failures  
✅ Include correlation IDs in logs  
✅ Announce errors to screen readers  
✅ Provide recovery options (retry, reload)  
✅ Log technical details for debugging  

### DON'T:
❌ Show technical error messages to users  
❌ Retry validation errors  
❌ Ignore correlation IDs  
❌ Hardcode error messages in components  
❌ Skip error logging  
❌ Block UI during retries  
❌ Retry indefinitely  

## Error Message Guidelines

### User-Friendly Messages Should:
- Be in Portuguese (pt-BR)
- Explain what went wrong
- Suggest action if possible
- Be concise (1-2 sentences)
- Avoid technical jargon
- Be polite and empathetic

### Examples

**Bad**:
```
Error: NullReferenceException at line 42 in ProvidersController
```

**Good**:
```
Não foi possível carregar os provedores. Verifique sua conexão e tente novamente.
```

**Bad**:
```
HTTP 422 Unprocessable Entity
```

**Good**:
```
Dados inválidos. Verifique os campos e tente novamente.
```

## Monitoring and Alerts

### Recommended Alerts:
- Error rate > 5% (last 5 minutes)
- Same error code > 10 times (last 1 minute)
- Retry exhaustion rate > 1% (last 10 minutes)

### Metrics to Track:
- Error count by error code
- Retry success rate
- Average retry attempts
- Error rate by user
- Error rate by operation

### Dashboard Queries (Application Insights):

```kusto
// Error rate by code
traces
| where severityLevel == 2 or severityLevel == 3
| extend ErrorCode = tostring(customDimensions.ErrorCode)
| summarize Count=count() by ErrorCode
| order by Count desc

// Retry statistics
traces
| where message contains "Retrying"
| extend Operation = tostring(customDimensions.Operation)
| extend Attempt = toint(customDimensions.Attempt)
| summarize AvgAttempts=avg(Attempt), MaxAttempts=max(Attempt) by Operation
```

## Future Enhancements

- [ ] Integrate with Sentry for error tracking
- [ ] Add circuit breaker pattern
- [ ] Implement offline queue for failed requests
- [ ] Add error analytics dashboard
- [ ] Support for custom error handlers per module
- [ ] Error report export (CSV/JSON)
- [ ] User feedback on errors (helpful/not helpful)

## Summary

Error handling system implemented:
- ✅ ErrorHandlingService with 30+ error code mappings
- ✅ Automatic retry with exponential backoff (max 3 attempts)
- ✅ Correlation IDs for error tracking
- ✅ User-friendly Portuguese messages
- ✅ Screen reader announcements
- ✅ HTTP status code mapping
- ✅ Integration with Fluxor Effects

**Result**: Consistent, user-friendly error handling across the entire Admin Portal.
