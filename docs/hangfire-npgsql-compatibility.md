# Hangfire + Npgsql 10.x Compatibility Guide

## ⚠️ CRITICAL COMPATIBILITY ISSUE

**Status**: UNVALIDATED RISK  
**Severity**: HIGH  
**Impact**: Production deployments BLOCKED until compatibility validated

### Problem Summary

- **Hangfire.PostgreSql 1.20.12** was compiled against **Npgsql 6.x**
- **Npgsql 10.x** introduces **BREAKING CHANGES** (see [release notes](https://www.npgsql.org/doc/release-notes/10.0.html))
- Runtime compatibility between these versions is **UNVALIDATED** by the Hangfire.PostgreSql maintainer
- Failure modes include: job persistence errors, serialization issues, connection failures, data corruption

## 🚨 Deployment Requirements

### MANDATORY VALIDATION BEFORE PRODUCTION DEPLOY

**DO NOT deploy to production** without completing ALL of the following:

1. ✅ **Integration Tests Pass**: All Hangfire integration tests in CI/CD pipeline MUST pass
   ```bash
   dotnet test --filter Category=HangfireIntegration
   ```

2. ✅ **Staging Environment Testing**: Manual validation in staging environment with production-like workload
   - Enqueue at least 100 test jobs
   - Verify job persistence across application restarts
   - Test automatic retry mechanism (induce failures)
   - Validate recurring job scheduling and execution
   - Monitor Hangfire dashboard for errors

3. ✅ **Production Monitoring Setup**: Configure monitoring BEFORE deploy
   - Hangfire job failure rate alerts (threshold: >5%)
   - Database error log monitoring for Npgsql exceptions
   - Application performance monitoring for background job processing
   - Dashboard health check endpoint

4. ✅ **Rollback Plan Documented**: Verified rollback procedure ready
   - Database backup taken before migration
   - Rollback script tested in staging
   - Estimated rollback time documented
   - Communication plan for stakeholders

## 📦 Package Version Strategy

### Current Approach (OPTION 1)

**Status**: TESTING - Requires validation  
**Package Versions**:
- `Npgsql.EntityFrameworkCore.PostgreSQL`: 10.0.0-rc.2
- `Hangfire.PostgreSql`: 1.20.12 (built against Npgsql 6.x)

**Validation Strategy**:
- Comprehensive integration tests (see `tests/MeAjudaAi.Integration.Tests/Jobs/HangfireIntegrationTests.cs`)
- CI/CD pipeline gates deployment on test success
- Staging environment verification with production workload
- Production monitoring for early failure detection

**Risks**:
- Unknown compatibility issues may emerge in production
- Npgsql 10.x breaking changes may affect Hangfire.PostgreSql internals
- No official support from Hangfire.PostgreSql maintainer for Npgsql 10.x

**Fallback Plan**: Downgrade to Option 2 if issues detected

### Alternative Approach (OPTION 2 - SAFE)

**Status**: FALLBACK if Option 1 fails  
**Package Versions**:
```xml
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageVersion Include="Npgsql" Version="8.0.5" />
<PackageVersion Include="Hangfire.PostgreSql" Version="1.20.12" />
```

**Trade-offs**:
- ✅ **Pro**: Known compatible versions (Npgsql 8.x + Hangfire.PostgreSql 1.20.12)
- ✅ **Pro**: Lower risk, proven in production
- ❌ **Con**: Delays .NET 10 migration benefits
- ❌ **Con**: Misses performance improvements in EF Core 10 / Npgsql 10

**When to use**:
- If integration tests fail in Option 1
- If staging environment detects Hangfire issues
- If production job failure rate exceeds 5%

### Future Approach (OPTION 3 - WAIT)

**Status**: NOT AVAILABLE YET  
**Waiting for**: Hangfire.PostgreSql 2.x with Npgsql 10 support

**Monitoring**:
- Watch: https://github.com/frankhommers/Hangfire.PostgreSql/issues
- NuGet package releases: https://www.nuget.org/packages/Hangfire.PostgreSql

**Estimated Timeline**: Unknown - no official roadmap published

### Alternative Backend (OPTION 4 - SWITCH)

**Status**: EMERGENCY FALLBACK ONLY

**Options**:
1. **Hangfire.Pro.Redis**
   - Requires commercial license ($)
   - Proven scalability and reliability
   - No PostgreSQL dependency

2. **Hangfire.SqlServer**
   - Requires SQL Server infrastructure
   - Additional costs and complexity

3. **Hangfire.InMemory**
   - Development/testing ONLY
   - NOT suitable for production (jobs lost on restart)

## 🧪 Integration Testing

### Test Coverage

Comprehensive integration tests validate:
1. **Job Persistence**: Jobs are stored correctly in PostgreSQL via Npgsql 10.x
2. **Job Execution**: Background workers process jobs successfully
3. **Parameter Serialization**: Job arguments serialize/deserialize correctly
4. **Automatic Retry**: Failed jobs trigger retry mechanism
5. **Recurring Jobs**: Scheduled jobs are persisted and executed
6. **Database Connection**: Hangfire connects to PostgreSQL via Npgsql 10.x

### Running Tests Locally

```bash
# Run all Hangfire integration tests
dotnet test --filter Category=HangfireIntegration

# Run with detailed output
dotnet test --filter Category=HangfireIntegration --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~Hangfire_WithNpgsql10_ShouldPersistJobs"
```

### CI/CD Pipeline Integration

Tests are executed automatically in GitHub Actions:
- **Workflow**: `.github/workflows/pr-validation.yml`
- **Step**: "CRITICAL - Hangfire Npgsql 10.x Compatibility Tests"
- **Trigger**: Every pull request
- **Gating**: Pipeline FAILS if Hangfire tests fail

## 📊 Production Monitoring

### Key Metrics to Track

1. **Hangfire Job Failure Rate**
   - **Threshold**: Alert if >5% failure rate
   - **Action**: Investigate logs, consider rollback if persistent
   - **Query**: `SELECT COUNT(*) FROM hangfire.state WHERE name='Failed'`

2. **Npgsql Connection Errors**
   - **Monitor**: Application logs for NpgsqlException
   - **Patterns**: Connection timeouts, command execution failures
   - **Action**: Review exception stack traces for Npgsql 10 breaking changes

3. **Background Job Processing Time**
   - **Baseline**: Measure average processing time before migration
   - **Alert**: If processing time increases >50%
   - **Cause**: Potential Npgsql performance regression

4. **Hangfire Dashboard Health**
   - **Endpoint**: `/hangfire`
   - **Check**: Dashboard loads without errors
   - **Frequency**: Every 5 minutes
   - **Alert**: If dashboard becomes inaccessible

### Logging Configuration

Enable detailed Hangfire + Npgsql logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Hangfire": "Information",
      "Npgsql": "Warning",
      "Npgsql.Connection": "Information",
      "Npgsql.Command": "Debug"
    }
  }
}
```

**Note**: Set `Npgsql.Command` to `Debug` only for troubleshooting (high log volume)

### Monitoring Queries

```sql
-- Job failure rate (last 24 hours)
SELECT 
    COUNT(CASE WHEN s.name = 'Failed' THEN 1 END)::float / COUNT(*)::float * 100 AS failure_rate_percent,
    COUNT(CASE WHEN s.name = 'Succeeded' THEN 1 END) AS succeeded_count,
    COUNT(CASE WHEN s.name = 'Failed' THEN 1 END) AS failed_count,
    COUNT(*) AS total_jobs
