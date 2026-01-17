# Resilience Architecture - MeAjudaAi Admin Portal

## Overview

This document describes the resilience architecture for the Admin Portal, including retry policies, circuit breakers, timeouts, and error handling patterns.

## Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────┐
│ Blazor Component (Pages/Providers.razor)                          │
│  └─ Dispatches Fluxor Action (LoadProvidersAction)                │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│ Fluxor Effect (ProvidersEffects.HandleLoadProvidersAction)        │
│  └─ Calls ErrorHandlingService.ExecuteWithErrorHandlingAsync      │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│ BUSINESS LOGIC LAYER: ErrorHandlingService                        │
│  ✓ Error Mapping (HTTP status → Portuguese messages)              │
│  ✓ Correlation Tracking (Activity.Current.Id)                     │
│  ✓ Structured Logging                                             │
│  ✗ NO retry (delegated to Polly)                                  │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│ Refit API Client (IProvidersApi)                                  │
│  └─ HttpClient with Polly policies                                │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│ TRANSPORT LAYER: Polly Policies (PollyPolicies.GetCombinedPolicy) │
│                                                                    │
│  1. ⏱️ Timeout Policy (outer)                                      │
│     └─ 30 seconds per request attempt                             │
│                                                                    │
│  2. 🔄 Retry Policy (middle)                                       │
│     └─ 3 attempts with exponential backoff: 2s → 4s → 8s          │
│     └─ Handles: 5xx, 408, network errors, timeout exceptions      │
│     └─ Skips: 4xx (except 408), 409 Conflict                      │
│                                                                    │
│  3. ⚡ Circuit Breaker Policy (inner)                              │
│     └─ Opens after 5 consecutive failures                         │
│     └─ Break duration: 30 seconds                                 │
│     └─ Half-open state: tests with single request                 │
│                                                                    │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│ Backend API (MeAjudaAi.Modules.Providers.Api)                     │
└────────────────────────────────────────────────────────────────────┘
```

## Retry Policy Details

### Default Policy (GetCombinedPolicy)

**Applies to:**
- IProvidersApi
- IServiceCatalogsApi
- ILocationsApi

**Configuration:**
```csharp
Retry: 3 attempts
Backoff: Exponential (2^attempt seconds)
  - Attempt 1: immediate
  - Attempt 2: wait 2 seconds
  - Attempt 3: wait 4 seconds
  - Attempt 4: wait 8 seconds
Total max time: 14s + (4 × request time) + (4 × 30s timeout) = up to 134s worst case

Retries for:
  ✅ HTTP 5xx (Server errors)
  ✅ HTTP 408 (Request timeout)
  ✅ Network exceptions (HttpRequestException)
  ✅ Polly timeout exceptions (TimeoutRejectedException)

Skips retry for:
  ❌ HTTP 4xx (except 408) - Client errors, not transient
  ❌ HTTP 409 Conflict - Resource already exists or modified
  ❌ HTTP 401/403 - Authentication/authorization issues
  ❌ HTTP 404 - Not found (permanent)
```

**Example Timeline:**
```
00:00.000 - Attempt 1 → 503 Service Unavailable
00:02.000 - Attempt 2 (after 2s delay) → 503 Service Unavailable
00:06.000 - Attempt 3 (after 4s delay) → 503 Service Unavailable
00:14.000 - Attempt 4 (after 8s delay) → 200 OK ✅
```

### Upload Policy (GetUploadPolicy)

**Applies to:**
- IDocumentsApi (file uploads)

**Configuration:**
```csharp
Retry: NONE (prevents duplicate uploads)
Timeout: 2 minutes (extended for large files)
Circuit Breaker: Same as GetCombinedPolicy

Why no retry:
- File uploads are not idempotent (POST)
- Retry could create duplicate documents
- Large files take time → higher risk of timeout on retry
- User can manually retry via UI if upload fails
```

## Circuit Breaker Policy

### Purpose
Prevents cascading failures by "opening" the circuit after repeated failures, giving the backend time to recover.

### States

**Closed** (Normal operation):
- All requests pass through
- Failures are counted
- Opens after 5 consecutive failures

**Open** (Circuit tripped):
- All requests fail immediately (no backend call)
- Returns error to caller
- Duration: 30 seconds
- After 30s, transitions to Half-Open

**Half-Open** (Testing):
- Single test request allowed
- If succeeds → Circuit closes (back to Closed)
- If fails → Circuit re-opens for another 30s

### Example Scenario

```
Backend becomes unresponsive:

