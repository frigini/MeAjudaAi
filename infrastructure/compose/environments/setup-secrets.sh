#!/bin/bash
set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "🔐 Setting up Docker secrets for MeAjudaAi production deployment..."

# Function to validate non-empty input
validate_input() {
    local input="$1"
    local field_name="$2"
    
    if [[ -z "$input" ]]; then
        echo -e "${RED}❌ Error: $field_name cannot be empty!${NC}" >&2
        return 1
    fi
    return 0
}

# Function to check if secret exists
secret_exists() {
    local secret_name="$1"
    docker secret ls --format "{{.Name}}" | grep -q "^${secret_name}$"
}

# Function to handle existing secret
handle_existing_secret() {
    local secret_name="$1"
    
    echo -e "${YELLOW}⚠️  Secret '$secret_name' already exists.${NC}"
    echo "What would you like to do?"
    echo "1) Skip creation (keep existing secret)"
    echo "2) Remove and recreate"
    echo "3) Exit script"
    
    while true; do
        read -p "Choose an option (1-3): " choice
        case $choice in
            1)
                echo -e "${GREEN}✅ Keeping existing secret '$secret_name'${NC}"
                return 1  # Skip creation
                ;;
            2)
                echo -e "${YELLOW}🗑️  Removing existing secret '$secret_name'...${NC}"
                docker secret rm "$secret_name"
                echo -e "${GREEN}✅ Secret removed successfully${NC}"
                return 0  # Proceed with creation
                ;;
            3)
                echo -e "${RED}❌ Exiting script...${NC}"
                exit 0
                ;;
            *)
                echo -e "${RED}❌ Invalid choice. Please enter 1, 2, or 3.${NC}"
                ;;
        esac
    done
}

# Function to create secret with validation
create_secret_with_validation() {
    local secret_name="$1"
    local prompt_message="$2"
    local password
    
    # Check if secret already exists
    if secret_exists "$secret_name"; then
        if ! handle_existing_secret "$secret_name"; then
            return 0  # Skip creation
        fi
    fi
    
    # Prompt for password with validation
    while true; do
        read -s -p "$prompt_message" password
        echo  # New line after hidden input
        
        if validate_input "$password" "password"; then
            break
        else
            echo -e "${RED}❌ Password cannot be empty. Please try again.${NC}"
        fi
    done
    
    # Create the secret
    echo -n "$password" | docker secret create "$secret_name" -
    echo -e "${GREEN}✅ Secret '$secret_name' created successfully${NC}"
}

# Check if Docker Swarm is initialized
if ! docker info | grep -q "Swarm: active"; then
    echo -e "${YELLOW}⚠️  Docker Swarm is not active. Initializing Docker Swarm...${NC}"
    docker swarm init
    echo -e "${GREEN}✅ Docker Swarm initialized${NC}"
fi

echo "📝 Please provide the following credentials for production deployment:"
echo

# Create all required secrets based on production.yml
create_secret_with_validation "meajudaai_redis_password" "🔑 Enter Redis password: "

echo
echo -e "${GREEN}🎉 All secrets created successfully!${NC}"
echo
echo "📋 Created secrets:"
echo "  - meajudaai_redis_password"
echo
echo -e "${GREEN}✅ You can now run the production stack with:${NC}"
echo "  docker compose -f infrastructure/compose/environments/production.yml --env-file .env.prod up -d"
echo
echo "🔍 To verify secrets were created:"
echo "  docker secret ls"
echo
echo "🗑️  To remove secrets later:"
echo "  docker secret rm meajudaai_redis_password"