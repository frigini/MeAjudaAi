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

MODULES_DIR="/docker-entrypoint-initdb.d/modules"
if [ -d "${MODULES_DIR}" ]; then
    for module_path in "${MODULES_DIR}"/*; do
        [ -d "${module_path}" ] || continue
        module_name=$(basename "${module_path}")
        echo "📁 Setting up ${module_name} module..."
        for script_name in 00-roles.sql 01-permissions.sql; do
            script_path="${module_path}/${script_name}"
            if [ -f "${script_path}" ]; then
                execute_sql "${script_path}"
            fi
        done
    done
fi

# Execute cross-module views
echo "🔗 Setting up cross-module views..."
if [ -f "/docker-entrypoint-initdb.d/views/cross-module-views.sql" ]; then
    execute_sql "/docker-entrypoint-initdb.d/views/cross-module-views.sql"
fi

echo "✅ MeAjudaAi Database initialization completed!"