00:00 - Request 1 fails (5xx) → Retry 3× → Circuit failure count: 1
00:15 - Request 2 fails (5xx) → Retry 3× → Circuit failure count: 2
00:30 - Request 3 fails (5xx) → Retry 3× → Circuit failure count: 3
00:45 - Request 4 fails (5xx) → Retry 3× → Circuit failure count: 4
01:00 - Request 5 fails (5xx) → Retry 3× → Circuit failure count: 5
01:15 - CIRCUIT OPENS ⚡ (5 consecutive failures)

Next 30 seconds (01:15 - 01:45):
- All requests fail immediately (no backend call)
- User sees: "Serviço temporariamente indisponível. Tente novamente."

01:45 - Half-Open state: Test request
  - If succeeds → Circuit closes, normal operation resumes
  - If fails → Circuit re-opens for another 30s
```

## Timeout Policy

### Default Timeout: 30 seconds
- Applied per request attempt (not total time)
- Covers slow backend responses
- Throws TimeoutRejectedException if exceeded

### Upload Timeout: 2 minutes
- Extended for large file uploads
- Prevents timeout on slow networks
- Applies to IDocumentsApi only

### Timeout vs Retry Interaction

```
Scenario: Backend responds slowly (35s)

Without retry:
  00:00 - Request sent
  00:30 - Timeout! ⏱️ (30s exceeded)
  Total: 30s, user sees error

With retry (Polly):
  00:00 - Attempt 1 sent
  00:30 - Timeout! ⏱️
  00:32 - Attempt 2 sent (2s backoff)
  01:02 - Timeout! ⏱️
  01:06 - Attempt 3 sent (4s backoff)
  01:36 - Timeout! ⏱️
  01:44 - Attempt 4 sent (8s backoff)
  02:14 - Success ✅ (backend recovered)
  Total: 134s worst case
```

## Error Handling Flow

### Success Path
```csharp
1. Component dispatches LoadProvidersAction
2. Effect calls ErrorHandlingService.ExecuteWithErrorHandlingAsync
3. ErrorHandlingService calls IProvidersApi.GetProvidersAsync
4. Polly HttpClient sends request → Backend returns 200 OK
5. Result<T>.Success flows back to Effect
6. Effect dispatches LoadProvidersSuccessAction
7. Reducer updates state
8. Component re-renders with data ✅
```

### Transient Error Path (Retry Success)
```csharp
1. Component dispatches LoadProvidersAction
2. Effect calls ErrorHandlingService.ExecuteWithErrorHandlingAsync
3. ErrorHandlingService calls IProvidersApi.GetProvidersAsync
4. Polly HttpClient:
   a. Attempt 1 → 503 Service Unavailable ❌
   b. Wait 2 seconds ⏳
   c. Attempt 2 → 503 Service Unavailable ❌
   d. Wait 4 seconds ⏳
   e. Attempt 3 → 200 OK ✅
5. Result<T>.Success flows back to Effect
6. Effect dispatches LoadProvidersSuccessAction
7. User never saw error (transparent retry) 🎉
```

### Permanent Error Path (No Retry)
```csharp
1. Component dispatches LoadProvidersAction
2. Effect calls ErrorHandlingService.ExecuteWithErrorHandlingAsync
3. ErrorHandlingService calls IProvidersApi.GetProvidersAsync
4. Polly HttpClient → Backend returns 404 Not Found
5. Polly skips retry (404 is not transient)
6. Result<T>.Failure(Error.NotFound) flows back to Effect
7. ErrorHandlingService.HandleApiError maps 404 → "Recurso não encontrado."
8. Effect shows error via Snackbar
9. Effect dispatches LoadProvidersFailureAction
10. User sees Portuguese error message 🇧🇷
```

### Circuit Open Path (Fast Fail)
```csharp
1. Circuit is already OPEN (5 previous failures)
2. Component dispatches LoadProvidersAction
3. Effect calls ErrorHandlingService.ExecuteWithErrorHandlingAsync
4. Polly immediately throws BrokenCircuitException (no backend call)
5. ErrorHandlingService catches exception
6. Maps to "Serviço temporariamente indisponível. Tente novamente mais tarde."
7. Effect shows error via Snackbar
8. User sees error instantly (no 30s wait) ⚡
```

## Logging Examples

### Polly Retry Logging
```
warn: MeAjudaAi.Web.Admin.Services.Resilience.PollyPolicies[0]
      ⚠️ Retry 1/3 after 2s delay. Request: GET /api/providers?page=1. Reason: 503
