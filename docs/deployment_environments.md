# Deployment Environments

## Overview
This document describes the different deployment environments available for the MeAjudaAi platform and their configurations.

## Environment Types

### Development Environment
- **Purpose**: Local development and testing
- **Configuration**: Simplified setup with local databases
- **Access**: Developer machines only
- **Database**: Local PostgreSQL container
- **Authentication**: Simplified for development

### Staging Environment
- **Purpose**: Pre-production testing and validation
- **Configuration**: Production-like setup with test data
- **Access**: Development team and stakeholders
- **Database**: Dedicated staging database
- **Authentication**: Full authentication system

### Production Environment
- **Purpose**: Live application serving real users
- **Configuration**: Fully secured and optimized
- **Access**: End users and authorized administrators
- **Database**: Production PostgreSQL with backups
- **Authentication**: Complete authentication with external providers

## Deployment Process

### ⚠️ CRITICAL: Pre-Deployment Validation

**BEFORE deploying to ANY environment**, ensure ALL critical compatibility validations pass:

#### Hangfire + Npgsql 10.x Compatibility Check

**Status**: MANDATORY - Deployment BLOCKED until validated  
**Documentation**: See `docs/hangfire-npgsql-compatibility.md`

**Pre-Deployment Checklist**:
- [ ] All Hangfire integration tests pass in CI/CD (`Category=HangfireIntegration`)
- [ ] Manual validation completed in staging environment
- [ ] Production monitoring configured (job failure rate, Npgsql errors)
- [ ] Rollback procedure tested and documented
- [ ] Database backup verified before production deploy
- [ ] Team trained on rollback procedure
- [ ] Stakeholders notified of compatibility risk

**Validation Command**:
```bash
# Must pass before deploy
dotnet test --filter Category=HangfireIntegration
```

**Risk**: Hangfire.PostgreSql 1.20.12 compiled against Npgsql 6.x, migrating to Npgsql 10.x with breaking changes. Runtime compatibility UNVALIDATED by upstream maintainer.

**Rollback Plan**: If Hangfire failures >5%, immediately execute rollback to Npgsql 8.x (see Rollback Procedures section below)

---

### Infrastructure Setup
The deployment process uses Bicep templates for infrastructure as code:

1. **Azure Resources**: Defined in `infrastructure/main.bicep`
2. **Service Bus**: Configured in `infrastructure/servicebus.bicep`
3. **Docker Compose**: Environment-specific configurations

### CI/CD Pipeline
Automated deployment through GitHub Actions:

1. **Build**: Compile and test the application
2. **Security Scan**: Vulnerability and secret detection
3. **Deploy**: Push to appropriate environment
4. **Validation**: Health checks and smoke tests

### Environment Variables
Each environment requires specific configuration:

- **Database connections**
- **Authentication providers**
- **Service endpoints**
- **Logging levels**
- **Feature flags**

## Rollback Procedures

### Hangfire + Npgsql Rollback (CRITICAL)

**Trigger Conditions** (execute rollback if ANY occur):
- Hangfire job failure rate exceeds 5% for >1 hour
- Critical background jobs fail repeatedly
- Npgsql connection errors spike in logs
- Dashboard unavailable or shows data corruption
- Database performance degrades significantly

**Rollback Steps** (Estimated time: 1.5 hours):

#### 1. Stop Application (5 minutes)
```bash
# Azure App Service
az webapp stop --name meajudaai-api --resource-group meajudaai-prod

# Kubernetes
kubectl scale deployment meajudaai-api --replicas=0
```

#### 2. Database Backup (10 minutes) - OPTIONAL if schema corrupted
```bash
# Only if Hangfire schema is corrupted
pg_restore -h $DB_HOST -U $DB_USER -d $DB_NAME \
  --schema=hangfire \
  --clean --if-exists \
  hangfire_backup_$(date +%Y%m%d).dump
```

#### 3. Downgrade Packages (15 minutes)

Update `Directory.Packages.props`:
```xml
<!-- Rollback to EF Core 9.x + Npgsql 8.x -->
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageVersion Include="Npgsql" Version="8.0.5" />
<!-- Hangfire.PostgreSql remains unchanged -->
<PackageVersion Include="Hangfire.PostgreSql" Version="1.20.12" />
```

#### 4. Rebuild and Redeploy (30 minutes)
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

#### 5. Verify System Health (30 minutes)
```bash
# Check Hangfire dashboard
curl -f https://api.meajudaai.com/hangfire

# Verify jobs processing
dotnet run -- test-hangfire-job

# Monitor logs
az webapp log tail --name meajudaai-api --resource-group meajudaai-prod
```

#### 6. Post-Rollback Actions
- [ ] Document failure root cause
- [ ] Open GitHub issue / Hangfire.PostgreSql bug report
- [ ] Update documentation with lessons learned
- [ ] Notify stakeholders of rollback completion

**Detailed Rollback Guide**: See `docs/hangfire-npgsql-compatibility.md`

## Monitoring and Maintenance

### Critical Monitoring (Hangfire + Background Jobs)

**Mandatory Monitoring**:
1. **Job Failure Rate**: Alert if >5%
   ```sql
   SELECT COUNT(CASE WHEN s.name = 'Failed' THEN 1 END)::float / COUNT(*)::float * 100
   FROM hangfire.job j JOIN hangfire.state s ON s.jobid = j.id
   WHERE j.createdat > NOW() - INTERVAL '24 hours';
   ```

2. **Npgsql Connection Errors**: Monitor application logs
   - Pattern: `NpgsqlException`, `connection pool exhausted`
   - Action: Investigate and consider rollback

3. **Dashboard Health**: `/hangfire` endpoint check every 5 minutes

4. **Job Processing Time**: Baseline + alert if >50% increase

**Logging Configuration for Troubleshooting**:
```json
{
  "Logging": {
    "LogLevel": {
      "Hangfire": "Information",
      "Npgsql": "Warning",
      "Npgsql.Connection": "Information"
    }
  }
}
```

### Health Checks
- Application health endpoints
- Database connectivity
- External service availability

### Logging
- Structured logging with Serilog
- Application insights integration
- Error tracking and alerting

### Backup and Recovery
- Regular database backups
- Infrastructure state backups
- Disaster recovery procedures

## Related Documentation

- [CI/CD Setup](../CI-CD-Setup.md)
- [Infrastructure Documentation](../../infrastructure/Infrastructure.md)
- [Development Guidelines](../development.md)