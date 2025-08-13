#!/bin/bash

set -e

# === Configurações ===
RESOURCE_GROUP="meajudaai-dev"
ENVIRONMENT_NAME="dev"
BICEP_FILE="infrastructure/main.bicep" # nome do seu arquivo bicep principal
LOCATION="brazilsouth"  # ou o que estiver no seu resource group
PROJECT_DIR="src/Bootstrapper/MeAjudaAi.ApiService" # caminho para seu projeto .NET Aspire

echo "🔐 Fazendo login no Azure (se necessário)..."
az account show > /dev/null 2>&1 || az login

echo "📦 Deploying Bicep template..."
DEPLOYMENT_NAME="sb-deployment-$(date +%s)"

# Deploy the Bicep template first
az deployment group create \
  --name "$DEPLOYMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --template-file "$BICEP_FILE" \
  --parameters environmentName="$ENVIRONMENT_NAME" location="$LOCATION"

# Get the Service Bus namespace and policy names from outputs
NAMESPACE_NAME=$(az deployment group show \
  --name "$DEPLOYMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query "properties.outputs.serviceBusNamespace.value" \
  --output tsv)

MANAGEMENT_POLICY_NAME=$(az deployment group show \
  --name "$DEPLOYMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query "properties.outputs.managementPolicyName.value" \
  --output tsv)

# Get the connection string securely using Azure CLI
OUTPUT_JSON=$(az servicebus namespace authorization-rule keys list \
  --resource-group "$RESOURCE_GROUP" \
  --namespace-name "$NAMESPACE_NAME" \
  --name "$MANAGEMENT_POLICY_NAME" \
  --query "primaryConnectionString" \
  --output tsv)

if [ -z "$OUTPUT_JSON" ]; then
  echo "❌ Erro: não foi possível extrair a ConnectionString do output do Bicep."
  exit 1
fi

echo "✅ ConnectionString obtida com sucesso."
echo "🔗 ConnectionString: $OUTPUT_JSON"

# === Define a variável de ambiente ===
export Messaging__ServiceBus__ConnectionString="$OUTPUT_JSON"

echo "🚀 Rodando aplicação Aspire..."
cd "$PROJECT_DIR"
dotnet run
