# Coverage Improvement Plan - 17.4% → 70%

**Date:** 2025-11-28  
**Current Coverage:** 17.4% (6556/37550 lines)  
**Target Coverage:** 70%  
**Gap:** +52.6pp (19,729 lines needed)

---

## 📊 Executive Summary

| Metric | Current | Target | Gap |
|--------|---------|--------|-----|
| **Line Coverage** | 17.4% | 70% | +52.6pp |
| **Branch Coverage** | 12.2% | 60% | +47.8pp |
| **Method Coverage** | 35.2% | 70% | +34.8pp |
| **Lines to Cover** | 6,556 | 26,285 | +19,729 |

### Critical Issue
**Docker Desktop is blocking 68 integration/E2E tests locally**, causing 14pp coverage gap between local (17.4%) and pipeline (35.11%).

---

## 🎯 Module-by-Module Analysis

### Priority 1: **High-Value, Low Coverage** (Will add ~35pp)

#### 1. **Providers Module** - Infrastructure 1.4% → Target 70%
**Impact:** ~8-10pp  
**Current:** 35/2382 lines (1.4%)  
**Files with 0% coverage:**
- `ProviderRepository.cs` (0/106 lines)
- `ProviderActivatedDomainEventHandler.cs` (0/15 lines)
- `ProviderAwaitingVerificationDomainEventHandler.cs` (0/15 lines)
- `ProviderDeletedDomainEventHandler.cs` (0/28 lines)
- `ProviderProfileUpdatedDomainEventHandler.cs` (0/25 lines)
- `ProviderRegisteredDomainEventHandler.cs` (0/23 lines)
- `ProviderVerificationStatusUpdatedDomainEventHandler.cs` (0/50 lines)

**Actions:**
```powershell
# Create test files
New-Item -Path "src/Modules/Providers/Tests/Infrastructure/Repositories" -Type Directory -Force
New-Item -Path "src/Modules/Providers/Tests/Infrastructure/Events" -Type Directory -Force

# Files to create:
- ProviderRepositoryTests.cs (+106 lines)
- ProviderDomainEventHandlersTests.cs (+156 lines)
```

#### 2. **Documents Module** - Infrastructure 6.6% → Target 70%
**Impact:** ~6-8pp  
**Current:** 71/1071 lines (6.6%)  
**Files with 0% coverage:**
- `DocumentRepository.cs` (0/27 lines)
- `DocumentVerifiedDomainEventHandler.cs` (0/30 lines)
- `DocumentVerificationJob.cs` (0/99 lines)
- `DocumentsDbContext.cs` (0/30 lines)

**Actions:**
```powershell
# Files to create:
- DocumentRepositoryTests.cs (+27 lines)
- DocumentVerificationJobTests.cs (+99 lines)
- DocumentDomainEventHandlersTests.cs (+30 lines)
```

#### 3. **ServiceCatalogs Module** - Infrastructure 0% → Target 70%
**Impact:** ~6-8pp  
**Current:** 0/1011 lines (0%)  
**All files have 0% coverage:**
- `ServiceCategoryRepository.cs` (0/45 lines)
- `ServiceRepository.cs` (0/80 lines)

**Actions:**
```powershell
# Create test directory
New-Item -Path "src/Modules/ServiceCatalogs/Tests/Infrastructure" -Type Directory -Force

# Files to create:
- ServiceCategoryRepositoryTests.cs (+45 lines)
- ServiceRepositoryTests.cs (+80 lines)
```

#### 4. **SearchProviders Module** - Infrastructure 3% → Target 70%
**Impact:** ~5-7pp  
**Current:** 31/1008 lines (3%)  
**Files with 0% coverage:**
- `SearchableProviderRepository.cs` (0/154 lines)

**Actions:**
```powershell
# File to create:
- SearchableProviderRepositoryTests.cs (+154 lines)
```

