# Standalone Services

This directory contains Docker Compose files for running individual services independently, useful for development scenarios where you only need specific components.

## Available Services

### PostgreSQL Only

**File**: `postgres-only.yml`

A standalone PostgreSQL setup for development when you only need the database service.

### Keycloak Only

**File**: `keycloak-only.yml`

A standalone Keycloak setup with embedded H2 database for quick authentication testing.

### Usage

**1. Set Required Environment Variable:**
```bash
# Generate a secure password
export POSTGRES_PASSWORD=$(openssl rand -base64 32)
```

**2. Alternative: Use .env File:**
```bash
# Copy and configure environment template
cp .env.example .env
# Edit .env and set POSTGRES_PASSWORD
```

**3. Start PostgreSQL:**
```bash
docker compose -f postgres-only.yml up -d
```

### Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_DB` | `MeAjudaAi` | Database name |
| `POSTGRES_USER` | `postgres` | Database user |
| `POSTGRES_PASSWORD` | **REQUIRED** | Database password (no default) |
| `POSTGRES_PORT` | `5432` | Host port mapping |

### Features

- **Security**: Requires explicit password (no unsafe defaults)
- **Health Checks**: Built-in PostgreSQL readiness checks
- **Initialization Scripts**: Automatically runs scripts from `../../database/`
- **Data Persistence**: Uses named volumes for data retention

### Connection Details

- **Host**: `localhost`
- **Port**: `5432` (or custom via `POSTGRES_PORT`)
- **Database**: `MeAjudaAi` (or custom via `POSTGRES_DB`)
- **Username**: `postgres` (or custom via `POSTGRES_USER`)
- **Password**: Set via `POSTGRES_PASSWORD` environment variable

## Keycloak Only Usage

**1. Set Required Environment Variable:**
```bash
# Generate a secure password
export KEYCLOAK_ADMIN_PASSWORD=$(openssl rand -base64 32)
```

**2. Alternative: Use .env File:**
```bash
# Copy and configure environment template
cp .env.example .env
# Edit .env and set KEYCLOAK_ADMIN_PASSWORD
```

**3. Start Keycloak:**
```bash
docker compose -f keycloak-only.yml up -d
```

**4. Access Keycloak:**
- **URL**: http://localhost:8080
- **Username**: `admin` (or custom via `KEYCLOAK_ADMIN`)
- **Password**: Value from `KEYCLOAK_ADMIN_PASSWORD`

### PostgreSQL Initialization

The PostgreSQL service includes automatic database setup:

**Initialization Scripts**: Located in `postgres/init/`
- `01-init-standalone.sql` - Creates basic schema and sample data
- `02-custom-setup.sh` - Sets up additional users and permissions
- Custom scripts can be added following the naming convention

**Features**:
- Creates `app` schema with sample `users` table
- Installs useful extensions (`uuid-ossp`, `pgcrypto`)
- Sets up read-only user for reporting
- Automatic execution on first container startup

### Security Notes

⚠️ **Important Security Practices**:
- Always use strong passwords generated with `openssl rand -base64 32`
- Never commit `.env` files to version control
- These setups are for development only - use proper secrets management in production
- PostgreSQL service includes database initialization scripts for development convenience
- Keycloak uses embedded H2 database (data is not persistent between container restarts)
- Initialization scripts create development users with default passwords - change in production