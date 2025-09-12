# Docker Setup Guide

This guide explains how to use the Docker Compose configurations for the MeAjudaAi project.

## Directory Structure

```
infrastructure/
├── compose/
│   ├── base/                    # Modular service definitions
│   │   ├── postgres.yml
│   │   ├── keycloak.yml
│   │   ├── redis.yml
│   │   └── rabbitmq.yml
│   ├── environments/            # Complete environment setups
│   │   ├── development.yml      # Full development stack
│   │   ├── testing.yml          # Testing environment
│   │   └── production.yml       # Production configuration
│   └── standalone/              # Individual services
│       ├── keycloak-only.yml    # Just Keycloak
│       └── postgres-only.yml    # Just PostgreSQL
├── keycloak/                    # Keycloak configuration
├── scripts/                     # Convenience scripts
└── docs/                        # Documentation
```

## Quick Start

### Development Environment (Recommended)

Start the complete development environment with all services:

```bash
# From infrastructure directory
./scripts/start-dev.sh

# Or manually:
cd compose
docker compose -f environments/development.yml up -d
```

This includes:
- PostgreSQL (main database)
- Keycloak + PostgreSQL (authentication)
- Redis (caching)
- RabbitMQ (messaging)
- PgAdmin (database management)

### Keycloak Only

If you only need Keycloak for authentication testing:

```bash
# From infrastructure directory
./scripts/start-keycloak.sh

# Or manually:
cd compose
docker compose -f standalone/keycloak-only.yml up -d
```

### Stop All Services

```bash
# From infrastructure directory
./scripts/stop-all.sh
```

## Available Configurations

### Environments

1. **Development** (`environments/development.yml`)
   - All services with development settings
   - Default passwords and configurations
   - Includes management tools (PgAdmin)

2. **Testing** (`environments/testing.yml`)
   - Lightweight setup for automated tests
   - Separate ports to avoid conflicts
   - Optimized for fast startup/teardown

3. **Production** (`environments/production.yml`)
   - Security-focused configuration
   - Environment variable-based secrets
   - Health checks and logging
   - **Note**: Use Kubernetes in real production

### Base Services

Individual services that can be combined:

- `base/postgres.yml` - PostgreSQL database
- `base/keycloak.yml` - Keycloak with its own PostgreSQL
- `base/redis.yml` - Redis cache
- `base/rabbitmq.yml` - RabbitMQ message broker

### Standalone Services

Quick single-service setups:

- `standalone/keycloak-only.yml` - Keycloak with embedded H2 database
- `standalone/postgres-only.yml` - Just PostgreSQL

## Service Access

### Development Environment

| Service | URL | Credentials |
|---------|-----|-------------|
| Keycloak Admin | http://localhost:8080 | admin/admin |
| PgAdmin | http://localhost:8081 | admin@meajudaai.com/admin |
| RabbitMQ Management | http://localhost:15672 | guest/guest |
| PostgreSQL | localhost:5432 | postgres/dev123 |
| Redis | localhost:6379 | (no auth) |

### Testing Environment

| Service | URL | Credentials |
|---------|-----|-------------|
| Keycloak Test | http://localhost:8081 | admin/admin |
| PostgreSQL Test | localhost:5433 | postgres/test123 |
| Redis Test | localhost:6380 | (no auth) |

## Environment Variables

### Development

Development uses hardcoded safe values. No environment file needed.

### Production

Copy and modify the template:

```bash
cp keycloak/config/production/keycloak.env.template keycloak/config/production/keycloak.env
# Edit the file with your production values
```

Required production variables:
- `POSTGRES_PASSWORD`
- `KEYCLOAK_ADMIN_PASSWORD`
- `KEYCLOAK_DB_PASSWORD`
- `KEYCLOAK_HOSTNAME`
- `REDIS_PASSWORD`
- `RABBITMQ_USER`
- `RABBITMQ_PASS`

## Common Commands

### View running containers
```bash
docker ps --filter "name=meajudaai"
```

### View logs
```bash
# All services
docker compose -f environments/development.yml logs -f

# Specific service
docker compose -f environments/development.yml logs -f keycloak
```

### Clean up volumes (DANGER: Deletes all data)
```bash
docker volume ls | grep meajudaai | awk '{print $2}' | xargs docker volume rm
```

### Rebuild containers
```bash
docker compose -f environments/development.yml down
docker compose -f environments/development.yml up -d --force-recreate
```

## Integration with Aspire

The current structure is designed to be compatible with future .NET Aspire integration:

1. **Service Discovery**: Container names follow consistent patterns
2. **Environment Variables**: Standard configuration approach
3. **Health Checks**: All services include health check endpoints
4. **Logging**: Structured logging configuration ready

When migrating to Aspire:
1. Services can be gradually moved to Aspire hosting
2. Docker Compose can remain for external dependencies
3. Same environment variable patterns can be used

## Troubleshooting

### Port Conflicts
If you get port conflicts, check what's running:
```bash
netstat -tulpn | grep :8080
```

### Database Connection Issues
1. Ensure containers are running: `docker ps`
2. Check logs: `docker compose logs postgres`
3. Verify network connectivity: `docker network ls`

### Keycloak Import Issues
1. Check if realm file exists: `ls keycloak/config/realm-import/`
2. Verify volume mount in logs: `docker compose logs keycloak`

### Performance Issues
For better performance in development:
1. Allocate more memory to Docker Desktop
2. Use Docker volumes instead of bind mounts for databases
3. Enable Docker BuildKit