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

**BEFORE deploying to ANY environment**, ensure ALL critical compatibility validations pass.

For detailed Hangfire + Npgsql 10.x compatibility validation procedures, see the dedicated guide:
📖 **[Hangfire Npgsql Compatibility Guide](./hangfire-npgsql-compatibility.md)**

**Quick Checklist** (see full guide for details):
- [ ] All Hangfire integration tests pass (`dotnet test --filter Category=HangfireIntegration`)
- [ ] Manual validation in staging complete
- [ ] Monitoring configured (alerts, dashboards)
- [ ] Rollback procedure tested
- [ ] Team trained and stakeholders notified

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

For detailed rollback procedures and troubleshooting:
📖 **[Hangfire Npgsql Compatibility Guide](./hangfire-npgsql-compatibility.md)** _integration tests removed — monitor via health checks_

**Quick Rollback Steps**:

1. **Stop Application** (~5 min)
   ```bash
   az webapp stop --name $APP_NAME --resource-group $RESOURCE_GROUP
   ```

2. **Database Backup** (~10 min, if needed)
   ```bash
   pg_dump -h $DB_HOST -U $DB_USER --schema=hangfire -Fc > hangfire_backup.dump
   ```

3. **Downgrade Packages** (~15 min)
   - Revert to EF Core 9.x + Npgsql 8.x in `Directory.Packages.props`

4. **Rebuild & Redeploy** (~30 min)
   ```bash
   dotnet test --filter Category=HangfireIntegration  # Validate
   ```

5. **Verify Health** (~30 min)
   - Check Hangfire dashboard: `$API_ENDPOINT/hangfire`
   - Monitor job processing and logs

**Full Rollback Procedure**: See the dedicated compatibility guide for environment-agnostic commands and detailed troubleshooting.

## Monitoring and Maintenance

### Critical Monitoring

For comprehensive Hangfire + background jobs monitoring, see:
📖 **[Hangfire Npgsql Compatibility Guide](./hangfire-npgsql-compatibility.md)** _integration tests removed — monitor via health checks_

**Key Metrics** (see guide for queries and alert configuration):
1. **Job Failure Rate**: Alert if >5% → Investigate and consider rollback
2. **Npgsql Connection Errors**: Monitor application logs
3. **Dashboard Health**: Check `/hangfire` endpoint every 5 minutes
4. **Job Processing Time**: Alert if >50% increase from baseline

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