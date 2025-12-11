# Phase 2 Coverage Plan - Gap Analysis

**Current Baseline**: 39% line coverage (14,303/36,597 lines)  
**Target**: 70% minimum (CI warning threshold)  
**Gap**: +31 percentage points (~11,300 lines)

---

## 📊 Priority Matrix

### 🔴 CRITICAL - 0% Coverage (High Impact)

| Component | Lines | Priority | Estimated Tests | Impact |
|-----------|-------|----------|-----------------|--------|
| **AppHost** (all files) | ~500 | P0 | 15-20 | +1.4% |
| NoOpClaimsTransformation | ~20 | P1 | 2-3 | +0.05% |
| SecurityOptions | ~30 | P1 | 3-5 | +0.08% |
| ApiVersionOperationFilter | ~40 | P1 | 4-6 | +0.11% |
| ExampleSchemaFilter | ~50 | P1 | 5-7 | +0.14% |
| EndpointLimits | ~15 | P2 | 2-3 | +0.04% |
| RoleLimits | ~15 | P2 | 2-3 | +0.04% |

**Subtotal**: ~670 lines | 33-47 tests | **+1.85%**

---

### 🟡 HIGH PRIORITY - Low Coverage (<40%)

| Component | Current | Lines Uncovered | Priority | Estimated Tests | Impact |
|-----------|---------|-----------------|----------|-----------------|--------|
| EnvironmentSpecificExtensions | 32.2% | ~60 | P1 | 8-10 | +0.16% |
| StaticFilesMiddleware | 25% | ~45 | P1 | 6-8 | +0.12% |
| Documents.API.Extensions | 43.5% | ~130 | P1 | 10-15 | +0.36% |

**Subtotal**: ~235 lines | 24-33 tests | **+0.64%**

---

### 🟢 MEDIUM PRIORITY - Moderate Coverage (40-70%)

| Component | Current | Lines Uncovered | Priority | Estimated Tests | Impact |
|-----------|---------|-----------------|----------|-----------------|--------|
| SecurityExtensions | 59% | ~80 | P2 | 10-12 | +0.22% |
| Program (ApiService) | 59% | ~70 | P2 | 8-10 | +0.19% |
| RateLimitingMiddleware | 66.1% | ~50 | P2 | 6-8 | +0.14% |
| CorsOptions | 70.8% | ~25 | P3 | 3-5 | +0.07% |

**Subtotal**: ~225 lines | 27-35 tests | **+0.62%**

---

## 🎯 Implementation Plan (Sprints)

### Sprint 1 - Critical Infrastructure (Week 1)
**Goal**: +2.5% coverage (39% → 41.5%)

#### Task 1.1: AppHost Testing ⏱️ 4-6 hours
- [ ] Test Keycloak configuration extensions
- [ ] Test PostgreSQL extensions  
- [ ] Test environment helpers
- [ ] Test Program startup/DI configuration
- **Lines**: ~500 | **Tests**: 15-20 | **Impact**: +1.4%

#### Task 1.2: Swagger/OpenAPI Filters ⏱️ 2-3 hours
- [ ] Test ApiVersionOperationFilter
- [ ] Test ExampleSchemaFilter
- [ ] Test ModuleTagsDocumentFilter edge cases
- **Lines**: ~90 | **Tests**: 9-13 | **Impact**: +0.25%

#### Task 1.3: Options/Configuration ⏱️ 1-2 hours
- [ ] Test SecurityOptions
- [ ] Test EndpointLimits
- [ ] Test RoleLimits
- [ ] Test NoOpClaimsTransformation
- **Lines**: ~80 | **Tests**: 9-14 | **Impact**: +0.22%

**Sprint 1 Total**: ~670 lines | 33-47 tests | **+1.87%**

---

### Sprint 2 - High Priority Gaps (Week 2)
**Goal**: +1.5% coverage (41.5% → 43%)

#### Task 2.1: Environment & Configuration ⏱️ 2-3 hours
- [ ] Test EnvironmentSpecificExtensions (all branches)
- [ ] Test configuration loading/validation
- [ ] Test environment detection logic
- **Lines**: ~60 | **Tests**: 8-10 | **Impact**: +0.16%

