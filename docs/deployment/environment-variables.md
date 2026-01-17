# Environment Variables - Deployment Guide

## Overview

Todas as configurações do MeAjudaAi Admin Portal podem ser sobrescritas via variáveis de ambiente em produção. Este guia documenta como configurar corretamente cada ambiente de deployment.

## Hierarquia de Configuração

A configuração é carregada na seguinte ordem de prioridade (última sobrescreve anterior):

1. **appsettings.json** - Valores padrão
2. **appsettings.{Environment}.json** - Valores por ambiente (Development, Staging, Production)
3. **Azure Key Vault** - Secrets sensíveis (connection strings, API keys)
4. **Environment Variables** ⭐ - **Maior prioridade**, sobrescreve tudo

## Formato de Variáveis

### Convenção .NET

Use `__` (dois underscores) para representar níveis aninhados em JSON:

```json
// appsettings.json
{
  "Keycloak": {
    "Authority": "https://keycloak.example.com"
  }
}
```

```bash
# Environment Variable equivalente
Keycloak__Authority=https://keycloak.example.com
```

### Exemplos de Conversão

| JSON Path | Environment Variable |
|-----------|---------------------|
| `ApiBaseUrl` | `ApiBaseUrl` |
| `Keycloak.Authority` | `Keycloak__Authority` |
| `Keycloak.ClientId` | `Keycloak__ClientId` |
| `Features.EnableReduxDevTools` | `Features__EnableReduxDevTools` |

## Variáveis Críticas (Obrigatórias)

Estas variáveis **DEVEM** ser configuradas em produção:

### API Base URL
```bash
ApiBaseUrl=https://api.meajudaai.com
```
- **Descrição**: URL base da API backend
- **Exemplo Dev**: `https://localhost:7001`
- **Exemplo Prod**: `https://api.meajudaai.com`
- **Validação**: Deve ser URL absoluta válida

### Keycloak Authority
```bash
Keycloak__Authority=https://auth.meajudaai.com/realms/meajudaai
```
- **Descrição**: URL do Keycloak realm para autenticação OIDC
- **Exemplo Dev**: `https://localhost:8443/realms/meajudaai`
- **Exemplo Prod**: `https://auth.meajudaai.com/realms/meajudaai`
- **Validação**: Deve terminar com `/realms/{realm-name}`

### Keycloak Client ID
```bash
Keycloak__ClientId=admin-portal
```
- **Descrição**: ID do cliente OIDC configurado no Keycloak
- **Padrão**: `meajudaai-admin`
- **Prod**: Criar cliente específico por ambiente

### Post Logout Redirect URI
```bash
Keycloak__PostLogoutRedirectUri=https://admin.meajudaai.com
```
- **Descrição**: URL para redirect após logout
- **Deve corresponder**: URL configurada no Keycloak client

## Variáveis Opcionais

### Keycloak Scope
```bash
Keycloak__Scope="openid profile email roles"
```
- **Padrão**: `openid profile email`
- **Prod**: Adicionar `roles` para autorização

### Feature Flags

#### Redux DevTools
```bash
Features__EnableReduxDevTools=false
```
- **Padrão**: `true` (development)
- **Prod**: **SEMPRE `false`** (expõe state interno)

## Configuração por Ambiente

### 🐳 Docker / Docker Compose

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  admin-portal:
    image: meajudaai/admin-portal:latest
    environment:
      - ApiBaseUrl=https://api.staging.meajudaai.com
      - Keycloak__Authority=https://auth.staging.meajudaai.com/realms/meajudaai
      - Keycloak__ClientId=admin-portal-staging
      - Keycloak__PostLogoutRedirectUri=https://admin-staging.meajudaai.com
      - Features__EnableReduxDevTools=false
    ports:
      - "8080:80"
```

**Dockerfile (build-time):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Runtime environment variables
ENV ApiBaseUrl=https://api.meajudaai.com \
    Keycloak__Authority=https://auth.meajudaai.com/realms/meajudaai \
    Keycloak__ClientId=admin-portal \
    Keycloak__PostLogoutRedirectUri=https://admin.meajudaai.com \
    Features__EnableReduxDevTools=false
```

**docker run (CLI):**
```bash
docker run -d \
  -e ApiBaseUrl=https://api.prod.com \
  -e Keycloak__Authority=https://auth.prod.com/realms/prod \
  -e Keycloak__ClientId=admin \
  -p 8080:80 \
  meajudaai/admin-portal:latest
```

---

### ☁️ Azure App Service

**Portal Azure** → App Service → Configuration → Application Settings:

