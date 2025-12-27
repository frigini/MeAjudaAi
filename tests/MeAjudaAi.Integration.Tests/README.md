# Integration Tests

## Overview

This directory contains integration tests for the MeAjudaAi API. These tests verify that different components work together correctly in a controlled test environment.

## Test Configuration Files

### `appsettings.Testing.json`

Default test configuration with **GeographicRestriction enabled**.

- **Purpose**: Test geographic restriction middleware with real API endpoints (mocked via WireMock)
- **Geographic Restriction**: ENABLED by default
- **External APIs**: Points to WireMock server at localhost:5050 (automatic mocking)
- **Use Case**: Validate geographic restriction logic with mocked API responses
- **Database**: Uses localhost PostgreSQL with test-only credentials

**Note**: This is the only test configuration file. Geographic restriction can be toggled via test properties:
```csharp
protected override bool UseMockGeographicValidation => true; // Default
protected override bool UseMockGeographicValidation => false; // Use real IBGE validation (via WireMock)
```

## Database Credentials

⚠️ **IMPORTANT: Test-Only Credentials**

The connection strings in these files use **test-only, localhost credentials**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meajudaai_test;Username=testuser;Password=test123;Port=5432;Include Error Detail=true"
  }
}
```

**PostgreSQL + PostGIS Configuration:**

- **Docker Image**: `postgis/postgis:15-3.4`
- **PostGIS Extension**: Automatically enabled for geographic data support
- **NetTopologySuite**: Required for SearchProviders module (GeoPoint fields)
- **Error Details**: Connection string includes `Include Error Detail=true` for CI diagnostics

**Security Notes:**

1. **Never use these credentials in production** - They are intentionally simple and meant ONLY for local testing
2. **Localhost only** - The connection string points to `localhost`, ensuring no external database access
3. **CI/CD handling** - CI jobs should read these files from the repository but never log or export their contents
4. **Test database** - The database name includes `_test` suffix to clearly distinguish from production databases
5. **Tracked in git** - These files are intentionally tracked in version control as they contain non-sensitive test data

### Why These Files Are Tracked

- They define the test environment configuration that all developers need
- They use clearly non-production values (localhost, test123, etc.)
- They enable consistent test behavior across different development machines
- They do not contain any production secrets or credentials

## External API Mocking

### WireMock.Net Infrastructure

The test suite uses **WireMock.Net** to mock external HTTP APIs, eliminating network dependencies and enabling consistent test behavior.

#### Global WireMock Server (Available)

**All tests inherit a pre-configured WireMock server from `ApiTestBase`:**

- Automatically started before each test via `InitializeAsync()`
- Running on **localhost:5050**
- Pre-configured with all external API stubs (IBGE, ViaCep, BrasilApi, OpenCep, Nominatim)
- Application configured to use mock URLs in `appsettings.Testing.json`
- Accessible via `WireMock` property in test classes
- Automatic cleanup on test disposal

**Benefits:**
- No network calls to real external APIs during tests
- Consistent, predictable responses
- Fast test execution
- No rate limiting or external dependencies

**Note:** Existing unavailability tests (IbgeUnavailabilityTests, CepProvidersUnavailabilityTests) create their own WireMockFixture instances to override default stubs with custom error scenarios. This is an advanced pattern - most tests can simply rely on the global instance.

**Usage Example (using global WireMock):**
```csharp
public class MyIntegrationTests : ApiTestBase
{
    [Fact]
    public async Task MyTest_WithExternalApi_ShouldWork()
    {
        // WireMock is already running with all stubs configured
        // External API calls are automatically mocked

        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-Location", "Muriaé|MG");
        var response = await Client.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // IBGE validation happened automatically via mock
    }
}
```

#### Custom WireMock Configuration (Optional)

For tests requiring **custom stub configuration** (error scenarios, timeouts, etc.):

```csharp
public class MyUnavailabilityTests : ApiTestBase
{
    [Fact]
    public async Task MyTest_WhenApiReturns500_ShouldHandleGracefully()
    {
        // Override default stub with custom behavior
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithBody("Internal Server Error"));

        // Test continues...
    }
}
```

**Note:** Use `global::WireMock` namespace prefix to avoid ambiguity with the `WireMock` property.

**WireMockFixture** (`Infrastructure/WireMockFixture.cs`):
- HTTP server running on **localhost:5050**
- Comprehensive API stubs for all external services
- Automatic lifecycle management (`IAsyncDisposable`)
- Console logging for debugging
- Reset() method to clear all stubs between tests

**Configured Mock Endpoints:**

1. **IBGE Localidades API** (`/api/v1/localidades/municipios`)
   - Successful lookups: Muriaé (3143906), Itaperuna (3302205), Linhares (3203205)
   - Unknown cities: Empty array response
   - Error scenarios: 500 errors, timeouts (30s delay), malformed JSON
   - Query parameters: `?nome={cityName}&orderBy=nome`

2. **ViaCep API** (`/ws/{cep}/json`)
   - CEP 01310-100: Avenida Paulista, São Paulo/SP
   - Invalid CEPs: `{"erro": true}` response

3. **BrasilApi CEP** (`/api/cep/v1/{cep}`)
   - CEP 01310-100: Structured JSON response with state/city/street

4. **OpenCep API** (`/{cep}.json`)
   - CEP 01310-100: Full address with IBGE code

5. **Nominatim Geocoding** (`/reverse?lat={lat}&lon={lon}&format=json`)
   - São Paulo coordinates (-23.5505, -46.6333)

### Using WireMock in Tests

```csharp
[Collection("Integration")]
public class MyTests : ApiTestBase, IAsyncLifetime
{
    private WireMockFixture? _wireMock;