#### Task 2.2: Static Files & Middleware ⏱️ 2-3 hours
- [ ] Test StaticFilesMiddleware
- [ ] Test file serving scenarios
- [ ] Test MIME type detection
- [ ] Test security validations
- **Lines**: ~45 | **Tests**: 6-8 | **Impact**: +0.12%

#### Task 2.3: Documents.API Extensions ⏱️ 3-4 hours
- [ ] Test document validation extensions
- [ ] Test file type handling
- [ ] Test error scenarios
- **Lines**: ~130 | **Tests**: 10-15 | **Impact**: +0.36%

**Sprint 2 Total**: ~235 lines | 24-33 tests | **+0.64%**

---

### Sprint 3 - Security & Performance (Week 3)
**Goal**: +1% coverage (43% → 44%)

#### Task 3.1: Security Components ⏱️ 3-4 hours
- [ ] Test SecurityExtensions missing branches
- [ ] Test authentication/authorization edge cases
- [ ] Test CORS configuration scenarios
- **Lines**: ~105 | **Tests**: 13-17 | **Impact**: +0.29%

#### Task 3.2: Middleware Components ⏱️ 2-3 hours
- [ ] Test RateLimitingMiddleware edge cases
- [ ] Test rate limit bypass scenarios
- [ ] Test burst handling
- **Lines**: ~50 | **Tests**: 6-8 | **Impact**: +0.14%

#### Task 3.3: Program/Startup ⏱️ 2-3 hours
- [ ] Test Program startup paths
- [ ] Test DI container configuration
- [ ] Test middleware pipeline
- **Lines**: ~70 | **Tests**: 8-10 | **Impact**: +0.19%

**Sprint 3 Total**: ~225 lines | 27-35 tests | **+0.62%**

---

## 📈 Expected Progress

| Milestone | Coverage | Tests Added | Cumulative Lines |
|-----------|----------|-------------|------------------|
| **Baseline** | 39.0% | 1,407 | 14,303 |
| After Sprint 1 | 40.9% | +40 (1,447) | 14,973 (+670) |
| After Sprint 2 | 41.5% | +28 (1,475) | 15,208 (+235) |
| After Sprint 3 | 42.1% | +30 (1,505) | 15,433 (+225) |
| **Phase 2 Complete** | **42.1%** | **+98** | **15,433** |

---

## 🚀 Quick Wins (Can be done in parallel)

### Quick Win 1: Simple Options/POCOs ⏱️ 30 min
- SecurityOptions, EndpointLimits, RoleLimits
- Just test property getters/setters
- **Lines**: ~60 | **Tests**: 7-11 | **Impact**: +0.16%

### Quick Win 2: No-Op Implementations ⏱️ 15 min
- NoOpClaimsTransformation
- Test that it returns input unchanged
- **Lines**: ~20 | **Tests**: 2-3 | **Impact**: +0.05%

### Quick Win 3: Filter Edge Cases ⏱️ 45 min
- Complete coverage for existing filters
- Test null inputs, empty collections
- **Lines**: ~30 | **Tests**: 5-7 | **Impact**: +0.08%

**Quick Wins Total**: ~110 lines | 14-21 tests | **+0.30%** in <2 hours

---

## 🎓 Learnings & Standards

### Coverage Thresholds (Aligned with CI)
- **Minimum (CI Warning)**: 70% line, 60% branch, 70% method
- **Recommended**: 85% line, 75% branch, 85% method
- **Excellent**: 90%+ line, 80%+ branch, 90%+ method

### Test Categories
1. **Unit Tests**: Isolated component logic (80% of new tests)
2. **Integration Tests**: Component interactions (15% of new tests)
3. **E2E Tests**: Critical user flows (5% of new tests)

### Tracking
- Create GitHub issues for each Sprint
- Update this plan daily with progress
- Run local coverage after each task
- Merge to master when Sprint completes

---

## 📝 Next Steps

1. **Start with Quick Wins** (today) - Get easy +0.30% boost
2. **Sprint 1 Task 1.1** (tomorrow) - AppHost testing (highest impact)
3. **Daily standup** - Update progress, adjust priorities
4. **Weekly review** - Validate coverage gains, pivot if needed

---

**Created**: 2 Dec 2025  
**Last Updated**: 2 Dec 2025  
**Owner**: @frigini  
**Status**: 🟡 Ready to start
