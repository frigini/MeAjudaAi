# Skipped Tests Analysis & Fix Plan

**Date**: 2024
**Scope**: Fix ALL 38 skipped tests + increase coverage 28.69% → 80%+
**Branch**: feature/module-integration

## Executive Summary

**Total Skipped Tests**: 38
- **AUTH (Authentication)**: 11 tests
- **IBGE (API Dependency)**: 14 tests (10 integration + 4 unavailability)
- **HANGFIRE**: 6 tests
- **GEOGRAPHIC (Middleware)**: 3 tests
- **INFRA (Infrastructure)**: 3 tests
- **DIAGNOSTIC**: 1 test (intentionally skipped)

**Current Coverage**: 28.69%
**Target Coverage**: 80%+
**Estimated New Unit Tests Needed**: ~165+

---

## 1. Authentication Infrastructure (11 tests) - PRIORITY 1

### Root Cause
`ConfigurableTestAuthenticationHandler` with `SetAllowUnauthenticated(true)` causes race condition where admin config isn't applied before authorization checks. Tests expect specific roles/permissions but get 403 Forbidden.

### Affected Tests

#### PermissionAuthorizationE2ETests.cs (5 tests)
1. **UserWithRequiredPermission_Can_Access_Protected_Endpoint**
   - Skip Reason: Returns OK instead of Forbidden in CI
   - Expected: Forbidden when permission missing
   - Actual: OK (admin access forced)
   - Line: 36

2. **UserWithoutRequiredPermission_Cannot_Access_Protected_Endpoint**
   - Skip Reason: SetAllowUnauthenticated causes inconsistent auth
   - Expected: 201/BadRequest
   - Actual: 403 Forbidden
   - Line: 57

3. **UserWithoutRequiredPermission_Cannot_Create_User**
   - Skip Reason: SetAllowUnauthenticated forces Admin access
   - Expected: Forbidden
   - Actual: BadRequest (validation)
   - Line: 88

4. **UserWithMultiplePermissions_Can_Access_Multiple_Protected_Endpoints**
   - Skip Reason: SetAllowUnauthenticated forces all requests to Admin
   - Expected: Permission-specific behavior
   - Actual: All requests treated as Admin
   - Line: 117

5. **UserWithRequiredPermission_Cannot_Access_Endpoint_Without_Different_Permission**
   - Skip Reason: Returns OK instead of Forbidden
   - Expected: Forbidden for different permission
   - Actual: OK
   - Line: 189

#### ServiceCatalogsEndToEndTests.cs (1 test)
6. **Update_Service_Returns_NoContent**
   - Skip Reason: Returns 403 instead of 204
   - Expected: 204 NoContent
   - Actual: 403 Forbidden
   - Line: 138

#### ServiceCatalogsAdvancedE2ETests.cs (2 tests)
7. **Validation_Rules_Properly_Enforced_For_Service_Catalog_Operations**
   - Skip Reason: Returns 403 instead of 200/204
   - Expected: 200/204
   - Actual: 403 Forbidden
   - GitHub Issue: #<TBD>
   - Line: 21

8. **Validation_Rules_Properly_Enforced_For_Provider_In_Service_Operations**
   - Skip Reason: Returns 403 instead of 400/404/200
   - Expected: Various status codes based on validation
   - Actual: 403 Forbidden
   - GitHub Issue: #<TBD>
   - Line: 82

#### ModuleIntegrationTests.cs (1 test)
9. **ServicesModule_Can_Validate_Services_From_Catalogs**
   - Skip Reason: SetAllowUnauthenticated causes 403 instead of 201/409
   - Expected: 201 Created or 409 Conflict
   - Actual: 403 Forbidden
   - GitHub Issue: #<TBD>
   - Line: 12

#### ApiVersioningTests.cs (1 test)
10. **Legacy_Clients_Receive_Proper_Error_Messages**
    - Skip Reason: SetAllowUnauthenticated causes inconsistent auth
    - Expected: OK/401/400
    - Actual: 403 Forbidden
    - Line: 44

#### UsersModuleTests.cs (1 test)
11. **ProvidersModule_Can_Query_Active_Services_Only**
    - Skip Reason: Returns 403 instead of 400
    - Expected: 400 BadRequest
    - Actual: 403 Forbidden
    - GitHub Issue: #<TBD>
    - Line: 72

