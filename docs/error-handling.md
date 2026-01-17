# Error Handling Guide - MeAjudaAi Admin Portal

## Overview

This guide documents the standardized error handling system implemented in the Admin Portal, including error code mapping, correlation IDs, user-friendly error messages, and resilience patterns.

## Architecture

### Resilience Layers

The Admin Portal uses a **two-layer resilience architecture**:

1. **Transport Layer (Polly Policies)** - HttpClient level
   - ✅ Retry logic (3 attempts: 2s → 4s → 8s backoff)
   - ✅ Circuit breaker (opens after 5 failures, 30s break)
   - ✅ Timeout (30s for normal, 2min for uploads)
   - ✅ Handles network errors and transient HTTP failures (5xx, 408)

2. **Business Logic Layer (ErrorHandlingService)** - Application level
   - ✅ Error mapping (HTTP status → Portuguese messages)
   - ✅ Correlation tracking (Activity.Current.Id)
   - ✅ Structured logging
   - ❌ NO retry (handled by Polly at HttpClient level)

### Error Handling Services

1. **ErrorHandlingService** - Maps backend error codes to Portuguese messages, correlation tracking
2. **ErrorLoggingService** - Logs errors with correlation IDs and stack traces
3. **ErrorBoundary** - Catches component render errors globally
4. **LiveRegionService** - Announces errors to screen readers

## Resilience Policy Details

### Polly Policies (HttpClient Level)

**GetCombinedPolicy()** - Default for API clients:
```csharp
// 1. Retry Policy: 3 attempts with exponential backoff
//    - Attempt 1: immediate
//    - Attempt 2: wait 2 seconds
//    - Attempt 3: wait 4 seconds
//    - Attempt 4: wait 8 seconds
//    Total: up to 14 seconds + request time

// 2. Circuit Breaker: Opens after 5 consecutive failures
//    - Break duration: 30 seconds
//    - Half-open state: tests with single request

// 3. Timeout: 30 seconds per request
//    - Applied per attempt (not total time)

// Handled errors:
// - HTTP 5xx (Server errors)
// - HTTP 408 (Request timeout)
// - Network exceptions (HttpRequestException)
// - Polly timeout exceptions
```

**GetUploadPolicy()** - For file uploads:
```csharp
// 1. NO retry (prevents duplicate uploads)
// 2. Circuit Breaker: Same as GetCombinedPolicy (5 failures, 30s break)
// 3. Timeout: 2 minutes (extended for large files)
```

### ErrorHandlingService (Business Logic Level)

**ExecuteWithErrorHandlingAsync()** - Error mapping and correlation:
```csharp
// 1. Correlation tracking (Activity.Current.Id)
// 2. Structured logging with context
// 3. HTTP status → Portuguese messages
// 4. NO retry (Polly handles at HttpClient level)

// Benefits of single retry layer:
// ✅ Exactly 3 total attempts (not 9 with double stacking)
// ✅ Retry at transport level (network, timeout, 5xx)
// ✅ Error mapping at business level (user-friendly messages)
// ✅ Clear separation of concerns
```

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
    // Polly handles retry at HttpClient level (3 attempts with exponential backoff)
    // ErrorHandlingService provides error mapping and correlation tracking
    var result = await _errorHandler.ExecuteWithErrorHandlingAsync(
        () => _providersApi.GetProvidersAsync(action.PageNumber, action.PageSize),
        "carregar provedores");

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
- ✅ Automatic retry via Polly (transport level)
- ✅ Correlation IDs for error tracking
- ✅ Screen reader announcements
- ✅ Consistent error UX across app
- ✅ Exactly 3 attempts (not 9 with double stacking)

### 2. Error Mapping with Correlation Tracking

**Non-Idempotent Methods** (NO retry by default):
- ❌ POST - Create operations (duplicate resources)
- ❌ PUT - Update operations (conflicting updates)
- ❌ DELETE - Delete operations (404 on retry)
- ❌ PATCH - Partial updates (conflicting changes)

**Retry Strategy**:
- **Attempt 1**: Immediate (0s delay)
- **Attempt 2**: 1s delay (2^0 seconds)
- **Attempt 3**: 2s delay (2^1 seconds)
- **Attempt 4**: 4s delay (2^2 seconds)

**Only retries transient errors**:
- 5xx Server Errors (500, 502, 503, 504)
- 408 Request Timeout