```

### Circuit Breaker Logging
```
error: MeAjudaAi.Web.Admin.Services.Resilience.PollyPolicies[0]
       🔴 Circuit breaker opened! Will retry after 30s. Reason: 503

info: MeAjudaAi.Web.Admin.Services.Resilience.PollyPolicies[0]
      🟡 Circuit breaker half-open - testing connection

info: MeAjudaAi.Web.Admin.Services.Resilience.PollyPolicies[0]
      ✅ Circuit breaker reset - connection restored
```

### ErrorHandlingService Logging
```
info: MeAjudaAi.Web.Admin.Services.ErrorHandlingService[0]
      Operação 'carregar provedores' bem-sucedida [CorrelationId: 00-7f8a3b2c-01]

error: MeAjudaAi.Web.Admin.Services.ErrorHandlingService[0]
       Operação 'carregar provedores' falhou com status 503: Service Unavailable [CorrelationId: 00-7f8a3b2c-01]
```

## Configuration

### Registering Polly Policies

**ServiceCollectionExtensions.cs:**
```csharp
public static IServiceCollection AddApiClient<TClient>(
    this IServiceCollection services,
    string baseUrl,
    bool useUploadPolicy = false)
    where TClient : class
{
    var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<TClient>();

    services.AddRefitClient<TClient>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
        .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
        .AddHttpMessageHandler(() => new PollyLoggingHandler())
        .AddPolicyHandler(useUploadPolicy
            ? PollyPolicies.GetUploadPolicy(logger)
            : PollyPolicies.GetCombinedPolicy(logger));

    return services;
}
```

**Program.cs:**
```csharp
// Default policy (retry + circuit breaker + timeout)
builder.Services.AddApiClient<IProvidersApi>(apiBaseUrl);
builder.Services.AddApiClient<IServiceCatalogsApi>(apiBaseUrl);
builder.Services.AddApiClient<ILocationsApi>(apiBaseUrl);

// Upload policy (no retry, extended timeout)
builder.Services.AddApiClient<IDocumentsApi>(apiBaseUrl, useUploadPolicy: true);
```

## Metrics and Monitoring

### Recommended Metrics

**Retry Metrics:**
- Retry attempt count per endpoint
- Retry success rate (succeeded after retry vs failed after all attempts)
- Average retry duration

**Circuit Breaker Metrics:**
- Circuit state changes (closed → open → half-open → closed)
- Time spent in open state
- Circuit open count per hour

**Error Metrics:**
- Error rate by HTTP status code
- Error rate by endpoint
- Correlation ID tracking (frontend → backend)

### Application Insights Queries

**Retry Success Rate:**
```kusto
traces
| where message contains "Retry"
| extend Endpoint = tostring(customDimensions.RequestUri)
| extend Attempt = toint(customDimensions.RetryCount)
| summarize TotalRetries=count(), MaxAttempts=max(Attempt) by Endpoint
| extend RetrySuccessRate = (TotalRetries - MaxAttempts) * 100.0 / TotalRetries
```

**Circuit Breaker State Changes:**
```kusto
traces
| where message contains "Circuit breaker"
| extend State = case(
    message contains "opened", "OPENED",
    message contains "half-open", "HALF-OPEN",
    message contains "reset", "CLOSED",
    "UNKNOWN")