| Name | Value | Slot Setting |
|------|-------|--------------|
| `ApiBaseUrl` | `https://api-prod.azurewebsites.net` | ✅ |
| `Keycloak__Authority` | `https://keycloak.azure.com/realms/prod` | ✅ |
| `Keycloak__ClientId` | `admin-portal-prod` | ✅ |
| `Keycloak__PostLogoutRedirectUri` | `https://admin-prod.azurewebsites.net` | ✅ |
| `Features__EnableReduxDevTools` | `false` | ❌ |

> ✅ **Slot Setting**: Marcar para variáveis que mudam por ambiente (staging/prod)

**Azure CLI:**
```bash
az webapp config appsettings set \
  --resource-group meajudaai-rg \
  --name admin-portal-prod \
  --settings \
    ApiBaseUrl=https://api-prod.azurewebsites.net \
    Keycloak__Authority=https://auth.azure.com/realms/prod \
    Keycloak__ClientId=admin-portal \
    Features__EnableReduxDevTools=false
```

**Bicep/ARM Template:**
```bicep
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: 'admin-portal-prod'
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'ApiBaseUrl'
          value: 'https://api-prod.azurewebsites.net'
        }
        {
          name: 'Keycloak__Authority'
          value: 'https://auth.azure.com/realms/prod'
        }
        {
          name: 'Keycloak__ClientId'
          value: 'admin-portal'
        }
        {
          name: 'Features__EnableReduxDevTools'
          value: 'false'
        }
      ]
    }
  }
}
```

---

### ☸️ Kubernetes

**ConfigMap (dados não-sensíveis):**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: admin-portal-config
  namespace: meajudaai
data:
  ApiBaseUrl: "https://api.meajudaai.com"
  Keycloak__Authority: "https://auth.meajudaai.com/realms/meajudaai"
  Keycloak__PostLogoutRedirectUri: "https://admin.meajudaai.com"
  Features__EnableReduxDevTools: "false"
```

**Secret (dados sensíveis - client ID pode ser secret):**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: admin-portal-secrets
  namespace: meajudaai
type: Opaque
stringData:
  Keycloak__ClientId: "admin-portal-k8s"
```

**Deployment:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: admin-portal
spec:
  template:
    spec:
      containers:
      - name: admin-portal
        image: meajudaai/admin-portal:1.0.0
        envFrom:
        - configMapRef:
            name: admin-portal-config
        - secretRef:
            name: admin-portal-secrets
        # OU individuais:
        env:
        - name: ApiBaseUrl
          valueFrom:
            configMapKeyRef:
              name: admin-portal-config
              key: ApiBaseUrl
        - name: Keycloak__ClientId
          valueFrom:
            secretKeyRef:
              name: admin-portal-secrets
              key: Keycloak__ClientId
```

**Helm Values:**
```yaml
# values.yaml
config:
  apiBaseUrl: "https://api.meajudaai.com"
  keycloak:
    authority: "https://auth.meajudaai.com/realms/meajudaai"
    clientId: "admin-portal"
    postLogoutRedirectUri: "https://admin.meajudaai.com"
  features:
    enableReduxDevTools: false
```

---

### 🖥️ Linux / Systemd

**systemd service file** (`/etc/systemd/system/admin-portal.service`):
```ini
[Unit]
Description=MeAjudaAi Admin Portal
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/meajudaai/admin-portal
ExecStart=/opt/meajudaai/admin-portal/MeAjudaAi.Web.Admin

# Environment variables
Environment=ApiBaseUrl=https://api.meajudaai.com
Environment=Keycloak__Authority=https://auth.meajudaai.com/realms/meajudaai
Environment=Keycloak__ClientId=admin-portal
Environment=Keycloak__PostLogoutRedirectUri=https://admin.meajudaai.com
Environment=Features__EnableReduxDevTools=false

Restart=on-failure

[Install]
WantedBy=multi-user.target
```

**Ou via arquivo de ambiente** (`/etc/meajudaai/admin-portal.env`):
```bash
ApiBaseUrl=https://api.meajudaai.com
Keycloak__Authority=https://auth.meajudaai.com/realms/meajudaai
Keycloak__ClientId=admin-portal
Features__EnableReduxDevTools=false
```

```ini
[Service]
EnvironmentFile=/etc/meajudaai/admin-portal.env
```

---

## Validação de Configuração

### Startup Validation

O aplicativo valida configuração automaticamente no startup:

```csharp
// Program.cs
ValidateConfiguration(clientConfig);
```

**Validações executadas**:
- ✅ ApiBaseUrl não pode ser vazio
- ✅ ApiBaseUrl deve ser URL absoluta válida
- ✅ Keycloak.Authority não pode ser vazio
- ✅ Keycloak.Authority deve ser URL absoluta válida
- ✅ Keycloak.ClientId não pode ser vazio
- ✅ Keycloak.PostLogoutRedirectUri não pode ser vazio

**Comportamento em caso de erro**:
```
❌❌❌ CONFIGURATION VALIDATION FAILED ❌❌❌