**NEVER retries**:
- 409 Conflict (resource already exists or was modified)
- 400 Bad Request (validation errors)
- 401 Unauthorized (authentication required)
- 403 Forbidden (insufficient permissions)
- 404 Not Found (resource doesn't exist)
- 422 Unprocessable Entity (business rule violation)

**Non-retryable errors fail immediately to prevent:**
- Duplicate resource creation (POST retry → 2 users created)
- Double payments (POST retry → charged twice)
- Data corruption (PUT retry → conflicting updates)
- Unexpected state (DELETE retry → 404 on second attempt)

### 3. Manual Error Mapping

```csharp
// Display user-friendly message for API error
var result = await _errorHandler.ExecuteWithErrorHandlingAsync(
    () => _api.GetDataAsync(),
    "carregar dados");

if (!result.IsSuccess)
{
    var message = _errorHandler.HandleApiError(result, "carregar dados");
    _snackbar.Add(message, Severity.Error);
    // Returns: "Recurso não encontrado." (for 404)
}
```

### 3. HTTP Status Code Mapping

```csharp
var message = _errorHandler.GetUserFriendlyMessage(404);
// Returns: "Recurso não encontrado."

var message = _errorHandler.GetUserFriendlyMessage(500);
// Returns: "Erro interno do servidor. Nossa equipe foi notificada."

var message = _errorHandler.GetUserFriendlyMessage(503, "Backend custom message");
// Returns: "Backend custom message" (backend message takes priority)
```

### 4. Display Error to User

```csharp
// Error message automatically shown via Snackbar
var errorMessage = _errorHandler.HandleApiError(result, "salvar provedor");
_snackbar.Add(errorMessage, Severity.Error);
// Also announces to screen readers via LiveRegionService
```

## Correlation IDs

Every error is logged with a unique correlation ID (Activity.Current.Id) for tracking across frontend and backend.

**Frontend Logging**:
```csharp
_logger.LogError(
    "Operação '{Operation}' falhou com status {StatusCode}: {ErrorMessage} [CorrelationId: {CorrelationId}]",
    "carregar provedores",
    statusCode,
    errorMessage,
    correlationId);
```

**Example Log Output**:
```
error: MeAjudaAi.Web.Admin.Services.ErrorHandlingService[0]
       Operação 'carregar provedores' falhou com status 503: Service Unavailable [CorrelationId: 00-7f8a3b2c1d9e4f5a6b7c8d9e0f1a2b3c-1a2b3c4d5e6f7a8b-01]
```

**Polly Retry Logging**:
```
warn: MeAjudaAi.Web.Admin.Services.Resilience.PollyPolicies[0]
      ⚠️ Retry 1/3 after 2s delay. Request: https://api.meajudaai.com/providers?page=1. Reason: 503
```

**User-Facing Error UI** (in ErrorBoundaryContent):
```razor
<MudAlert Severity="Severity.Info">
    <b>ID de Rastreamento:</b> @ErrorState.CorrelationId
    <small>Informe este código ao suporte técnico.</small>
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
## Testing Error Handling

### Simulate Network Error

```csharp
// In Effect test
var mockApi = new Mock<IProvidersApi>();
mockApi.Setup(x => x.GetProvidersAsync(It.IsAny<int>(), It.IsAny<int>()))
       .ThrowsAsync(new HttpRequestException("Network error"));

// Polly will retry 3 times at HttpClient level
// ErrorHandlingService maps to: "Erro de conexão. Verifique sua internet."
```

### Simulate Backend Error

```csharp
mockApi.Setup(x => x.CreateProviderAsync(It.IsAny<CreateProviderRequest>()))
       .ReturnsAsync(Result<Guid>.Failure(
           Error.Conflict("Provider already exists")));

// No retry for 409 Conflict (not transient)
// ErrorHandlingService maps to: "Conflito. O recurso já existe ou foi modificado."
```

### Test Retry Logic

```csharp
// Simulate transient error that resolves after 2 attempts
var attempt = 0;
mockApi.Setup(x => x.GetProvidersAsync(It.IsAny<int>(), It.IsAny<int>()))
       .ReturnsAsync(() =>
       {
           attempt++;
           if (attempt < 3)
               return Result<PagedResult<ProviderDto>>.Failure(
                   Error.ServiceUnavailable("Service temporarily unavailable"));
           
           return Result<PagedResult<ProviderDto>>.Success(pagedResult);
       });

// Polly retries 2 times (attempt 1 fails, attempt 2 fails, attempt 3 succeeds)
// Total backoff: 2s + 4s = 6 seconds
```

## Architecture Decision: Single Retry Layer

**Problem**: Previously had double-retry stacking:
- Polly retry (3 attempts) × ErrorHandlingService retry (3 attempts) = **9 total attempts**
- Excessive backend load, slow UX, unnecessary network traffic

**Solution**: Moved all retry logic to Polly (HttpClient level):
- ✅ Retry at transport level (network errors, timeouts, 5xx)
- ✅ Circuit breaker for fail-fast behavior
- ✅ ErrorHandlingService handles only error mapping + correlation tracking
- ✅ **Exactly 3 total attempts** (not 9)

**Benefits**:
- Clear separation of concerns
- Standard Polly patterns
- Better performance (fewer retries)
- Easier to test and debug

## Best Practices

### DO:
✅ Use ErrorHandlingService.ExecuteWithErrorHandlingAsync in all Effects  
✅ Let Polly handle retry at HttpClient level (3 attempts)  
✅ Map HTTP status codes to Portuguese messages  
✅ Include correlation IDs (Activity.Current.Id) in logs  
✅ Announce errors to screen readers via LiveRegionService  
✅ Provide recovery options (ErrorBoundary.Recover())  
✅ Log technical details for debugging  
✅ Trust Polly for transient error detection (5xx, 408, network)

### DON'T:
❌ Show technical error messages to users  
❌ Implement retry logic in business layer (Polly handles it)  
❌ Ignore correlation IDs  
❌ Hardcode error messages in components  
❌ Skip error logging  
❌ Block UI during retries (Polly handles async)  
❌ Retry non-transient errors (400, 404, 409)  
❌ Stack multiple retry layers (causes 9× attempts)  

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
