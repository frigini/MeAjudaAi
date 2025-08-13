#!/bin/bash

# Infrastructure Deployment Script for GitHub Actions
# Usage: ./deploy.sh <environment> <location> [resource-group-name]

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check parameters
if [ $# -lt 2 ]; then
    print_error "Usage: $0 <environment> <location> [resource-group-name]"
    print_error "Example: $0 dev brazilsouth"
    print_error "Example: $0 prod brazilsouth meajudaai-prod-rg"
    exit 1
fi

ENVIRONMENT=$1
LOCATION=$2
RESOURCE_GROUP=${3:-"meajudaai-${ENVIRONMENT}"}

# Validate environment
case $ENVIRONMENT in
    dev|staging|prod)
        print_status "Deploying to environment: $ENVIRONMENT"
        ;;
    *)
        print_error "Invalid environment: $ENVIRONMENT. Must be dev, staging, or prod"
        exit 1
        ;;
esac

print_status "=== Azure Infrastructure Deployment ==="
print_status "Environment: $ENVIRONMENT"
print_status "Location: $LOCATION"
print_status "Resource Group: $RESOURCE_GROUP"
print_status "==========================================="

# Check if logged in to Azure
print_status "Checking Azure authentication..."
if ! az account show > /dev/null 2>&1; then
    print_error "Not logged in to Azure. Please run 'az login' first."
    exit 1
fi

SUBSCRIPTION_NAME=$(az account show --query "name" -o tsv)
print_success "Authenticated to Azure subscription: $SUBSCRIPTION_NAME"

# Create resource group if it doesn't exist
print_status "Creating resource group if it doesn't exist..."
if az group show --name "$RESOURCE_GROUP" > /dev/null 2>&1; then
    print_success "Resource group '$RESOURCE_GROUP' already exists"
else
    print_status "Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    print_success "Resource group created successfully"
fi

# Generate deployment name with timestamp
DEPLOYMENT_NAME="meajudaai-${ENVIRONMENT}-$(date +%s)"
print_status "Deployment name: $DEPLOYMENT_NAME"

# Validate Bicep template
print_status "Validating Bicep template..."
if az deployment group validate \
    --resource-group "$RESOURCE_GROUP" \
    --template-file infrastructure/main.bicep \
    --parameters environmentName="$ENVIRONMENT" location="$LOCATION" > /dev/null; then
    print_success "Bicep template validation passed"
else
    print_error "Bicep template validation failed"
    exit 1
fi

# Deploy infrastructure
print_status "Deploying infrastructure..."
print_status "This may take several minutes..."

DEPLOYMENT_OUTPUT=$(az deployment group create \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file infrastructure/main.bicep \
    --parameters environmentName="$ENVIRONMENT" location="$LOCATION" \
    --output json)

if [ $? -eq 0 ]; then
    print_success "Infrastructure deployment completed successfully"
else
    print_error "Infrastructure deployment failed"
    exit 1
fi

# Extract and display outputs
print_status "Extracting deployment outputs..."
SERVICE_BUS_NAMESPACE=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.serviceBusNamespace.value // empty')
MANAGEMENT_POLICY_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.managementPolicyName.value // empty')
APPLICATION_POLICY_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.applicationPolicyName.value // empty')
SERVICE_BUS_ENDPOINT=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.serviceBusEndpoint.value // empty')

print_success "=== Deployment Outputs ==="
echo "Service Bus Namespace: $SERVICE_BUS_NAMESPACE"
echo "Management Policy: $MANAGEMENT_POLICY_NAME"
echo "Application Policy: $APPLICATION_POLICY_NAME"
echo "Service Bus Endpoint: $SERVICE_BUS_ENDPOINT"
print_success "=========================="

# Get connection strings securely (for local use)
if [ "$ENVIRONMENT" = "dev" ]; then
    print_status "Retrieving connection strings for development..."
    
    MANAGEMENT_CONNECTION_STRING=$(az servicebus namespace authorization-rule keys list \
        --resource-group "$RESOURCE_GROUP" \
        --namespace-name "$SERVICE_BUS_NAMESPACE" \
        --name "$MANAGEMENT_POLICY_NAME" \
        --query "primaryConnectionString" \
        --output tsv)
    
    print_success "Development connection string available (use 'export' commands below)"
    echo ""
    echo "# Add these to your environment variables:"
    echo "export Messaging__ServiceBus__ConnectionString=\"$MANAGEMENT_CONNECTION_STRING\""
    echo "export AZURE_SERVICE_BUS_NAMESPACE=\"$SERVICE_BUS_NAMESPACE\""
fi

# Save outputs to file for GitHub Actions
if [ -n "$GITHUB_ACTIONS" ]; then
    echo "serviceBusNamespace=$SERVICE_BUS_NAMESPACE" >> $GITHUB_OUTPUT
    echo "managementPolicyName=$MANAGEMENT_POLICY_NAME" >> $GITHUB_OUTPUT  
    echo "applicationPolicyName=$APPLICATION_POLICY_NAME" >> $GITHUB_OUTPUT
    echo "serviceBusEndpoint=$SERVICE_BUS_ENDPOINT" >> $GITHUB_OUTPUT
    echo "resourceGroup=$RESOURCE_GROUP" >> $GITHUB_OUTPUT
    echo "deploymentName=$DEPLOYMENT_NAME" >> $GITHUB_OUTPUT
fi

print_success "Deployment completed successfully! 🎉"
