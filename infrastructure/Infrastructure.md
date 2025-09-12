# MeAjudaAi Infrastructure

This directory contains all infrastructure-related configurations for the MeAjudaAi project, organized in a professional and modular structure.

## Directory Structure

```
infrastructure/
├── compose/
│   ├── base/                    # Modular service definitions
│   │   ├── postgres.yml         # PostgreSQL database
│   │   ├── keycloak.yml         # Keycloak authentication
│   │   ├── redis.yml            # Redis cache
│   │   └── rabbitmq.yml         # RabbitMQ messaging
│   ├── environments/            # Complete environment setups
│   │   ├── development.yml      # Full development stack
│   │   ├── testing.yml          # Testing environment
│   │   └── production.yml       # Production configuration
│   └── standalone/              # Individual services
│       ├── keycloak-only.yml    # Just Keycloak
│       └── postgres-only.yml    # Just PostgreSQL
├── keycloak/                    # Keycloak configuration
│   ├── config/
│   │   ├── development/         # Dev environment variables
│   │   ├── production/          # Prod environment template
│   │   └── realm-import/        # Realm configuration
│   └── README.md
├── scripts/                     # Convenience scripts
│   ├── start-dev.sh            # Start development environment
│   ├── start-keycloak.sh       # Start only Keycloak
│   └── stop-all.sh             # Stop all services
├── docs/                        # Documentation
│   └── docker-setup.md         # Detailed setup guide
└── Infrastructure.md            # This file
```

## Quick Start

### Development Environment (Recommended)

Start the complete development environment:

```bash
# Using convenience script
./scripts/start-dev.sh

# Or manually
cd compose
docker compose -f environments/development.yml up -d
```

### Keycloak Only

For authentication-only development:

```bash
# Using convenience script
./scripts/start-keycloak.sh

# Or manually
cd compose
docker compose -f standalone/keycloak-only.yml up -d
```

### Stop All Services

```bash
./scripts/stop-all.sh
```

## Services & Access

### Development Environment

| Service | URL | Credentials |
|---------|-----|-------------|
| Keycloak Admin | http://localhost:8080 | admin/admin |
| PgAdmin | http://localhost:8081 | admin@meajudaai.com/admin |
| RabbitMQ Management | http://localhost:15672 | guest/guest |
| PostgreSQL | localhost:5432 | postgres/dev123 |
| Redis | localhost:6379 | (no auth) |

### Testing Environment

Separate ports to avoid conflicts with development:

| Service | URL | Credentials |
|---------|-----|-------------|
| Keycloak Test | http://localhost:8081 | admin/admin |
| PostgreSQL Test | localhost:5433 | postgres/test123 |
| Redis Test | localhost:6380 | (no auth) |

## Architecture Benefits

### 1. Modular Design
- **Base services**: Reusable components
- **Environment-specific**: Complete setups for different scenarios
- **Standalone services**: Individual services when needed

### 2. Environment Separation
- **Development**: Full-featured with management tools
- **Testing**: Lightweight, optimized for CI/CD
- **Production**: Security-focused with health checks

### 3. Aspire Compatibility
The structure is designed for future .NET Aspire integration:
- Consistent naming conventions
- Standard environment variable patterns
- Health check endpoints
- Service discovery ready

### 4. Professional Organization
- Clear separation of concerns
- Comprehensive documentation
- Convenience scripts for common tasks
- Production-ready configurations

## Environment Variables

### Development
No configuration needed - uses safe defaults.

### Production
Copy and customize the template:

```bash
cp keycloak/config/production/keycloak.env.template keycloak/config/production/keycloak.env
# Edit with your production values
```

Required variables:
- Database passwords
- Keycloak admin credentials
- Domain configuration
- Redis authentication
- RabbitMQ credentials

## Migration from Aspire

When you're ready to integrate with .NET Aspire:

1. **Gradual Migration**: Move services one by one to Aspire hosting
2. **External Dependencies**: Keep complex services (Keycloak) in Docker
3. **Shared Configuration**: Use same environment variable patterns
4. **Service Discovery**: Container names are already Aspire-compatible

## Documentation

For detailed setup instructions, troubleshooting, and advanced configurations, see:
- `docs/docker-setup.md` - Complete Docker setup guide
- `keycloak/README.md` - Keycloak-specific configuration

## Production Notes

The production compose file serves as a reference. For real production:

- Use Kubernetes or container orchestration
- Implement proper secrets management  
- Set up SSL/TLS certificates
- Configure monitoring and logging
- Implement backup strategies
- Use managed databases when available

## Migration Path

This organization supports your project evolution:

1. **Current**: Docker Compose for all services
2. **Transition**: Aspire for .NET services, Docker for external dependencies
3. **Future**: Full container orchestration platform

The modular structure ensures smooth transitions between these phases.