#### 5. **Providers Module** - Application Queries 0% → Target 90%
**Impact:** ~2-3pp  
**Current:** 0/32 lines (0%)  
**File with 0% coverage:**
- `GetProvidersQueryHandler.cs` (0/32 lines)

**Actions:**
```powershell
# File to create:
- GetProvidersQueryHandlerTests.cs (+40 lines)
```

#### 6. **Users Module** - Application Queries 0% → Target 90%
**Impact:** ~2-3pp  
**Current:** 0/36 lines (0%)  
**File with 0% coverage:**
- `GetUsersByIdsQueryHandler.cs` (0/36 lines)

**Actions:**
```powershell
# File to create:
- GetUsersByIdsQueryHandlerTests.cs (+50 lines)
```

---

### Priority 2: **API Layer** (Will add ~8-10pp)

All API modules have **0% coverage** (OpenAPI generated code excluded):

#### 1. **Documents.API** - 0% → Target 60%
**Impact:** ~1-2pp  
**Files:** 5 endpoints (160 lines)

#### 2. **Providers.API** - 0% → Target 60%
**Impact:** ~2-3pp  
**Files:** 14 endpoints (413 lines)

#### 3. **ServiceCatalogs.API** - 0% → Target 60%
**Impact:** ~1-2pp  
**Files:** 20 endpoints (309 lines)

#### 4. **SearchProviders.API** - 0% → Target 60%
**Impact:** ~0.5pp  
**Files:** 1 endpoint (46 lines)

**Strategy:** Create E2E tests for API endpoints (blocked by Docker Desktop - priority after Docker fix)

---

### Priority 3: **ApiService** (Will add ~3-5pp)

**Current:** 11.1% (229/2052 lines)  
**High-value targets:**
- `SecurityExtensions.cs` (0/305 lines)
- `PerformanceExtensions.cs` (0/131 lines)
- `MiddlewareExtensions.cs` (0/8 lines)
- `RequestLoggingMiddleware.cs` (0/101 lines)
- `RateLimitingMiddleware.cs` (0/121 lines)

**Note:** Low priority - mostly infrastructure setup code

---

## 📋 Test Implementation Checklist

### Phase 1: Repository Tests (Week 1-2) - **Expected +10-12pp**
- [ ] ProviderRepositoryTests.cs
- [ ] DocumentRepositoryTests.cs
- [ ] ServiceCategoryRepositoryTests.cs
- [ ] ServiceRepositoryTests.cs
- [ ] SearchableProviderRepositoryTests.cs

### Phase 2: Domain Event Handlers (Week 2-3) - **Expected +8-10pp**
- [ ] ProviderDomainEventHandlersTests.cs
- [ ] DocumentDomainEventHandlersTests.cs

### Phase 3: Application Handlers (Week 3-4) - **Expected +4-6pp**
- [ ] GetProvidersQueryHandlerTests.cs
- [ ] GetUsersByIdsQueryHandlerTests.cs
- [ ] DocumentVerificationJobTests.cs

### Phase 4: Missing Domain Layer Tests (Week 4-5) - **Expected +8-10pp**
- [ ] Providers.Domain.ValueObjects.Address tests (30% → 100%)
- [ ] ServiceCatalogs.Domain entities (27% → 90%)
- [ ] SearchProviders.Domain entities (17% → 90%)

### Phase 5: API E2E Tests (Week 5-6) - **Expected +10-12pp**
**⚠️ Blocked by Docker Desktop issue**
- [ ] Documents API endpoints (5 endpoints)
- [ ] Providers API endpoints (14 endpoints)
- [ ] ServiceCatalogs API endpoints (20 endpoints)
- [ ] SearchProviders API endpoint (1 endpoint)

### Phase 6: Infrastructure Services (Week 6-7) - **Expected +6-8pp**
- [ ] Locations.Infrastructure external clients (0% → 70%)
- [ ] Users.Infrastructure identity services (23% → 70%)

