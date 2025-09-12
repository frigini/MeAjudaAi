# Docker Compose Quick Reference

This file provides quick commands for the most common Docker operations in the MeAjudaAi project.

## Quick Commands

### Start Development Environment
```bash
# Full development stack (recommended)
./scripts/start-dev.sh

# Or manually:
cd compose
docker compose -f environments/development.yml up -d
```

### Start Individual Services
```bash
# Only Keycloak
./scripts/start-keycloak.sh

# Only PostgreSQL
cd compose
docker compose -f standalone/postgres-only.yml up -d
```

### Stop All Services
```bash
./scripts/stop-all.sh
```

### Check Status
```bash
docker ps --filter "name=meajudaai"
```

### View Logs
```bash
# All services
docker compose -f environments/development.yml logs -f

# Specific service
docker compose -f environments/development.yml logs -f keycloak
```

## Service URLs

| Service | Development | Testing |
|---------|-------------|---------|
| Keycloak Admin | http://localhost:8080 | http://localhost:8081 |
| PgAdmin | http://localhost:8081 | N/A |
| RabbitMQ Management | http://localhost:15672 | N/A |
| PostgreSQL | localhost:5432 | localhost:5433 |
| Redis | localhost:6379 | localhost:6380 |

## Default Credentials

- **Keycloak**: admin/admin
- **PostgreSQL**: postgres/dev123 (testing: postgres/test123)
- **PgAdmin**: admin@meajudaai.com/admin
- **RabbitMQ**: guest/guest

For detailed documentation, see `docs/docker-setup.md`.