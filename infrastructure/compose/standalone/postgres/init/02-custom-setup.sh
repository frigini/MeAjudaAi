#!/bin/bash
# PostgreSQL Initialization Shell Script Example
# This script runs after SQL scripts and can perform additional setup

set -e

# Export PSQLRC to prevent reading user's .psqlrc
export PSQLRC=/dev/null

echo "🔧 Running custom PostgreSQL initialization..."

# Check or generate readonly user password
if [ -z "$READONLY_USER_PASSWORD" ]; then
    echo "❌ ERROR: READONLY_USER_PASSWORD environment variable is not set!"
    echo "Please set a secure password for the readonly user:"
    echo "export READONLY_USER_PASSWORD='your-secure-password-here'"
    exit 1
fi

# Export the variable to ensure it's available for subprocesses
export READONLY_USER_PASSWORD

export PGPASSWORD="${PGPASSWORD:-$POSTGRES_PASSWORD}"
# Wait for PostgreSQL to be ready
until pg_isready -h localhost -p 5432 -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  echo "⏳ Waiting for PostgreSQL to be ready..."
  sleep 2
done

echo "✅ PostgreSQL is ready!"

# Set the password in session configuration for secure access
psql -v ON_ERROR_STOP=1 -v READONLY_USER_PASSWORD="$READONLY_USER_PASSWORD" --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -c \
    "SELECT set_config('app.readonly_user_password', :'READONLY_USER_PASSWORD', false);" > /dev/null

# Example: Create additional users or perform complex setup
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create a read-only user for reporting (optional)
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'readonly_user') THEN
            CREATE ROLE readonly_user LOGIN PASSWORD current_setting('app.readonly_user_password');
        END IF;
    END
    \$\$;
    
    -- Grant read-only permissions
    GRANT CONNECT ON DATABASE $POSTGRES_DB TO readonly_user;
    GRANT USAGE ON SCHEMA app TO readonly_user;
    GRANT SELECT ON ALL TABLES IN SCHEMA app TO readonly_user;
    ALTER DEFAULT PRIVILEGES FOR ROLE ${POSTGRES_USER} IN SCHEMA app GRANT SELECT ON TABLES TO readonly_user;
EOSQL

echo "🎉 Custom PostgreSQL setup completed successfully!"
echo "📊 Database: $POSTGRES_DB"
echo "👤 Main user: $POSTGRES_USER"
echo "📖 Read-only user: readonly_user (password set from environment)"
echo "🏗️  Schema: app"