---

## 🚀 Quick Wins (Can implement immediately)

### 1. Missing Handler Tests (~4pp in 1-2 days)
```bash
# Create these files today:
src/Modules/Providers/Tests/Application/Handlers/GetProvidersQueryHandlerTests.cs
src/Modules/Users/Tests/Application/Handlers/GetUsersByIdsQueryHandlerTests.cs
```

### 2. Missing Repository Tests (~10pp in 3-5 days)
```bash
# Create these files this week:
src/Modules/Providers/Tests/Infrastructure/ProviderRepositoryTests.cs
src/Modules/Documents/Tests/Infrastructure/DocumentRepositoryTests.cs
src/Modules/ServiceCatalogs/Tests/Infrastructure/ServiceCategoryRepositoryTests.cs
src/Modules/ServiceCatalogs/Tests/Infrastructure/ServiceRepositoryTests.cs
src/Modules/SearchProviders/Tests/Infrastructure/SearchableProviderRepositoryTests.cs
```

### 3. Missing Value Object Tests (~3pp in 1 day)
```bash
# Create these files:
src/Modules/Providers/Tests/Domain/ValueObjects/AddressTests.cs
```

**Total Quick Wins: ~17pp in 1 week**

---

## 🔍 Detailed Gap Analysis

### By Module (Coverage %)

| Module | Current | Target | Gap | Priority |
|--------|---------|--------|-----|----------|
| **Providers.Infrastructure** | 1.4% | 70% | +68.6pp | 🔴 CRITICAL |
| **ServiceCatalogs.Infrastructure** | 0% | 70% | +70pp | 🔴 CRITICAL |
| **SearchProviders.Infrastructure** | 3% | 70% | +67pp | 🔴 CRITICAL |
| **Documents.Infrastructure** | 6.6% | 70% | +63.4pp | 🔴 HIGH |
| **Users.Infrastructure** | 23.4% | 70% | +46.6pp | 🟡 MEDIUM |
| **Locations.Infrastructure** | 15.9% | 70% | +54.1pp | 🟡 MEDIUM |
| **ApiService** | 11.1% | 60% | +48.9pp | 🟢 LOW |
| **All APIs** | 0% | 60% | +60pp | 🟡 MEDIUM* |

*Blocked by Docker Desktop

### By Layer (Lines to Cover)

| Layer | Current Lines | Target Lines | Lines Needed | Estimated Effort |
|-------|---------------|--------------|--------------|------------------|
| **Infrastructure** | ~500 | ~6,000 | ~5,500 | 3-4 weeks |
| **Application** | ~2,200 | ~4,500 | ~2,300 | 1-2 weeks |
| **Domain** | ~1,900 | ~3,500 | ~1,600 | 1-2 weeks |
| **API** | ~200 | ~2,500 | ~2,300 | 2-3 weeks |
| **ApiService** | ~230 | ~1,200 | ~970 | 1 week |
| **Total** | **6,556** | **26,285** | **19,729** | **8-12 weeks** |

---

## 🛠️ Test Patterns to Use

### Repository Tests Pattern
```csharp
public class ProviderRepositoryTests : DatabaseTestBase
{
    private readonly ProviderRepository _repository;

    [Fact]
    public async Task AddAsync_ValidProvider_ShouldPersist()
    {
        // Arrange
        var provider = Provider.Create(...);
        
        // Act
        await _repository.AddAsync(provider);
        await Context.SaveChangesAsync();
        
        // Assert
        var result = await _repository.GetByIdAsync(provider.Id);
        result.Should().NotBeNull();
    }
}
```

### Handler Tests Pattern
```csharp
public class GetProvidersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnProviders()
    {
        // Arrange
        var mockRepository = Substitute.For<IProviderRepository>();
        var handler = new GetProvidersQueryHandler(mockRepository);
        var query = new GetProvidersQuery { PageSize = 10 };
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }
}
```

