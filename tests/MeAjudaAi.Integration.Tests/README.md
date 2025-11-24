# Integration Tests

## Overview

This directory contains integration tests for the MeAjudaAi API. These tests verify that different components work together correctly in a controlled test environment.

## Test Configuration Files

### `appsettings.Testing.json`

Default test configuration with **GeographicRestriction enabled**.

- **Purpose**: Test geographic restriction middleware with real API endpoints
- **Geographic Restriction**: ENABLED (`FeatureManagement.GeographicRestriction: true`)
- **External APIs**: Points to real external services (ViaCep, IBGE, etc.)
- **Use Case**: Validate geographic restriction logic with live API calls

### `appsettings.Testing.Disabled.json`

Test configuration with **GeographicRestriction disabled**.

- **Purpose**: Test application behavior when geographic restriction is turned off
- **Geographic Restriction**: DISABLED (`FeatureManagement.GeographicRestriction: false`)
- **External APIs**: Points to mock/localhost endpoints (to be implemented)
- **Use Case**: Validate fail-open behavior and graceful degradation

## Database Credentials

⚠️ **IMPORTANT: Test-Only Credentials**

The connection strings in these files use **test-only, localhost credentials**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meajudaai_test;Username=testuser;Password=test123;Port=5432"
  }
}
```

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

### Current State (TODO)

Currently, `appsettings.Testing.json` points to **real external services**:

```json
"ViaCep": { "BaseUrl": "https://viacep.com.br" },
"IBGE": { "BaseUrl": "https://servicodados.ibge.gov.br/api/v1/localidades/" }
```

This causes integration tests to make actual network calls, which is not ideal for CI/CD.

### Future Implementation (Recommended)

Tests should use mocked endpoints to avoid external dependencies:

```json
"ViaCep": { "BaseUrl": "http://localhost:5050/viacep" },
"IBGE": { "BaseUrl": "http://localhost:5050/ibge/" }
```

**Implementation Options:**

1. **WireMock.Net** - Start a mock HTTP server in test setup
2. **HttpMessageHandler mocking** - Inject test doubles for HttpClient
3. **Test containers** - Spin up containerized mock services

## Running Tests

### Run all integration tests

```bash
dotnet test tests/MeAjudaAi.Integration.Tests
```

### Run tests with GeographicRestriction enabled

```bash
ASPNETCORE_ENVIRONMENT=Testing dotnet test tests/MeAjudaAi.Integration.Tests
```

### Run tests with GeographicRestriction disabled

```bash
ASPNETCORE_ENVIRONMENT=Testing.Disabled dotnet test tests/MeAjudaAi.Integration.Tests
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