FROM hangfire.job j
JOIN hangfire.state s ON s.jobid = j.id
WHERE j.createdat > NOW() - INTERVAL '24 hours';

-- Failed jobs with error details
SELECT 
    j.id,
    j.createdat,
    s.reason AS failure_reason,
    s.data->>'ExceptionMessage' AS error_message
FROM hangfire.job j
JOIN hangfire.state s ON s.jobid = j.id
WHERE s.name = 'Failed'
ORDER BY j.createdat DESC
LIMIT 50;

-- Recurring jobs status
SELECT 
    id AS job_id,
    cron,
    createdat,
    lastexecution,
    nextexecution
FROM hangfire.set
WHERE key = 'recurring-jobs'
ORDER BY nextexecution ASC;
```

## 🔄 Rollback Procedure

### When to Rollback

Trigger rollback if:
- Hangfire job failure rate exceeds 5% for more than 1 hour
- Critical jobs fail repeatedly (e.g., payment processing, notifications)
- Npgsql connection errors spike in application logs
- Dashboard becomes unavailable or shows data corruption
- Database performance degrades significantly

### Rollback Steps

#### 1. Stop Application

```bash
# Azure App Service
az webapp stop --name meajudaai-api --resource-group meajudaai-prod

# Kubernetes
kubectl scale deployment meajudaai-api --replicas=0
```

#### 2. Restore Database Backup (if needed)

```bash
# Only if Hangfire schema is corrupted
pg_restore -h $DB_HOST -U $DB_USER -d $DB_NAME \
  --schema=hangfire \
  --clean --if-exists \
  hangfire_backup_$(date +%Y%m%d).dump
```

#### 3. Downgrade Packages

Update `Directory.Packages.props`:

```xml
<!-- Rollback to EF Core 9.x + Npgsql 8.x -->
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="9.0.0" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageVersion Include="Npgsql" Version="8.0.5" />