### Domain Event Handler Pattern
```csharp
public class ProviderActivatedDomainEventHandlerTests
{
    [Fact]
    public async Task Handle_ProviderActivated_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var mockEventBus = Substitute.For<IEventBus>();
        var handler = new ProviderActivatedDomainEventHandler(mockEventBus);
        var @event = new ProviderActivatedDomainEvent(ProviderId.Create());
        
        // Act
        await handler.Handle(@event, CancellationToken.None);
        
        // Assert
        await mockEventBus.Received(1).PublishAsync(Arg.Any<ProviderActivatedIntegrationEvent>());
    }
}
```

---

## 📈 Estimated Timeline

### Conservative Estimate (12 weeks)
- **Week 1-2:** Repository tests (+10pp) → **27%**
- **Week 3-4:** Domain event handlers (+8pp) → **35%**
- **Week 5-6:** Application handlers (+6pp) → **41%**
- **Week 7-8:** Domain value objects (+8pp) → **49%**
- **Week 9-10:** Infrastructure services (+6pp) → **55%**
- **Week 11:** API E2E tests (+10pp) → **65%**
- **Week 12:** Refinement and edge cases (+5pp) → **70%** ✅

### Aggressive Estimate (6 weeks)
- **Week 1:** Repository tests (+10pp) → **27%**
- **Week 2:** Domain event handlers (+8pp) → **35%**
- **Week 3:** Application handlers + Value objects (+14pp) → **49%**
- **Week 4:** Infrastructure services (+6pp) → **55%**
- **Week 5:** API E2E tests (+10pp) → **65%**
- **Week 6:** Refinement (+5pp) → **70%** ✅

---

## 🚧 Blockers

### 1. Docker Desktop (CRITICAL)
**Impact:** Blocks 68 integration/E2E tests (~14pp coverage)  
**Status:** Known issue, works in CI/CD  
**Workaround:** Focus on unit tests first, fix Docker later

### 2. Missing Test Infrastructure
**Impact:** Need to create test base classes for new modules  
**Status:** Can copy patterns from Users module  
**Effort:** ~1 day per module

---

## 🎯 Success Metrics

### Coverage Targets by Layer
- **Domain:** 80%+ (business logic critical)
- **Application:** 70%+ (handlers and services)
- **Infrastructure:** 60%+ (repositories and integrations)
- **API:** 50%+ (E2E tests)

### Quality Gates
- No untested public methods in Domain layer
- All CQRS handlers must have at least 1 test
- All repositories must have CRUD tests
- All domain events must have handler tests

---

## 💡 Next Steps

### Immediate Actions (Today)
1. ✅ Generate coverage report (DONE)
2. ⏭️ Create `GetProvidersQueryHandlerTests.cs`
3. ⏭️ Create `GetUsersByIdsQueryHandlerTests.cs`

### This Week
1. Create 5 repository test files
2. Create domain event handler tests for Providers
3. Create Address value object tests

### This Sprint (2 weeks)
1. Complete all Priority 1 tests
2. Increase coverage to 35%+
3. Document test patterns in `test_infrastructure.md`

---

## 📚 Resources

### Test Examples
- **Best coverage:** `Providers.Application` (65.3%)
- **Good patterns:** `ServiceCatalogs.Application` (35.6%)
- **Reference:** `Users.Application` (53.4%)

### Tools
- **Coverage tool:** XPlat Code Coverage
- **Report generator:** ReportGenerator
- **Test framework:** xUnit
- **Mocking:** NSubstitute
- **Assertions:** FluentAssertions

### Documentation
- [Test Infrastructure Guide](./test_infrastructure.md)
- [Code Coverage Guide](./code_coverage_guide.md)
- [E2E Architecture Analysis](./e2e-architecture-analysis.md)

---

**Last Updated:** 2025-11-28 15:06 UTC  
**Author:** GitHub Copilot  
**Version:** 1.0
