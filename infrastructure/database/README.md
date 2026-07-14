# Database Initialization Scripts

This directory contains PostgreSQL initialization scripts that are automatically executed when the database container starts for the first time.

## Structure

```text
database/
├── 01-init-meajudaai.sh          # Docker init script (informational only)
├── modules/                       # Module-specific database setup (used at runtime)
│   ├── users/                    # Users module schema and permissions
│   │   ├── 00-roles.sql          # Database roles for users module
│   │   └── 01-permissions.sql    # Permissions setup for users module
│   ├── providers/                # Providers module schema and permissions
│   │   ├── 00-roles.sql          # Database roles for providers module
│   │   └── 01-permissions.sql    # Permissions setup for providers module
│   ├── documents/                # Documents module schema and permissions
│   │   ├── 00-roles.sql          # Database roles for documents module (includes hangfire_role)
│   │   └── 01-permissions.sql    # Permissions setup for documents and hangfire schemas
│   ├── search_providers/         # Search Providers module (PostGIS geospatial)
│   │   ├── 00-roles.sql          # Database roles for search_providers module
│   │   └── 01-permissions.sql    # Permissions setup and PostGIS extension
│   ├── locations/                # Locations module (CEP lookup and geocoding)
│   │   ├── 00-roles.sql          # Database roles for locations module
│   │   └── 01-permissions.sql    # Permissions setup for locations module
│   ├── service_catalogs/         # Service Catalog module (admin-managed)
│   │   ├── 00-roles.sql          # Database roles for service_catalogs module
│   │   └── 01-permissions.sql    # Permissions setup for service_catalogs module
│   ├── bookings/                 # Bookings module
│   │   ├── 00-roles.sql          # Database roles for bookings module
│   │   └── 01-permissions.sql    # Permissions setup for bookings module
│   ├── communications/           # Communications module
│   │   ├── 00-roles.sql          # Database roles for communications module
│   │   └── 01-permissions.sql    # Permissions setup for communications module
│   ├── payments/                 # Payments module
│   │   ├── 00-roles.sql          # Database roles for payments module
│   │   └── 01-permissions.sql    # Permissions setup for payments module
│   └── ratings/                  # Ratings module
│       ├── 00-roles.sql          # Database roles for ratings module
│       └── 01-permissions.sql    # Permissions setup for ratings module
└── seeds/                        # Essential domain data (run manually after migrations)
    ├── README.md                 # Seed documentation
    └── 01-seed-service-catalogs.sql  # ServiceCategories and Services initial data
```

## How It Works

### Runtime (Aspire / Application Startup)

The application handles all database setup automatically:

1. **EF Core Migrations** — Create schemas and tables for each module
2. **SchemaPermissionsManager** — Reads `modules/*/00-roles.sql` and `modules/*/01-permissions.sql`, replaces placeholders (`{{ROLE_NAME}}`, `{{SCHEMA_NAME}}`, etc.) with real values, and executes them
3. **ConfigureAllModulesSchemaIsolation()** — Registers all 10 modules for automatic schema isolation setup

This means roles, permissions, and schemas are created **at runtime**, not during Docker container initialization.

### Docker Init (Compose Fallback)

The `01-init-meajudaai.sh` script runs on first container start but is **informational only**:
- Module SQL files contain placeholders that are not substituted at Docker init time
- Seeds require tables that don't exist yet (created by EF Core migrations later)
- Keycloak creates its own `identity` schema via `KC_DB_SCHEMA` environment variable

To seed data manually after the application has run migrations:
```bash
docker exec -it meajudaai-postgres psql -U postgres -d meajudaai \
  -f /docker-entrypoint-initdb.d/seeds/01-seed-service-catalogs.sql
```

## Adding New Modules

To add a new module:

1. Create directory: `modules/[module-name]/`
2. Add `00-roles.sql` for database roles
3. Add `01-permissions.sql` for permissions setup
4. Register the module in `ConfigureAllModulesSchemaIsolation()` in `DatabaseExtensions.cs`

At runtime, `SchemaPermissionsManager` reads the SQL files, replaces `{{PLACEHOLDER}}` values, and executes them.

## Hangfire Background Jobs

The `documents` module includes setup for **Hangfire** background job processing:

- **Schema**: `hangfire` - Isolated schema for Hangfire tables
- **Role**: `hangfire_role` - Dedicated role with full permissions on hangfire schema
- **Access**: Hangfire has SELECT/UPDATE access to `documents` schema for DocumentVerificationJob
- **Configuration**: Hangfire automatically creates its tables on first run (PrepareSchemaIfNecessary=true)

The Hangfire dashboard is available at `/hangfire` endpoint when the application is running.

## Module Schemas

The database initialization creates the following schemas:

| Schema | Module | Purpose |
|--------|--------|---------|
| `identity` | Keycloak | Identity and access management (Keycloak) |
| `users` | Users | User accounts, authentication, and profile management |
| `providers` | Providers | Service provider registration and verification |
| `documents` | Documents | Document upload, verification, and storage metadata |
| `search_providers` | Search Providers | Geospatial provider search with PostGIS |
| `locations` | Locations | CEP lookup, address validation, and geocoding |
| `service_catalogs` | Service Catalog | Admin-managed service categories and services |
| `bookings` | Bookings | Service booking and scheduling |
| `communications` | Communications | Email templates and notifications |
| `payments` | Payments | Payment processing and subscriptions |
| `ratings` | Ratings | Provider ratings and reviews |
| `hangfire` | Background Jobs | Hangfire job queue and execution tracking |

## PostGIS Extension

The `search_providers` module automatically enables the **PostGIS** extension for geospatial queries:

- Provides geolocation-based provider search
- Supports distance calculations and radius filtering
- Includes spatial indexes (GIST) for performance
- Grants access to `spatial_ref_sys` table for coordinate transformations

## Usage

These scripts are automatically used when running:
```bash
docker compose -f infrastructure/compose/base/postgres.yml up
```

The database directory is mounted as `/docker-entrypoint-initdb.d` in the container.

## Security Notes

- Scripts run with `POSTGRES_USER` privileges
- Each module gets isolated database roles and schemas
- Cross-module access is controlled via specific views
- Production deployments should review and validate all scripts
- **Security guidelines**: See [docs/database-security.md](../../docs/database-security.md)