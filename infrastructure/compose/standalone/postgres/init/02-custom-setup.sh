#!/bin/bash
# PostgreSQL Initialization Shell Script Example
# This script runs after SQL scripts and can perform additional setup

set -e

echo "🔧 Running custom PostgreSQL initialization..."

# Wait for PostgreSQL to be ready
until pg_isready -h localhost -p 5432 -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  echo "⏳ Waiting for PostgreSQL to be ready..."
  sleep 2
done

echo "✅ PostgreSQL is ready!"

# Example: Create additional users or perform complex setup
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create a read-only user for reporting (optional)
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'readonly_user') THEN
            CREATE ROLE readonly_user LOGIN PASSWORD 'readonly123';
        END IF;
    END
    \$\$;
    
    -- Grant read-only permissions
    GRANT CONNECT ON DATABASE $POSTGRES_DB TO readonly_user;
    GRANT USAGE ON SCHEMA app TO readonly_user;
    GRANT SELECT ON ALL TABLES IN SCHEMA app TO readonly_user;
    ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT SELECT ON TABLES TO readonly_user;
EOSQL

echo "🎉 Custom PostgreSQL setup completed successfully!"
echo "📊 Database: $POSTGRES_DB"
echo "👤 Main user: $POSTGRES_USER"
echo "📖 Read-only user: readonly_user (password: readonly123)"
echo "🏗️  Schema: app"