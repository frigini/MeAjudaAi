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

## Monitoring and Maintenance

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