<!-- Hangfire remains unchanged -->
<PackageVersion Include="Hangfire.PostgreSql" Version="1.20.12" />
```

#### 4. Rebuild and Redeploy

```bash
dotnet restore MeAjudaAi.sln --force
dotnet build MeAjudaAi.sln --configuration Release
dotnet test --filter Category=HangfireIntegration  # Validate rollback

# Deploy rolled-back version
az webapp deployment source config-zip \
  --resource-group meajudaai-prod \
  --name meajudaai-api \
  --src release.zip
```

#### 5. Verify System Health

```bash
# Check Hangfire dashboard
curl -f https://api.meajudaai.com/hangfire || echo "Dashboard check failed"

# Verify jobs are processing
dotnet run -- test-hangfire-job

# Monitor logs for 30 minutes
az webapp log tail --name meajudaai-api --resource-group meajudaai-prod
```

#### 6. Post-Rollback Actions

- [ ] Document the specific failure that triggered rollback
- [ ] Open issue on Hangfire.PostgreSql GitHub repo if bug found
- [ ] Update `docs/deployment_environments.md` with lessons learned
- [ ] Notify stakeholders of rollback and estimated time to retry upgrade

### Estimated Rollback Time

- **Preparation**: 15 minutes (stop application, backup database)
- **Execution**: 30 minutes (package downgrade, rebuild, redeploy)
- **Validation**: 30 minutes (health checks, monitoring)
- **Total**: ~1.5 hours

### Rollback Testing

Test rollback procedure in staging environment:

```bash
# 1. Deploy Npgsql 10.x version to staging
./scripts/deploy-staging.sh --version npgsql10

# 2. Induce Hangfire failures (if any)
# 3. Practice rollback procedure
./scripts/rollback-staging.sh --version npgsql8

# 4. Verify system recovery
./scripts/verify-staging-health.sh
```

## 📚 Additional Resources

### Official Documentation

- [Npgsql 10.0 Release Notes](https://www.npgsql.org/doc/release-notes/10.0.html)
- [Hangfire.PostgreSql GitHub Repository](https://github.com/frankhommers/Hangfire.PostgreSql)
- [Hangfire Documentation](https://docs.hangfire.io/)

### Breaking Changes in Npgsql 10.x

Key breaking changes that may affect Hangfire.PostgreSql:
1. **Type mapping changes**: Some PostgreSQL type mappings updated
2. **Connection pooling**: Internal connection pool refactored
3. **Command execution**: Command execution internals changed
4. **Async I/O**: Async implementation overhauled
5. **Parameter binding**: Parameter binding logic updated

See full list: https://www.npgsql.org/doc/release-notes/10.0.html#breaking-changes

### Internal Documentation

- `Directory.Packages.props` - Package version comments (lines 45-103)
- `tests/MeAjudaAi.Integration.Tests/Jobs/HangfireIntegrationTests.cs` - Test implementation
- `.github/workflows/pr-validation.yml` - CI/CD integration
- `docs/deployment_environments.md` - Deployment procedures

## 🆘 Troubleshooting

### Common Issues

#### Issue: Hangfire tables not created

**Symptom**: Application starts but Hangfire dashboard shows errors  
**Cause**: PrepareSchemaIfNecessary not working with Npgsql 10.x

**Solution**:
```bash
# Manually create Hangfire schema
psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c "CREATE SCHEMA IF NOT EXISTS hangfire;"

# Re-run application (Hangfire will create tables)
dotnet run
```

#### Issue: Job serialization failures

**Symptom**: Jobs enqueue but fail to deserialize parameters  
**Cause**: JSON serialization changes in Npgsql 10.x

**Solution**:
```csharp
// Check Hangfire GlobalConfiguration for serializer settings
GlobalConfiguration.Configuration
    .UseSerializerSettings(new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Objects,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    });
```

#### Issue: Connection pool exhaustion

**Symptom**: "connection pool exhausted" errors in logs  
**Cause**: Npgsql 10.x connection pooling changes

**Solution**:
```
# Increase connection pool size in connection string
Host=localhost;Database=meajudaai;Maximum Pool Size=100;
```

### Getting Help

1. **Internal team**: Post in #backend-infrastructure Slack channel
2. **Hangfire.PostgreSql**: Open issue at https://github.com/frankhommers/Hangfire.PostgreSql/issues
3. **Npgsql**: Open discussion at https://github.com/npgsql/npgsql/discussions

---

**Last Updated**: 2025-11-21  
**Owner**: Backend Infrastructure Team  
**Review Frequency**: Weekly until Npgsql 10.x compatibility validated
