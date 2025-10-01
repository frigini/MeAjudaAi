#!/bin/bash

# Script to create Docker secrets for MeAjudaAi production deployment
# Usage: ./setup-secrets.sh

set -e

echo "Setting up Docker secrets for MeAjudaAi production..."

# Check if Docker Swarm is initialized
if ! docker info | grep -q "Swarm: active"; then
    echo "Docker Swarm is not active. Initializing Docker Swarm..."
    docker swarm init
fi

# Create Redis password secret
echo "Creating Redis password secret..."
read -s -p "Enter Redis password: " REDIS_PASSWORD
echo
echo "$REDIS_PASSWORD" | docker secret create meajudaai_redis_password -

echo "✅ Docker secrets created successfully!"
echo ""
echo "You can now run the production stack with:"
echo "docker compose -f infrastructure/compose/environments/production.yml --env-file .env.prod up -d"
echo ""
echo "To remove secrets later:"
echo "docker secret rm meajudaai_redis_password"