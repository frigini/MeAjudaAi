# PostgreSQL Initialization Scripts

This directory contains SQL scripts and shell scripts that are automatically executed when the standalone PostgreSQL container starts for the first time.

## Execution Order

PostgreSQL executes initialization scripts in alphabetical order from the `/docker-entrypoint-initdb.d/` directory inside the container.

## Files

### `01-init-standalone.sql`
- Creates useful PostgreSQL extensions (`uuid-ossp`, `pgcrypto`)
- Sets up an `app` schema for development
- Creates a sample `users` table with UUID primary keys
- Inserts sample development data
- Grants appropriate permissions

## Adding Custom Scripts

You can add your own initialization scripts to this directory:

1. **SQL Scripts**: Name them with `.sql` extension (e.g., `02-my-tables.sql`)
2. **Shell Scripts**: Name them with `.sh` extension (e.g., `03-custom-setup.sh`)
3. **Execution Order**: Use numeric prefixes to control order (01-, 02-, 03-, etc.)

## Environment Variables

Scripts can use these environment variables:
- `$POSTGRES_DB` - Database name (default: MeAjudaAi)
- `$POSTGRES_USER` - Database user (default: postgres)
- `$POSTGRES_PASSWORD` - Database password (required)

## Example Usage

```sql
-- Example custom script: 02-my-feature.sql
CREATE TABLE IF NOT EXISTS app.my_feature (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

## Permissions

Ensure all scripts in this directory have appropriate read permissions for Docker:
```bash
chmod 644 *.sql
chmod 755 *.sh
```

## Docker Integration

The parent directory (`postgres/`) is mounted to `/docker-entrypoint-initdb.d/` in the PostgreSQL container:

```yaml
volumes:
  - ./postgres/init:/docker-entrypoint-initdb.d
```

This allows the PostgreSQL container to automatically discover and execute these initialization scripts on first startup.