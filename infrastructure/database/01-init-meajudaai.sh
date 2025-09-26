#!/bin/bash
# PostgreSQL Database Initialization Script
# This script sets up the MeAjudaAi database with modular schema structure
# Executed automatically by PostgreSQL docker-entrypoint-initdb.d

set -e

echo "🗄️ Initializing MeAjudaAi Database..."

# Function to execute SQL files
execute_sql() {
    local file="$1"
    echo "   Executing: $(basename "$file")"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f "$file"
}

# Execute Users module scripts
echo "📁 Setting up Users module..."
if [ -f "/docker-entrypoint-initdb.d/modules/users/00-roles.sql" ]; then
    execute_sql "/docker-entrypoint-initdb.d/modules/users/00-roles.sql"
fi
if [ -f "/docker-entrypoint-initdb.d/modules/users/01-permissions.sql" ]; then
    execute_sql "/docker-entrypoint-initdb.d/modules/users/01-permissions.sql"
fi

# Execute Providers module scripts
echo "📁 Setting up Providers module..."
if [ -f "/docker-entrypoint-initdb.d/modules/providers/00-roles.sql" ]; then
    execute_sql "/docker-entrypoint-initdb.d/modules/providers/00-roles.sql"
fi
if [ -f "/docker-entrypoint-initdb.d/modules/providers/01-permissions.sql" ]; then
    execute_sql "/docker-entrypoint-initdb.d/modules/providers/01-permissions.sql"
fi

# Execute cross-module views
echo "🔗 Setting up cross-module views..."
if [ -f "/docker-entrypoint-initdb.d/views/cross-module-views.sql" ]; then
    execute_sql "/docker-entrypoint-initdb.d/views/cross-module-views.sql"
fi

echo "✅ MeAjudaAi Database initialization completed!"