### Fix Strategy

**Option 1: Remove SetAllowUnauthenticated** (RECOMMENDED)
- Replace with proper `AuthenticateAsAdmin()`, `AuthenticateAsUser(permissions)`, `AuthenticateAsAnonymous()` calls
- Add test helpers for role/permission setup
- Ensure consistent authentication state throughout test execution

**Option 2: Fix Race Condition**
- Ensure `SetAllowUnauthenticated(true)` is called BEFORE `AuthenticateAsAdmin()`
- Add synchronization mechanism to guarantee config order
- Less robust, may still fail in CI

**Implementation Steps**:
1. Create `TestAuthenticationBuilder` with fluent API for role/permission setup
2. Refactor `ConfigurableTestAuthenticationHandler` to use deterministic state (no race conditions)
3. Update all 11 tests to use new authentication setup
4. Run locally to verify 403 errors resolved
5. Push and verify CI passes

**Estimated Effort**: 3-4 hours

---

## 2. IBGE API Dependency (14 tests) - PRIORITY 2

### Root Cause
Tests call real IBGE API (https://servicodados.ibge.gov.br/api/v1/localidades) instead of using mocks/stubs.

### Affected Tests

#### IbgeApiIntegrationTests.cs (10 tests - Real API)
1. **Search_Cities_By_Name_Returns_Valid_Results** (Line 39)
2. **Search_Cities_By_State_Returns_Valid_Results** (Line 65)
3. **Get_City_By_Id_Returns_Valid_Result** (Line 86)
4. **Search_States_Returns_Valid_Results** (Line 107)
5. **Get_State_By_Id_Returns_Valid_Result** (Line 120)
6. **Get_State_By_UF_Returns_Valid_Result** (Line 141)
7. **Search_Cities_With_Invalid_Parameters_Returns_Empty** (Line 162)
8. **Get_City_With_Invalid_Id_Returns_NotFound** (Line 183)
9. **Get_State_With_Invalid_Id_Returns_NotFound** (Line 197)
10. **Search_Cities_Handles_Special_Characters** (Line 207)

#### IbgeUnavailabilityTests.cs (4 tests - Middleware Fallback)
11. **Provider_In_Allowed_City_Returns_Success_When_Ibge_Unavailable** (Line 23)
    - Skip Reason: Middleware doesn't fall back to simple validation
    - Expected: 200 OK (allowed city passes)
    - Actual: Blocks request

12. **Provider_In_Allowed_State_Returns_Success_When_Ibge_Unavailable** (Line 47)
    - Skip Reason: Middleware doesn't fall back to simple validation
    - Expected: 200 OK (allowed state passes)
    - Actual: Blocks request

13. **Provider_In_Disallowed_State_Returns_Restricted_When_Ibge_Unavailable** (Line 71)
    - Skip Reason: CI returns 200 OK instead of 451
    - Expected: 451 Unavailable For Legal Reasons
    - Actual: 200 OK

14. **Provider_Update_In_Allowed_City_Returns_Success_When_Ibge_Unavailable** (Line 109)
    - Skip Reason: Middleware doesn't fall back to simple validation
    - Expected: 200 OK (allowed city passes)
    - Actual: Blocks request

### Fix Strategy

**For IbgeApiIntegrationTests (10 tests)**:
1. Add IBGE stubs to WireMockFixture
2. Create stub responses for:
   - City search by name (São Paulo, Rio de Janeiro)
   - City search by state (SP, RJ)
   - City by ID
   - State list
   - State by ID/UF
   - Invalid parameters (empty results)
   - Invalid IDs (404 responses)
   - Special characters handling
3. Reconfigure IbgeClient HttpClient in ApiTestBase (similar to CEP providers)
4. Remove Skip attributes

**For IbgeUnavailabilityTests (4 tests)**:
1. Fix GeographicRestrictionMiddleware to implement proper fallback logic
2. When IBGE unavailable:
   - Allowed cities/states → return 200 OK
   - Disallowed states → return 451 Unavailable
3. Add circuit breaker pattern for IBGE failures
4. Test all scenarios with WireMock fault simulation

**Implementation Steps**:
1. Create `WireMockFixture.SetupIbgeStubs()` method
2. Add 20+ IBGE stub responses (cities, states, errors)
3. Reconfigure IbgeClient in ApiTestBase
4. Implement middleware fallback logic in GeographicRestrictionMiddleware
5. Run locally to verify all 14 tests pass
6. Push and verify CI passes

**Estimated Effort**: 4-5 hours

---

## 3. Hangfire Integration (6 tests) - PRIORITY 3

### Root Cause
Tests require Aspire DCP/Dashboard which isn't available in CI/CD environment.

### Affected Tests

#### HangfireIntegrationTests.cs (6 tests)
1. **Hangfire_Background_Job_Is_Created_Successfully** (Line 108)
2. **Hangfire_Recurring_Job_Is_Created_Successfully** (Line 143)
3. **Hangfire_Job_Can_Be_Deleted** (Line 193)
4. **Hangfire_Dashboard_Is_Accessible** (Line 239)
5. **Hangfire_Jobs_Are_Persisted_In_PostgreSQL** (Line 283)
6. **Hangfire_Job_Execution_Completes_Successfully** (Line 326)

### Fix Strategy

**Option 1: Mock Hangfire Dashboard** (RECOMMENDED for CI)
- Use in-memory Hangfire storage for integration tests
- Mock dashboard responses
- Focus on job creation/deletion/execution, not dashboard UI

**Option 2: TestContainers for Hangfire**
- Spin up Hangfire container with PostgreSQL
- Slower tests, but more realistic

**Option 3: Skip Dashboard-Specific Tests**
- Only run locally (not recommended for coverage goal)

**Implementation Steps**:
1. Add Hangfire.InMemory NuGet package
2. Configure integration tests to use InMemoryStorage instead of PostgreSQL
3. Remove dashboard accessibility requirement
4. Test job creation, scheduling, deletion, execution
5. Run locally to verify
6. Push and verify CI passes

**Estimated Effort**: 2-3 hours

---

## 4. Geographic Restriction Middleware (3 tests) - PRIORITY 4

### Root Cause
CI returns 200 OK instead of 451 Unavailable. Likely feature flag or middleware registration issue in CI environment.

### Affected Tests

#### GeographicRestrictionFeatureFlagTests.cs (3 tests)
1. **Feature_Flag_Disabled_Allows_All_Requests** (Line 26)
   - Skip Reason: CI returns 200 OK instead of 451
   - Expected: 451 when flag disabled
   - Actual: 200 OK

2. **Feature_Flag_Enabled_Enforces_Geographic_Restrictions** (Line 50)
   - Skip Reason: CI returns 200 OK instead of expected behavior
   - Expected: Restrictions enforced
   - Actual: 200 OK

3. **Feature_Flag_Toggle_Changes_Middleware_Behavior** (Line 76)
   - Skip Reason: CI returns 200 OK instead of 451
   - Expected: Middleware behavior changes with flag
   - Actual: 200 OK

### Fix Strategy

**Diagnostic Steps**:
1. Add logging to GeographicRestrictionMiddleware to verify registration
2. Check feature flag configuration in CI environment
3. Verify middleware order in pipeline (must be after auth, before MVC)

**Potential Issues**:
- Feature flag not loaded in CI
- Middleware not registered in test environment
- Middleware skipped due to config issue

**Implementation Steps**:
1. Add diagnostic logging to middleware
2. Verify `GeographicRestriction:Enabled` configuration in CI
3. Check middleware registration in ApiTestBase
4. Add explicit middleware registration in test setup
5. Run locally, then push and verify CI

**Estimated Effort**: 1-2 hours

---

## 5. Infrastructure-Dependent Tests (3 tests) - PRIORITY 5

### DocumentsVerificationE2ETests.cs (1 test)
**Test**: Upload_Valid_Document_Returns_Success
- **Skip Reason**: Azurite container not accessible from app container in CI (localhost mismatch)
- **Expected**: 200 OK with document ID
- **Actual**: Connection failure to Azurite
- **Fix**: Configure proper Docker networking or use TestContainers.Azurite
- **Line**: 16

### CrossModuleCommunicationE2ETests.cs (1 test)
**Test**: Provider_Service_Integration_Returns_Valid_Results
- **Skip Reason**: Race condition or test isolation issue in CI. Users created in Arrange not found in Act
- **Expected**: Users available for provider operations
- **Actual**: Users not found
- **Fix**: Investigate TestContainers database persistence in GitHub Actions
- **Line**: 55

### CepProvidersUnavailabilityTests.cs (1 test)
**Test**: Successful_Response_Is_Cached**
- **Skip Reason**: Caching disabled in integration tests (Caching:Enabled = false)
- **Expected**: Cached response on second call
- **Actual**: Caching infrastructure not available
- **Fix**: Enable Redis/caching in integration test environment
- **Line**: 264

### Implementation Steps
1. **Azurite**: Add TestContainers.Azurite, configure network aliases
2. **Database Race**: Add explicit wait/retry for seeded data in Act phase
3. **Caching**: Enable Redis in integration tests or mock IDistributedCache
4. Run locally, then verify CI

**Estimated Effort**: 2-3 hours

---

## 6. Diagnostic Test (1 test) - SKIP

### ServiceCatalogsResponseDebugTest.cs
**Test**: Debug_Service_Catalog_Response_Format
- **Skip Reason**: Diagnostic test - enable only when debugging response format issues
- **Action**: Leave skipped (intentional)
- **Line**: 12

---

## Coverage Improvement Plan

### Current State
- **Coverage**: 28.69%
- **Target**: 80%+
- **Gap**: 51.31%

### High-Priority Coverage Areas

#### Domain Layer (Business Logic)
- **Entities**: User, Provider, Service, Catalog, Location
- **Value Objects**: CEP, CNPJ, Email, Address
- **Domain Events**: UserCreated, ProviderRegistered, ServicePublished
- **Domain Services**: AddressValidator, DocumentValidator

#### Application Layer (Handlers)
- **Commands**: CreateUser, RegisterProvider, PublishService
- **Queries**: GetProviderById, SearchServices
- **Validation**: FluentValidation rules
- **Authorization**: Permission checks

#### Infrastructure Layer
- **Repositories**: Error handling, transaction boundaries
- **External Services**: CEP providers, IBGE client
- **Middleware**: GeographicRestriction, Error handling

### Unit Test Strategy

**Estimated Tests Needed**: 165+

**Breakdown**:
- Domain Entities: ~40 tests (validation, state changes, business rules)
- Value Objects: ~30 tests (creation, validation, equality)
- Domain Events: ~20 tests (event data, serialization)
- Command Handlers: ~35 tests (success, validation, errors)
- Query Handlers: ~25 tests (filters, sorting, pagination)
- Repositories: ~15 tests (LINQ expressions, error handling)

**Coverage Targets**:
- Domain: 95%+
- Application: 85%+
- Infrastructure: 70%+
- Overall: 80%+

---

## Implementation Order

### Phase 1: Authentication (CRITICAL PATH)
1. Fix ConfigurableTestAuthenticationHandler (11 tests)
2. Verify all E2E tests pass
3. Commit: "fix: resolve ConfigurableTestAuthenticationHandler race condition"

### Phase 2: External Dependencies
4. IBGE WireMock stubs + middleware fallback (14 tests)
5. Hangfire in-memory storage (6 tests)
6. Commit: "fix: mock IBGE API and Hangfire dependencies"

### Phase 3: Infrastructure
7. Geographic Restriction middleware registration (3 tests)
8. Azurite/Database/Caching infrastructure (3 tests)
9. Commit: "fix: resolve infrastructure-dependent test failures"

### Phase 4: Coverage
10. Generate coverage report
11. Write domain unit tests (40 tests)
12. Write application unit tests (60 tests)
13. Write infrastructure unit tests (15 tests)
14. Commit: "test: add comprehensive unit tests for 80% coverage"

### Phase 5: Validation
15. Run full test suite locally (100% pass)
16. Push and verify CI (100% pass + 80% coverage)
17. Create summary report

---

## Success Criteria

- [ ] **E2E Tests**: 100/100 passing (86 → 100, fix 14 AUTH tests)
- [ ] **Integration Tests**: All passing (fix 20 skipped)
- [ ] **Coverage**: ≥ 80% (from 28.69%)
- [ ] **CI/CD**: Green pipeline
- [ ] **Documentation**: All fixes documented with rationale

## Estimated Total Effort

- **Test Fixes**: 12-17 hours
- **Unit Test Writing**: 15-20 hours
- **CI Validation**: 2-3 hours
- **Total**: 29-40 hours (4-5 days)

---

**Next Steps**: Start with Phase 1 - Authentication Infrastructure
