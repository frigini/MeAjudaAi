#!/bin/bash
# Test Database Initialization Scripts
# This script validates that all module database scripts execute successfully

set -e

POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-development123}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-meajudaai}"

echo "🧪 Testing Database Initialization Scripts"
echo ""

# Check if Docker is running
if ! docker ps >/dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker."
    exit 1
fi

# Export environment variables
export POSTGRES_PASSWORD
export POSTGRES_USER
export POSTGRES_DB

# Navigate to infrastructure/compose directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_DIR="$SCRIPT_DIR/compose/base"

if [ ! -d "$COMPOSE_DIR" ]; then
    echo "❌ Compose directory not found: $COMPOSE_DIR"
    exit 1
fi

cd "$COMPOSE_DIR"

echo "🐳 Starting PostgreSQL container with initialization scripts..."
echo ""

# Stop and remove existing container
docker compose -f postgres.yml down -v 2>/dev/null || true

# Start container
docker compose -f postgres.yml up -d

# Wait for PostgreSQL to be ready
echo "⏳ Waiting for PostgreSQL to be ready..."
MAX_ATTEMPTS=30
ATTEMPT=0
READY=false

while [ $ATTEMPT -lt $MAX_ATTEMPTS ] && [ "$READY" != "true" ]; do
    ATTEMPT=$((ATTEMPT + 1))
    sleep 2
    
    HEALTH_STATUS=$(docker inspect --format='{{.State.Health.Status}}' meajudaai-postgres 2>/dev/null || echo "unknown")
    if [ "$HEALTH_STATUS" = "healthy" ]; then
        READY=true
        echo "✅ PostgreSQL is ready!"
    else
        echo "   Attempt $ATTEMPT/$MAX_ATTEMPTS - Status: $HEALTH_STATUS"
    fi
done

if [ "$READY" != "true" ]; then
    echo "❌ PostgreSQL failed to start within timeout period"
    docker logs meajudaai-postgres
    exit 1
fi

echo ""
echo "🔍 Verifying database schemas..."

# Test schemas
SCHEMAS=("users" "providers" "documents" "search" "location" "hangfire" "meajudaai_app")

for schema in "${SCHEMAS[@]}"; do
    QUERY="SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = '$schema');"
    RESULT=$(docker exec meajudaai-postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -t -c "$QUERY" | tr -d '[:space:]')
    
    if [ "$RESULT" = "t" ]; then
        echo "   ✅ Schema '$schema' created successfully"
    else
        echo "   ❌ Schema '$schema' NOT found"
    fi
done

echo ""
echo "🔍 Verifying database roles..."

# Test roles
ROLES=(
    "users_role" "users_owner"
    "providers_role" "providers_owner"
    "documents_role" "documents_owner"
    "search_role" "search_owner"
    "location_role" "location_owner"
    "hangfire_role"
    "meajudaai_app_role" "meajudaai_app_owner"
)

for role in "${ROLES[@]}"; do
    QUERY="SELECT EXISTS(SELECT 1 FROM pg_roles WHERE rolname = '$role');"
    RESULT=$(docker exec meajudaai-postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -t -c "$QUERY" | tr -d '[:space:]')
    
    if [ "$RESULT" = "t" ]; then
        echo "   ✅ Role '$role' created successfully"
    else
        echo "   ❌ Role '$role' NOT found"
    fi
done

echo ""
echo "🔍 Verifying PostGIS extension..."

QUERY="SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'postgis');"
RESULT=$(docker exec meajudaai-postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -t -c "$QUERY" | tr -d '[:space:]')

if [ "$RESULT" = "t" ]; then
    echo "   ✅ PostGIS extension enabled"
else
    echo "   ❌ PostGIS extension NOT enabled"
fi

echo ""
echo "📊 Database initialization logs:"
echo ""
docker logs meajudaai-postgres 2>&1 | grep -E "Initializing|Setting up|completed" || true

echo ""
echo "✅ Database validation completed!"
echo ""
echo "💡 To connect to the database:"
echo "   docker exec -it meajudaai-postgres psql -U $POSTGRES_USER -d $POSTGRES_DB"
echo ""
echo "💡 To stop the container:"
echo "   docker compose -f $COMPOSE_DIR/postgres.yml down"
echo ""
