# Integration Tests

## Overview

This directory contains integration tests for the MeAjudaAi API. These tests verify that different components work together correctly in a controlled test environment.

## Project Structure

```
tests/MeAjudaAi.Integration.Tests/
├── Base/
│   ├── BaseApiTest.cs                  # Main base class for integration tests
│   ├── BaseApiTest.TestDataHelpers.cs  # Helper methods for creating test data via API/DB
│   ├── BasePerformanceTest.cs          # Base for performance tests
│   └── BaseSharedTest.cs               # Base for shared/cross-module tests
├── Infrastructure/
│   ├── BaseAspireIntegrationTest.cs    # Base for Aspire integration tests
│   ├── Database/                       # DB helpers (initializer, schema cache)
│   ├── Fixtures/                       # Shared fixtures (SimpleDatabaseFixture, etc.)
│   ├── TestModule.cs                   # TestModule flags enum
│   ├── TestDtos.cs                     # Shared DTOs (LoginResponseDto, LoginDataDto)
│   └── TestServicesConfiguration.cs    # DI service overrides for tests
├── GlobalUsings.cs                     # Global using directives
├── Modules/                            # Module-specific tests
│   ├── Bookings/                       # Api/, ModuleApi/, Flows/
│   ├── Communications/                 # ModuleApi/, Flows/
│   ├── Documents/                      # Api/, Database/
│   ├── Locations/                      # Api/, Services/, Middleware/
│   ├── Payments/                       # Api/, ModuleApi/
│   ├── Providers/                      # Api/, Database/
│   ├── Ratings/                        # Api/, ModuleApi/
│   ├── SearchProviders/                # Api/, Flows/
│   ├── ServiceCatalogs/                # Api/, Database/
│   └── Users/                          # Api/, Database/
├── Authentication/                     # Auth integration tests
├── Authorization/                      # Authorization integration tests
├── Database/                           # Cross-cutting DB tests (concurrency)
├── Diagnostics/                        # DI diagnostic tests
├── Middleware/                         # Middleware tests (security headers, compression)
└── Debug/                              # Debug/verification tests
```

## Writing Integration Tests

### Base Classes

| Base Class | Purpose | Use When |
|------------|---------|----------|
| `BaseApiTest` | Standard integration tests with DI, HTTP client, WireMock | Most integration tests |
| `BaseAspireIntegrationTest` | Aspire-based tests with `WebApplicationFactory` | Aspire-specific tests |
| `BasePerformanceTest` | Performance/stress tests | Benchmarking |
| `BaseSharedTest` | Cross-module tests | Multi-module scenarios |

### TestModule Optimization

**CRITICAL**: Always declare the minimum required modules to avoid slow startup:

```csharp
public class DocumentsIntegrationTests : BaseApiTest
{
    // Only run migrations for Documents module (~10-15s vs ~60-70s for All)
    protected override TestModule RequiredModules => TestModule.Documents;
}
```

### TestModule Flags

```csharp
[Flags]
public enum TestModule
{
    None = 0,               // No migrations (DI/config tests only)
    Users = 1 << 0,
    Providers = 1 << 1,
    Documents = 1 << 2,
    ServiceCatalogs = 1 << 3,
    Locations = 1 << 4,
    SearchProviders = 1 << 5,
    Communications = 1 << 6,
    Payments = 1 << 7,
    Bookings = 1 << 8,
    Ratings = 1 << 9,
    All = Users | Providers | Documents | ServiceCatalogs | Locations |
          SearchProviders | Communications | Payments | Bookings | Ratings
}
```

### Performance Comparison

| Scenario | All Modules | Required Only | Improvement |
|----------|-------------|---------------|-------------|
| Initialization | ~60-70s | ~10-15s | **83% faster** |
| Migrations applied | 10 modules always | Only needed | Minimal |
| Timeouts | Frequent | Rare | Stable |
| Connection pool | Exhaustion | Isolated per module | Reliable |

### Example: Standard Test

```csharp
/// <summary>
/// Integration tests for the Documents module.
/// Only applies Documents migrations for fast startup.
/// </summary>
public class DocumentsApiTests(ITestOutputHelper testOutput) : BaseApiTest(testOutput)
{
    protected override TestModule RequiredModules => TestModule.Documents;

    [Fact]
    public async Task UploadDocument_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/documents", validPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Example: Cross-Module Test

```csharp
public class CrossModuleFlowTests(ITestOutputHelper testOutput) : BaseApiTest(testOutput)
{
    // Requires multiple modules for integration testing
    protected override TestModule RequiredModules =>
        TestModule.Users | TestModule.Documents | TestModule.Providers |
        TestModule.SearchProviders;
}
```

### Test Data Helpers

`BaseApiTest` provides helper methods for creating test entities:

```csharp
// Via API (recommended for API tests)
var providerId = await CreateTestProviderViaApiAsync();
var userId = await CreateTestUserViaApiAsync();
var serviceId = await CreateTestServiceViaDbAsync(providerId);
var scheduleId = await CreateTestScheduleViaDbAsync(providerId);

// Via DB directly (for complex scenarios)
var provider = await CreateTestProviderViaDbAsync();
```

## Test Infrastructure

### Shared.Tests

The `MeAjudaAi.Shared.Tests` project provides:

- **Builders**: `UserBuilder`, `ProviderBuilder`, `ServiceCategoryBuilder`, `ServiceBuilder`, `BookingBuilder`, `DocumentBuilder`, `ReviewBuilder`
- **Mocks**: `MockKeycloakService`, `MockBlobStorageService`, `MockNoOpMessageBus`
- **Extensions**: `TestAuthorizationExtensions` (Scrutor-based auth handler registration)
- **Helpers**: `AuthConfig`, test data constants

### GlobalUsings.cs

The project includes a `GlobalUsings.cs` that provides:

```csharp
global using MeAjudaAi.Integration.Tests.Infrastructure;
```

This makes `TestModule`, `LoginResponseDto`, `LoginDataDto` and other infrastructure types available without explicit using directives.

## External API Mocking

### WireMock.Net

Tests use **WireMock.Net** to mock external HTTP APIs:

- **IBGE Localidades API**: City lookups (Muriaé, Itaperuna, Linhares)
- **ViaCep API**: CEP lookups
- **BrasilApi**: CEP with state/city/street
- **OpenCep**: Full address with IBGE code
- **Nominatim**: Geocoding (reverse geocode)

WireMock runs on `localhost:5050` and is automatically configured via `appsettings.Testing.json`.

## Running Tests

### All integration tests

```bash
dotnet test tests/MeAjudaAi.Integration.Tests
```

### Specific module

```bash
dotnet test tests/MeAjudaAi.Integration.Tests --filter "Module=Documents"
```

### Verbose logging

```bash
dotnet test tests/MeAjudaAi.Integration.Tests --logger "console;verbosity=detailed"
```

## Configuration

### appsettings.Testing.json

- Geographic restriction: Enabled by default
- External APIs: Points to WireMock at localhost:5050
- Database: PostgreSQL localhost with test credentials
- Hangfire: Disabled
- Keycloak: Disabled (mocked)

### Database Credentials

**Test-only credentials** (localhost only):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meajudaai_test;Username=testuser;Password=test123;Port=5432"
  }
}
```

- PostgreSQL + PostGIS (`postgis/postgis:16-3.4`)
- NetTopologySuite for geographic data
- Never use in production