| summarize Count=count() by State, bin(timestamp, 1h)
```

**Error Rate by Status Code:**
```kusto
traces
| where severityLevel >= 2  // Warning and above
| extend StatusCode = toint(customDimensions.StatusCode)
| summarize ErrorCount=count() by StatusCode, bin(timestamp, 5m)
| order by timestamp desc
```

## Testing Resilience

### Unit Tests

**Test Retry Success:**
```csharp
[Fact]
public async Task LoadProviders_RetrySucceeds_AfterTransientError()
{
    // Arrange
    var attempt = 0;
    _mockProvidersApi
        .Setup(x => x.GetProvidersAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(() =>
        {
            attempt++;
            if (attempt < 3)
                return Result<PagedResult<ProviderDto>>.Failure(
                    Error.ServiceUnavailable("Service unavailable"));
            
            return Result<PagedResult<ProviderDto>>.Success(_pagedResult);
        });

    // Act
    var result = await _errorHandlingService.ExecuteWithErrorHandlingAsync(
        () => _mockProvidersApi.Object.GetProvidersAsync(1, 10),
        "carregar provedores");

    // Assert
    result.IsSuccess.Should().BeTrue();
    attempt.Should().Be(3); // Polly retried 2 times (total 3 attempts)
}
```

**Test No Retry for 4xx:**
```csharp
[Fact]
public async Task LoadProviders_NoRetry_For404NotFound()
{
    // Arrange
    var attempt = 0;
    _mockProvidersApi
        .Setup(x => x.GetProvidersAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(() =>
        {
            attempt++;
            return Result<PagedResult<ProviderDto>>.Failure(
                Error.NotFound("Provider not found"));
        });

    // Act
    var result = await _errorHandlingService.ExecuteWithErrorHandlingAsync(
        () => _mockProvidersApi.Object.GetProvidersAsync(1, 10),
        "carregar provedores");

    // Assert
    result.IsSuccess.Should().BeFalse();
    attempt.Should().Be(1); // No retry for 404
}
```

### Integration Tests

Use Polly's `SimulateFailureHandler` to test circuit breaker:

```csharp
[Fact]
public async Task CircuitBreaker_Opens_After5ConsecutiveFailures()
{
    // Simulate 5 consecutive failures
    for (int i = 0; i < 5; i++)
    {
        var result = await _errorHandlingService.ExecuteWithErrorHandlingAsync(
            () => _mockProvidersApi.Object.GetProvidersAsync(1, 10),
            "carregar provedores");
        
        result.IsSuccess.Should().BeFalse();
    }

    // 6th attempt should fail immediately (circuit open)
    var stopwatch = Stopwatch.StartNew();
    var finalResult = await _errorHandlingService.ExecuteWithErrorHandlingAsync(
        () => _mockProvidersApi.Object.GetProvidersAsync(1, 10),
        "carregar provedores");
    stopwatch.Stop();

    finalResult.IsSuccess.Should().BeFalse();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Fast fail
}
```

## Best Practices

### DO:
✅ Use Polly policies for ALL HttpClient instances  
✅ Let Polly handle retry at transport level  
✅ Use ErrorHandlingService for error mapping only  
✅ Log retry attempts with context (endpoint, status code)  
✅ Monitor circuit breaker state changes  
✅ Set appropriate timeouts per endpoint type  
✅ Test resilience patterns in integration tests  
✅ Document retry behavior for frontend developers  

### DON'T:
❌ Implement retry logic in business layer (duplicate)  
❌ Retry non-idempotent operations (POST/PUT/DELETE) by default  
❌ Set timeout too low (causes false positives)  
❌ Ignore circuit breaker state in monitoring  
❌ Stack multiple retry layers (causes 9× attempts)  
❌ Use same policy for uploads (needs extended timeout, no retry)  
❌ Skip correlation ID tracking  

## Future Enhancements

- [ ] Add bulkhead isolation pattern (limit concurrent requests per endpoint)
- [ ] Implement rate limiting (prevent overwhelming backend during recovery)
- [ ] Add fallback policy (return cached data when circuit is open)
- [ ] Enhance monitoring with Polly.Extensions.Http metrics
- [ ] Add health checks for circuit breaker status
- [ ] Implement graceful degradation (disable features when circuit is open)
- [ ] Add user notification for prolonged outages

## Summary

**Single-Layer Resilience Architecture:**
- **Transport Layer (Polly)**: Retry (3×) + Circuit Breaker + Timeout
- **Business Layer (ErrorHandlingService)**: Error Mapping + Correlation Tracking
- **Result**: Exactly 3 total attempts (not 9 with double stacking)

**Benefits:**
- ✅ Clear separation of concerns
- ✅ Standard Polly patterns
- ✅ Better performance (fewer retries)
- ✅ Easier to test and debug
- ✅ Transparent retry for transient errors
- ✅ Fast fail for permanent errors (circuit breaker)
- ✅ User-friendly Portuguese error messages
