#!/bin/bash
# Stop all MeAjudaAi containers and clean up

echo "Stopping all MeAjudaAi containers..."

# Navigate to the compose directory
cd "$(dirname "$0")/../compose"

# Stop all possible configurations
echo "Stopping development environment..."
docker compose -f environments/development.yml down

echo "Stopping testing environment..."
docker compose -f environments/testing.yml down 2>/dev/null

echo "Stopping production environment..."
docker compose -f environments/production.yml down 2>/dev/null

echo "Stopping standalone services..."
docker compose -f standalone/keycloak-only.yml down 2>/dev/null
docker compose -f standalone/postgres-only.yml down 2>/dev/null

# Stop any containers with meajudaai prefix
echo "Stopping any remaining MeAjudaAi containers..."
docker ps -a --format "table {{.Names}}" | grep "meajudaai" | xargs -r docker stop

echo "All MeAjudaAi services stopped!"
echo ""
echo "To remove volumes (DANGER - this will delete all data):"
echo "docker volume ls | grep meajudaai | awk '{print \$2}' | xargs docker volume rm"