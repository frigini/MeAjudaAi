# MeAjudaAi Infrastructure

This folder contains the Azure infrastructure as code (Bicep templates) and CI/CD pipeline configuration for the MeAjudaAi project.

## 🏗️ Infrastructure Components

- **Azure Service Bus Standard**: Message queuing and pub/sub messaging
- **Authorization Rules**: Separate policies for management and application access
- **Resource Groups**: Environment-specific resource organization

## 📁 Structure

```
infrastructure/
├── main.bicep              # Main infrastructure template
├── servicebus.bicep        # Service Bus configuration
├── deploy.sh              # Deployment script
└── README.md              # This file
```

## 🚀 CI/CD Pipeline

### GitHub Actions Workflows

1. **`ci-cd.yml`** - Main deployment pipeline
   - Builds and tests .NET application
   - Validates Bicep templates
   - Deploys to different environments based on branch/manual trigger

2. **`pr-validation.yml`** - Pull request validation
   - Code quality checks
   - Security scanning
   - Infrastructure validation

### 🔧 Setup Instructions

#### 1. Azure Service Principal Setup

Create a service principal for GitHub Actions:

```bash
# Login to Azure
az login

# Create service principal
az ad sp create-for-rbac \
  --name "meajudaai-github-actions" \
  --role "Contributor" \
  --scopes "/subscriptions/YOUR_SUBSCRIPTION_ID" \
  --sdk-auth
```

#### 2. GitHub Secrets Configuration

Add these secrets to your GitHub repository (`Settings > Secrets and variables > Actions`):

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `AZURE_CREDENTIALS` | Service principal JSON output | Azure authentication credentials |

Example `AZURE_CREDENTIALS` format:
```json
{
  "clientId": "your-client-id",
  "clientSecret": "your-client-secret",
  "subscriptionId": "your-subscription-id",
  "tenantId": "your-tenant-id"
}
```

#### 3. GitHub Environments Setup

Create these environments in GitHub (`Settings > Environments`):

- **development** - Auto-deploys from `develop` branch
- **staging** - Auto-deploys from `main` branch  
- **production** - Manual approval required

### 🌍 Environment (Dev-Only Setup)

| Environment | Resource Group | Trigger | Cost Impact |
|-------------|---------------|---------|-------------|
| Development | `meajudaai-dev` | Push to `develop` or manual | ~$10/month |

**Note**: This setup is optimized for local development. You can easily add staging/production environments later when needed.

## 🚀 Usage

### Automatic Deployments

- **Development**: Push to `develop` branch
- **Staging**: Push to `main` branch
- **Production**: Use "Run workflow" button in GitHub Actions

### Manual Deployments

1. **Via GitHub Actions UI**:
   - Go to Actions tab
   - Select "CI/CD Pipeline"
   - Click "Run workflow"
   - Choose environment and options

2. **Local Development**:
   ```bash
   # Make script executable (Linux/Mac)
   chmod +x infrastructure/deploy.sh
   
   # Deploy to development
   ./infrastructure/deploy.sh dev brazilsouth
   
   # Deploy to production with custom resource group
   ./infrastructure/deploy.sh prod brazilsouth meajudaai-prod-custom
   ```

## 💰 Cost Management

### Current Costs (per environment):
- **Service Bus Standard**: ~$9.81 USD/month
- **Resource Group**: Free
- **Total per environment**: ~$10 USD/month

### Cost Optimization Tips:
1. **Delete dev resources** when not in use
2. **Use cleanup workflow** for temporary testing
3. **Monitor usage** with Azure Cost Management
4. **Consider Service Bus Basic** (~$5/month) for development

### Cleanup Commands:
```bash
# Delete entire environment
az group delete --name meajudaai-dev --yes --no-wait

# Or use the GitHub Actions cleanup job
```

## 🔒 Security Best Practices

### ✅ Implemented:
- No connection strings in deployment outputs
- Separate authorization policies for different access levels
- Environment-specific resource groups
- Azure RBAC for service principal

### 🔧 Secrets Management:
- Connection strings retrieved at runtime
- Use Azure Key Vault for production secrets (future enhancement)
- No hardcoded values in templates

## 🛠️ Local Development

For local testing without deploying infrastructure:

```bash
# Option 1: Deploy temporarily
./infrastructure/deploy.sh dev brazilsouth
# ... do your testing ...
az group delete --name meajudaai-dev --yes

# Option 2: Use local alternatives
# - Azurite for Azure Storage emulation
# - Local RabbitMQ for message queuing
# - In-memory implementations for testing
```

## 📊 Monitoring

### Available Outputs:
- Service Bus namespace name
- Management policy name  
- Application policy name
- Service Bus endpoint URL
- Resource group name
- Deployment name

### Connection Strings:
Retrieved securely via Azure CLI after deployment:
```bash
az servicebus namespace authorization-rule keys list \
  --resource-group meajudaai-dev \
  --namespace-name sb-MeAjudaAi-dev \
  --name ManagementPolicy \
  --query "primaryConnectionString"
```

## 🆘 Troubleshooting

### Common Issues:

1. **"Resource group not found"**
   - Solution: Pipeline creates resource groups automatically

2. **"Bicep validation failed"**
   - Check syntax: `az bicep build --file infrastructure/main.bicep`
   - Validate parameters match template requirements

3. **"Azure credentials expired"**
   - Regenerate service principal credentials
   - Update `AZURE_CREDENTIALS` secret

4. **"Deployment timeout"**
   - Service Bus creation can take 5-10 minutes
   - Check Azure portal for deployment status

### Debug Commands:
```bash
# Check current Azure context
az account show

# List resource groups
az group list --output table

# Check deployment status
az deployment group list --resource-group meajudaai-dev --output table
```

## 🔄 Pipeline Status

Check pipeline status at: `https://github.com/YOUR-USERNAME/MeAjudaAi/actions`

## 📚 Additional Resources

- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure Service Bus Pricing](https://azure.microsoft.com/en-us/pricing/details/service-bus/)
