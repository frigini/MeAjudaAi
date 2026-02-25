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
    # NOTE: Not using ON_ERROR_STOP for permissions scripts because tables don't exist yet
    # GRANTs on specific tables will fail but ALTER DEFAULT PRIVILEGES will work
    psql --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f "$file" || echo "   ⚠️  Some commands failed (expected for GRANTs on non-existent tables)"
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

# Execute data seeds (essential domain data)
echo "🌱 Seeding essential domain data..."
SEEDS_DIR="/docker-entrypoint-initdb.d/seeds"
if [ -d "${SEEDS_DIR}" ]; then
    # Execute seeds in alphabetical order (numeric prefix controls order)
    for seed_file in "${SEEDS_DIR}"/*.sql; do
        if [ -f "${seed_file}" ]; then
            execute_sql "${seed_file}"
        fi
    done
else
    echo "   ⚠️  No seeds directory found - skipping data seeding"
fi

echo "✅ MeAjudaAi Database initialization completed!"