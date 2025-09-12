#!/bin/bash
# Start only Keycloak for development
# Useful when you only need authentication service

echo "Starting Keycloak standalone..."

# Navigate to the compose directory
cd "$(dirname "$0")/../compose"

# Start Keycloak standalone
docker compose -f standalone/keycloak-only.yml up -d

echo "Keycloak started!"
echo ""
echo "Keycloak Admin Console: http://localhost:8080"
echo "Username: admin"
echo "Password: admin"
echo ""
echo "To stop: docker compose -f standalone/keycloak-only.yml down"