    public new async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _wireMock = new WireMockFixture();
        await _wireMock.StartAsync();
    }

    [Fact]
    public async Task Test_WithMockedIbge()
    {
        // WireMock server is ready at localhost:5050
        // All configured stubs are available
        Client.DefaultRequestHeaders.Add("X-User-Location", "Muriaé|MG");
        var response = await Client.GetAsync("/api/v1/users");
        response.Should().BeSuccessful();
    }

    public new async ValueTask DisposeAsync()
    {
        if (_wireMock is not null)
            await _wireMock.DisposeAsync();
        await base.DisposeAsync();
    }
}
```

### IBGE Unavailability Tests

`IbgeUnavailabilityTests.cs` validates resilient fallback behavior:

✅ **500 Error Fallback** - Falls back to simple city/state name validation  
✅ **Timeout Fallback** - Handles 30-second timeout gracefully  
✅ **Malformed JSON Fallback** - Handles invalid JSON responses  
✅ **Empty Array Fallback** - Handles "city not found" responses  
✅ **Fail-Closed Security** - Denies unauthorized cities even when IBGE down  

### Configuration

Update `appsettings.Testing.json` to use WireMock:

```json
"ViaCep": { "BaseUrl": "http://localhost:5050" },
"IBGE": { "BaseUrl": "http://localhost:5050/api/v1/localidades/" },
"BrasilApi": { "BaseUrl": "http://localhost:5050" },
"OpenCep": { "BaseUrl": "http://localhost:5050" },
"Nominatim": { "BaseUrl": "http://localhost:5050" }
```

### Troubleshooting

**Port 5050 already in use:**
```powershell
# Find process using port 5050
netstat -ano | findstr :5050
# Kill the process
taskkill /PID <process_id> /F
```

**WireMock not responding:**
- Check WireMock console logs for startup errors
- Verify `_wireMock.StartAsync()` is called in `InitializeAsync()`
- Ensure `DisposeAsync()` is properly called to cleanup

**Stub not matching:**
- WireMock matches exact paths and query parameters
- Check case sensitivity in paths
- Verify query parameter order doesn't matter (WireMock ignores order)

## Running Tests

### Run all integration tests

```bash
dotnet test tests/MeAjudaAi.Integration.Tests
```

### Run with real IBGE validation (via WireMock)

```bash
# Tests use ApiTestBase with UseMockGeographicValidation override
dotnet test tests/MeAjudaAi.Integration.Tests --filter "FullyQualifiedName~GeographicRestrictionConfig"
```

### Run with verbose logging

```bash
ASPNETCORE_ENVIRONMENT=Testing dotnet test tests/MeAjudaAi.Integration.Tests --logger "console;verbosity=detailed"
```

## Test Scenarios

### Geographic Restriction Tests

- ✅ Allowed cities pass validation
- ✅ Blocked cities return HTTP 451
- ✅ Malformed location headers are rejected
- ✅ Empty city/state values are treated as non-matching
- ✅ Feature flag toggles restriction on/off
- 🔲 IBGE service unavailable gracefully degrades (TODO)
- 🔲 External API mocking prevents real network calls (TODO)

### Edge Cases

- Malformed `X-User-Location` headers (e.g., `"City|"`, `"|State"`, `" | "`)
- Empty values after trimming
- Missing location headers (fail-open behavior)
- IBGE API timeouts or errors

## Credentials Security Checklist

- [x] Connection strings use `localhost` only
- [x] Database names include `_test` suffix
- [x] Passwords are generic test values (not production-like)
- [x] Files are documented as test-only
- [x] CI jobs do not log credential values
- [ ] Consider `.gitignore` for user-specific overrides (e.g., `appsettings.Testing.Local.json`)

## Future Improvements

1. **Mock External APIs**: Replace real API calls with WireMock.Net or similar
2. **Parameterized Configuration**: Use test fixtures to toggle configurations programmatically
3. **Test Containers**: Use Testcontainers for PostgreSQL instead of relying on localhost
4. **Credential Rotation**: Even for test credentials, consider periodic rotation
5. **Secret Management**: For sensitive test data, use environment variables or test secret stores
