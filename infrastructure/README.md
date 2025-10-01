# MeAjudaAi Infrastructure

This directory contains the infrastructure configuration for the MeAjudaAi platform.

## 🔒 Security Requirements

**Before starting any environment**, you must configure secure credentials:

1. **Copy the environment template**:
   ```bash
   cp compose/environments/.env.example compose/environments/.env
   ```

2. **Set secure passwords** for all services in `.env`:
   - `POSTGRES_PASSWORD` - Main database password
   - `KEYCLOAK_DB_PASSWORD` - Keycloak database password  
   - `KEYCLOAK_ADMIN_PASSWORD` - Keycloak admin password
   - `PGADMIN_DEFAULT_EMAIL` - PgAdmin login email
   - `PGADMIN_DEFAULT_PASSWORD` - PgAdmin login password

3. **Security Guidelines**:
   - Use different passwords for each service
   - Passwords should be at least 16 characters
   - Never commit `.env` files with real credentials
   - Use a password manager for secure generation

⚠️ **Docker Compose will fail to start** if these environment variables are not set, preventing accidental deployment with default/weak credentials.

## Docker Compose Services

### Keycloak Authentication

**Version Management**: 
- **All environments use pinned versions**: No `:latest` tags for reproducibility
- **Current default**: `26.0.2`
- **Consistent across environments**: Development, testing, and production use same `KEYCLOAK_VERSION`
- **Override capability**: Set `KEYCLOAK_VERSION` environment variable to use different version
- **Testing and Upgrades**:
  - Always test new Keycloak versions in development first
  - Check [Keycloak Release Notes](https://www.keycloak.org/docs/latest/release_notes/index.html) for breaking changes
  - Update the default version in `.env.example` after validation
  - **When updating**: Change `KEYCLOAK_VERSION` in all environment files simultaneously

**HTTP/HTTPS Configuration**:
- **Development**: HTTP enabled for convenience (`KC_HTTP_ENABLED=true`)
- **Production**: HTTP enabled internally, HTTPS enforced at proxy level (`KC_PROXY=edge`)
- **Testing**: HTTP enabled for test environment simplicity
- All environments include `--import-realm` flag for automatic realm setup

### Environment Configuration

Copy `.env.example` to `.env` and configure:

```dotenv
# Keycloak Version (Production Stable)
KEYCLOAK_VERSION=26.0.2

# Keycloak Admin Configuration (REQUIRED for all environments)
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD="your-secure-admin-password-here"  # REQUIRED

# Database Configuration (REQUIRED for production)
POSTGRES_PASSWORD="your-secure-postgres-password-here"     # REQUIRED for prod
KEYCLOAK_DB_PASSWORD="your-secure-keycloak-db-password-here"  # REQUIRED for prod

# RabbitMQ Configuration (REQUIRED for production)
RABBITMQ_USER=meajudaai
RABBITMQ_PASS="your-secure-rabbitmq-password-here"         # REQUIRED for prod

# Additional production variables
KEYCLOAK_HOSTNAME="your-keycloak-domain.com"               # REQUIRED for prod
RABBITMQ_ERLANG_COOKIE="your-secure-erlang-cookie-here"    # REQUIRED for prod

# Other configuration variables...
```

### Development vs Production Security

**Development Environment** (`development.yml`):
- Uses weak default passwords for convenience (e.g., `dev123`, `keycloak`)
- **REQUIRES** `KEYCLOAK_ADMIN_PASSWORD` and `RABBITMQ_PASS` environment variables (no defaults provided)
- Suitable ONLY for local development
- **NEVER use development defaults for shared or deployed environments**

**Production/Shared Environments**:
- Override weak defaults using environment variables
- Set `POSTGRES_PASSWORD`, `KEYCLOAK_DB_PASSWORD`, `KEYCLOAK_ADMIN_PASSWORD`, and `RABBITMQ_PASS` to strong values
- All services require secure credentials via `.env` file

**Security Notes**: 
- `KEYCLOAK_ADMIN_PASSWORD` and `RABBITMQ_PASS` are required for ALL environments (including development)
- `POSTGRES_PASSWORD` is required for all non-development deployments
- `RABBITMQ_USER` and `RABBITMQ_PASS` are required for all non-development deployments
- The compose files will fail if these variables are not provided (no insecure defaults)

**Important**: Add environment files to your `.gitignore`:

```gitignore
# Infrastructure environment files
infrastructure/.env
infrastructure/*.env
infrastructure/*.env.*
infrastructure/**/.env*
infrastructure/compose/environments/.env.*
```

### Development Setup

**Required Before Starting Development Environment:**

1. **Generate Required Passwords:**
   ```bash
   # Generate secure passwords
   export KEYCLOAK_ADMIN_PASSWORD="$(openssl rand -base64 32)"
   export RABBITMQ_PASS="$(openssl rand -base64 32)"
   # Tip: avoid echoing secrets; consider writing to a local .env file with strict permissions
   # umask 077; printf 'KEYCLOAK_ADMIN_PASSWORD=%s\nRABBITMQ_PASS=%s\n' "$KEYCLOAK_ADMIN_PASSWORD" "$RABBITMQ_PASS" > compose/environments/.env.development
   ```

2. **Alternative: Create .env file:**
   ```bash
   # Copy example and edit
   cp compose/environments/.env.development.example compose/environments/.env.development
   # Edit .env.development file and set both passwords
   ```

3. **Start Development Environment:**
   ```bash
   docker compose -f compose/environments/development.yml up -d
   # OR with .env file:
   docker compose -f compose/environments/development.yml --env-file compose/environments/.env.development up -d
   ```

### Usage

```bash
# Development (with environment variables set)
export KEYCLOAK_ADMIN_PASSWORD=$(openssl rand -base64 32)
export RABBITMQ_PASS=$(openssl rand -base64 32)
docker compose -f compose/environments/development.yml up -d

# Production (with .env file)
docker compose -f compose/environments/production.yml up -d

# Testing (uses defaults or custom .env.testing)
docker compose -f compose/environments/testing.yml up -d

# Standalone services (require explicit passwords)
export POSTGRES_PASSWORD=$(openssl rand -base64 32)
docker compose -f compose/standalone/postgres-only.yml up -d

export KEYCLOAK_ADMIN_PASSWORD=$(openssl rand -base64 32)
docker compose -f compose/standalone/keycloak-only.yml up -d
```

### Standalone Services

**Location**: `compose/standalone/`

Individual service configurations for development scenarios where you only need specific components.

**Security**: All standalone services require explicit passwords (no unsafe defaults)
**Features**: PostgreSQL includes automatic database initialization with development schema
- See `compose/standalone/README.md` for detailed usage instructions
- Use `compose/standalone/.env.example` as a template for configuration
- (dev-only) PostgreSQL automatically creates `app` schema with sample data on first startup

### Testing Environment

**Characteristics**:
- **Lightweight configuration** optimized for CI/CD and local testing
- **Separate ports** to avoid conflicts with development environment
- **Environment variable driven** with sensible defaults
- **PostgreSQL optimizations** for faster test execution (fsync=off, etc.)
- **Health checks** prevent startup race conditions and ensure service readiness

**Configuration**:
```bash
# Optional: Create custom test configuration
cp compose/environments/.env.testing.example compose/environments/.env.testing
# Edit .env.testing if needed (defaults usually work)

# Run with custom config
docker compose -f compose/environments/testing.yml --env-file compose/environments/.env.testing up -d
```

**Default Credentials** (testing only):
- **Main DB**: `postgres/test123` on `localhost:5433`
- **Keycloak Admin**: `admin/admin` on `localhost:8081`
- **Keycloak DB**: `keycloak/keycloak`
- **Redis**: No auth on `localhost:6380`

## Version Management Best Practices

1. **Pin specific versions** for all production services
2. **Test upgrades** in development environment first
3. **Document version changes** in commit messages
4. **Monitor security advisories** for all used versions