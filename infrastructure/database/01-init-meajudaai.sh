#!/bin/bash
# PostgreSQL Database Initialization Script
# Executed automatically by PostgreSQL docker-entrypoint-initdb.d on first container start
#
# IMPORTANT: This script runs BEFORE the application starts.
# - Module roles/permissions are managed at runtime by SchemaPermissionsManager (not here)
# - Database tables are created by EF Core migrations (not here)
# - Seeds require tables to exist, so they must be run manually AFTER migrations
#
# This script is kept minimal for Docker Compose fallback scenarios.
# For production, use Aspire which handles everything via runtime services.

set -e

echo "🗄️ MeAjudaAi Database Container Initialized"
echo ""
echo "ℹ️  Next steps:"
echo "   1. Start the application to run EF Core migrations"
echo "   2. Schema isolation (roles/permissions) is configured automatically at runtime"
echo "   3. To seed data manually after migrations:"
echo "      psql -U \$POSTGRES_USER -d \$POSTGRES_DB -f /docker-entrypoint-initdb.d/seeds/01-seed-service-catalogs.sql"
echo ""
echo "✅ Database container ready!"
