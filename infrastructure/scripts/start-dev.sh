#!/bin/bash
# Start complete development environment
# This script starts all services needed for development

echo "Starting MeAjudaAi Development Environment..."

# Navigate to the compose directory
cd "$(dirname "$0")/../compose"

# Start the development environment
docker compose -f environments/development.yml up -d

echo "Development environment started!"
echo ""
echo "Services available at:"
echo "- Keycloak Admin: http://localhost:8080 (admin/admin)"
echo "- PgAdmin: http://localhost:8081 (admin@meajudaai.com/admin)"
echo "- RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "- PostgreSQL: localhost:5432 (postgres/dev123)"
echo "- Redis: localhost:6379"
echo ""
echo "To stop: docker compose -f environments/development.yml down"