# Database Initialization Scripts

This directory contains PostgreSQL initialization scripts that are automatically executed when the database container starts for the first time.

## Structure

```text
database/
├── 01-init-meajudaai.sh          # Main initialization orchestrator
├── modules/                       # Module-specific database setup
│   ├── users/                    # Users module schema and permissions
│   │   ├── 00-roles.sql          # Database roles for users module
│   │   └── 01-permissions.sql    # Permissions setup for users module
│   ├── providers/                # Providers module schema and permissions
│   │   ├── 00-roles.sql          # Database roles for providers module
│   │   └── 01-permissions.sql    # Permissions setup for providers module
│   └── documents/                # Documents module schema and permissions
│       ├── 00-roles.sql          # Database roles for documents module (includes hangfire_role)
│       └── 01-permissions.sql    # Permissions setup for documents and hangfire schemas
└── views/                        # Cross-module database views
    └── cross-module-views.sql    # Views that span multiple modules (includes document status views)
```

## Execution Order

PostgreSQL executes initialization scripts in alphabetical order:

1. `01-init-meajudaai.sh` - Main orchestrator script that:
   - Sets up each module in proper order
   - Executes role creation before permissions
   - Sets up cross-module views last
   - Provides logging and error handling

2. Individual SQL files are executed by the shell script in logical order

## Adding New Modules

To add a new module:

1. Create directory: `modules/[module-name]/`
2. Add `00-roles.sql` for database roles
3. Add `01-permissions.sql` for permissions setup
4. The initialization script will automatically detect and execute them

## Hangfire Background Jobs

The `documents` module includes setup for **Hangfire** background job processing:

- **Schema**: `hangfire` - Isolated schema for Hangfire tables
- **Role**: `hangfire_role` - Dedicated role with full permissions on hangfire schema
- **Access**: Hangfire has SELECT/UPDATE access to `meajudaai_documents` schema for DocumentVerificationJob
- **Configuration**: Hangfire automatically creates its tables on first run (PrepareSchemaIfNecessary=true)

The Hangfire dashboard is available at `/hangfire` endpoint when the application is running.

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