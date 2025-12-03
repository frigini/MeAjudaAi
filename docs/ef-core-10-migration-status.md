# Entity Framework Core 10.0.0 Migration Status

## Current Status: ⚠️ BLOCKED - Waiting for Npgsql Stable Release

### Timeline
- **November 11, 2025**: EF Core 10.0.0 stable released
- **December 3, 2025** (TODAY): Npgsql still at 10.0.0-rc.2
- **Expected Q1 2026**: Npgsql 10.0.0 stable release

### The Problem

Dependabot PR #41 attempted to update `Microsoft.EntityFrameworkCore.Relational` from RC to stable, which caused a version conflict:

```
NU1107: Version conflict detected for Microsoft.EntityFrameworkCore
  MeAjudaAi.Shared -> Microsoft.EntityFrameworkCore (>= 10.0.0)
  Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0-rc.2 -> Microsoft.EntityFrameworkCore (= 10.0.0-rc.2.25502.107)
```

### Why This Happens

| Package | Current Version | Stable Available? | Issue |
|---------|----------------|-------------------|-------|
| **Microsoft.EntityFrameworkCore** | 10.0.0-rc.2.25502.107 | ✅ 10.0.0 | Can upgrade |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 10.0.0-rc.2 | ❌ Not yet | **BLOCKS upgrade** |
| **Hangfire.PostgreSql** | 1.20.12 (built for Npgsql 6.x) | ⚠️ Compatibility unknown | Needs validation |

**Root Cause**: Npgsql maintainers have not yet released the stable 10.0.0 version, and the RC version requires EF Core RC exactly (not stable).

### Current Configuration

**Directory.Packages.props** (Lines 36-47):
```xml
<!-- EF Core 10.0.0 RTM was released 11/11/2025, but Npgsql.EntityFrameworkCore.PostgreSQL does not yet -->
<!-- have a compatible stable 10.x release. Npgsql 10.0.0-rc.2 requires EF Core 10.0.0-rc.2.25502.107. -->
<!-- TODO: Upgrade to EF Core 10.0.0 stable once Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 is released -->
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0-rc.2.25502.107" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-rc.2.25502.107" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0-rc.2.25502.107" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0-rc.2.25502.107" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-rc.2.25502.107" />
<PackageVersion Include="EFCore.NamingConventions" Version="10.0.0-rc.2" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0-rc.2" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite" Version="10.0.0-rc.2" />
```

**.github/dependabot.yml** (Lines 108-121):
```yaml
# CRITICAL: Bloquear atualizações parciais do EF Core até Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 estável
# Contexto: Npgsql 10.0.0-rc.2 requer EF Core 10.0.0-rc.2.25502.107 EXATAMENTE
# Atualizar apenas EF Core para 10.0.0 estável sem Npgsql causa conflito NU1107
# Ação: Quando Npgsql 10.0.0 estável for lançado, remover estas regras e atualizar tudo junto
# Monitorar: https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL
- dependency-name: "Microsoft.EntityFrameworkCore"
  versions: ["10.0.0"]
- dependency-name: "Microsoft.EntityFrameworkCore.Design"
  versions: ["10.0.0"]
- dependency-name: "Microsoft.EntityFrameworkCore.Relational"
  versions: ["10.0.0"]
- dependency-name: "Microsoft.EntityFrameworkCore.InMemory"
  versions: ["10.0.0"]
- dependency-name: "Microsoft.EntityFrameworkCore.Sqlite"
  versions: ["10.0.0"]
```

### Actions Taken

1. ✅ **Configured Dependabot** to block EF Core 10.0.0 stable updates
2. ✅ **Documented the issue** in Directory.Packages.props with extensive comments
3. ✅ **Created integration tests** for Hangfire compatibility validation
4. ⚠️ **PR #41 must be closed** - Cannot merge due to version conflict

### What Happens Next

#### When Npgsql 10.0.0 Stable is Released:

1. **Update `.github/dependabot.yml`**:
   ```yaml
   # Remove lines 108-121 (the EF Core blocks)
   ```

2. **Update `Directory.Packages.props`**:
   ```xml
   <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
   <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
   <PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0" />
   <PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
   <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
   <PackageVersion Include="EFCore.NamingConventions" Version="10.0.0" />
   <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
   <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite" Version="10.0.0" />
   ```

3. **Run full test suite**:
   ```bash
   dotnet restore --force-evaluate --locked-mode
   dotnet build
   dotnet test
   ```

4. **Validate Hangfire integration**:
   ```bash
   dotnet test --filter "Category=HangfireIntegration"
   ```

5. **Update documentation** - Remove "TODO" comments about waiting for Npgsql

### Monitoring

**Check for Npgsql updates**:
- NuGet: https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL
- GitHub: https://github.com/npgsql/efcore.pg/releases
- RSS Feed: https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/feed/

**Check for Hangfire updates**:
- NuGet: https://www.nuget.org/packages/Hangfire.PostgreSql
- GitHub: https://github.com/frankhommers/Hangfire.PostgreSql/releases

### Alternative Options (Not Recommended)

#### Option 1: Downgrade to EF Core 9.x
```xml
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
```
- ✅ Stable and compatible
- ❌ Delays .NET 10 adoption
- ❌ Requires rollback of migration work

#### Option 2: Switch Hangfire Backend
```xml
<!-- Replace Hangfire.PostgreSql with: -->
<PackageVersion Include="Hangfire.Pro.Redis" Version="3.x" />
<!-- OR -->
<PackageVersion Include="Hangfire.InMemory" Version="0.x" /> <!-- Dev/Test only -->
```
- ✅ Removes Npgsql dependency for Hangfire
- ❌ Redis requires license or additional infrastructure
- ❌ InMemory not suitable for production

### Related Issues

- **PR #41**: Bump the ef-core group with 1 update (REJECT)
- **Issue #38**: Aspire.Npgsql.EntityFrameworkCore.PostgreSQL compatibility
- **Issue #39**: Hangfire.PostgreSql 2.x awaiting release

### Integration Test Coverage

**File**: `tests/MeAjudaAi.Integration.Tests/Jobs/HangfireIntegrationTests.cs`

**Tests** (Category=HangfireIntegration):
1. ✅ `Hangfire_WithNpgsql10_ShouldConnectToDatabase` - Connection test
2. ✅ `Hangfire_WithNpgsql10_ShouldPersistJobs` - Job persistence
3. ✅ `Hangfire_WithNpgsql10_ShouldExecuteEnqueuedJobs` - Job execution
4. ✅ `Hangfire_WithNpgsql10_ShouldRetryFailedJobs` - Retry mechanism
5. ✅ `Hangfire_WithNpgsql10_ShouldScheduleRecurringJobs` - Recurring jobs
6. ✅ `Hangfire_WithNpgsql10_ShouldSerializeJobParameters` - Serialization

**CI/CD Gate**: Pipeline fails if any Hangfire tests fail

### Summary

| Item | Status |
|------|--------|
| EF Core 10.0.0 Stable | ✅ Released |
| Npgsql 10.0.0 Stable | ❌ **WAITING** |
| Dependabot Blocks | ✅ Configured |
| Integration Tests | ✅ Passing |
| Documentation | ✅ Complete |
| PR #41 Action | ⚠️ Close/Reject |

**Conclusion**: The project is correctly configured with EF Core 10.0 RC and waiting for Npgsql stable release. No action needed until Npgsql 10.0.0 is available on NuGet.