❌ ApiBaseUrl is missing
❌ Keycloak Authority is not a valid absolute URI

Please check your backend configuration and ensure all required settings are properly configured.
```
- ❌ Aplicação **não inicia**
- ❌ Lança `InvalidOperationException`
- ✅ Logs detalhados no console

### Testando Configuração

**1. Local (Development):**
```bash
# Linux/Mac
export ApiBaseUrl=https://localhost:7001
export Keycloak__Authority=https://localhost:8443/realms/test
dotnet run

# Windows PowerShell
$env:ApiBaseUrl="https://localhost:7001"
$env:Keycloak__Authority="https://localhost:8443/realms/test"
dotnet run
```

**2. Docker:**
```bash
docker run --rm \
  -e ApiBaseUrl=https://api.test.com \
  -e Keycloak__Authority=https://auth.test.com/realms/test \
  meajudaai/admin-portal:latest
  
# Verificar logs
docker logs <container-id>
```

**3. Kubernetes:**
```bash
kubectl logs deployment/admin-portal -n meajudaai | grep "Configuration"

# Deve aparecer:
# ✅ Configuration loaded successfully
# ✅ Configuration validation passed
```

---

## Segurança e Boas Práticas

### ❌ NÃO faça:
```bash
# NÃO commitar secrets no código
ApiBaseUrl=https://api.com # ❌ hardcoded

# NÃO usar plain text para secrets em repos
Keycloak__ClientSecret=super-secret-123 # ❌ em git

# NÃO deixar ReduxDevTools em produção
Features__EnableReduxDevTools=true # ❌ em prod
```

### ✅ FAÇA:
```bash
# ✅ Usar Azure Key Vault para secrets
@Microsoft.KeyVault(SecretUri=https://vault.azure.net/secrets/ClientId)

# ✅ Usar variáveis de ambiente no CI/CD
${{ secrets.API_BASE_URL }}  # GitHub Actions
${API_BASE_URL}              # Azure DevOps

# ✅ Diferentes valores por ambiente
# dev:     https://localhost:7001
# staging: https://api-staging.azure.com
# prod:    https://api.meajudaai.com

# ✅ Validar em pipelines
az webapp config appsettings list --name admin-portal-prod | jq
```

### Rotação de Secrets

**Keycloak Client Secret** (se usar confidential client):
1. Criar novo secret no Keycloak
2. Atualizar variável de ambiente `Keycloak__ClientSecret`
3. Restart aplicação
4. Revogar old secret após validação

---

## Troubleshooting

### Erro: "Failed to fetch configuration from backend"

**Causa**: `ApiBaseUrl` incorreta ou API offline

**Solução**:
```bash
# Verificar conectividade
curl https://api.meajudaai.com/api/configuration/client

# Verificar variável
echo $ApiBaseUrl  # Linux/Mac
echo $env:ApiBaseUrl  # Windows PowerShell
```

### Erro: "Keycloak Authority is not a valid absolute URI"

**Causa**: Formato incorreto ou faltando `/realms/{realm}`

**Correto**:
```bash
Keycloak__Authority=https://auth.com/realms/meajudaai
```

**Incorreto**:
```bash
Keycloak__Authority=https://auth.com  # ❌ falta /realms/...
Keycloak__Authority=auth.com/realms/meajudaai  # ❌ falta https://
```

### Variável não está sendo aplicada

**Debugging**:
```csharp
// Adicionar temporariamente em Program.cs
Console.WriteLine($"ApiBaseUrl from env: {Environment.GetEnvironmentVariable("ApiBaseUrl")}");
Console.WriteLine($"ApiBaseUrl from config: {clientConfig.ApiBaseUrl}");
```

**Causas comuns**:
- ❌ Typo no nome da variável (case-sensitive em Linux)
- ❌ Usando `:` ao invés de `__` (apenas `:` funciona no Windows)
- ❌ Variável não exportada (Linux: `export VAR=value`)
- ❌ App não reiniciado após mudar variável

---

## Referências

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure App Service Configuration](https://learn.microsoft.com/en-us/azure/app-service/configure-common)
- [Kubernetes ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)
- [Docker Environment Variables](https://docs.docker.com/compose/environment-variables/)

## Exemplos Completos por Ambiente

Ver [infrastructure/README.md](../../infrastructure/README.md) para exemplos de configuração completa por ambiente (Azure, Kubernetes, Docker Compose).
