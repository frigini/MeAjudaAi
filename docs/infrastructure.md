# Guia de Infraestrutura - MeAjudaAi

Este documento fornece um guia completo para configurar, executar e fazer deploy da infraestrutura do MeAjudaAi.

## 🏗️ Estratégia de Infraestrutura

### **Principal: Orquestração .NET Aspire**
- **Desenvolvimento**: Containers locais orquestrados pelo Aspire
- **Produção**: Azure Container Apps com serviços gerenciados
- **Configuração**: Recursos condicionais baseados no ambiente

### **Alternativa: Docker Compose**
- Mantido para testes manuais e deployment alternativo
- Estrutura modular com configurações específicas por ambiente

## 📁 Estrutura da Infraestrutura

```text
infrastructure/
├── compose/                    # Docker Compose (alternativo)
│   ├── base/                   # Definições de serviços base
│   ├── environments/           # Configurações por ambiente
│   └── standalone/             # Testes de serviços individuais
├── keycloak/                   # Configuração de autenticação
│   └── realms/                 # Configurações de realm do Keycloak
├── database/                   # Gerenciamento de esquemas de banco
│   └── schemas/                # Scripts SQL para setup de schemas
├── main.bicep                  # Template de infraestrutura Azure
└── deploy.sh                   # Script de deployment Azure
```

## 🚀 Configuração para Desenvolvimento

### .NET Aspire (Recomendado)

```bash
cd src/Aspire/MeAjudaAi.AppHost
dotnet run
```

**Fornece:**
- PostgreSQL com setup automático de schemas
- Keycloak com importação automática de realm
- Redis para cache
- RabbitMQ para messaging
- Dashboard Aspire para monitoramento

**URLs de Acesso:**
- **Aspire Dashboard**: https://localhost:15888
- **API Service**: https://localhost:7032
- **Keycloak Admin**: http://localhost:8080 (admin/admin)
- **PostgreSQL**: localhost:5432 (postgres/dev123)
- **Redis**: localhost:6379
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Docker Compose (Alternativo)

```bash
cd infrastructure/compose

# Ambiente completo de desenvolvimento
docker compose -f environments/development.yml up -d

# Serviços individuais
docker compose -f standalone/keycloak-only.yml up -d
docker compose -f standalone/postgres-only.yml up -d
docker compose -f standalone/messaging-only.yml up -d
```

#### Composições Disponíveis

**Development** (`environments/development.yml`)
- PostgreSQL + Keycloak + Redis + RabbitMQ
- Configurações otimizadas para desenvolvimento local

**Testing** (`environments/testing.yml`)  
- Versão lightweight para testes automatizados
- Bancos em memória e configurações mínimas

**Standalone** (`standalone/`)
- Serviços individuais para depuração e testes específicos

## 🌐 Deploy em Produção

### Recursos Azure

| Recurso | Tipo | Descrição |
|---------|------|-----------|
| **Container Apps Environment** | Hospedagem | Ambiente para aplicações containerizadas |
| **PostgreSQL Flexible Server** | Banco de Dados | Banco principal com schemas separados |
| **Service Bus Standard** | Messaging | Sistema de messaging para produção |
| **Container Registry** | Registry | Armazenamento de imagens Docker |
| **Key Vault** | Segurança | Gerenciamento de segredos e chaves |
| **Application Insights** | Monitoramento | Telemetria e monitoramento da aplicação |

### Comandos de Deploy

```bash
# Autenticar no Azure
azd auth login

# Deploy completo (infraestrutura + aplicação)
azd up

# Deploy apenas da infraestrutura
azd provision

# Deploy apenas da aplicação  
azd deploy

# Verificar status dos recursos
azd show

# Limpar recursos (cuidado!)
azd down
```

### Configuração de Ambientes

#### Desenvolvimento Local

```bash
# Variáveis de ambiente para desenvolvimento
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Host=localhost;Database=meajudaai_dev;Username=postgres;Password=dev123"
export Keycloak__Authority="http://localhost:8080/realms/meajudaai"
```

#### Produção Azure

```bash
# Configuração automática via azd
# Secrets gerenciados pelo Key Vault
# Connection strings injetadas via Container Apps
```

## 🗄️ Configuração de Banco de Dados

### Estratégia de Schemas

Cada módulo possui seu próprio schema PostgreSQL com roles dedicadas:

```sql
-- Schema e role para módulo Users
CREATE SCHEMA IF NOT EXISTS users;
CREATE ROLE users_role;
GRANT USAGE ON SCHEMA users TO users_role;
GRANT ALL ON ALL TABLES IN SCHEMA users TO users_role;

-- Schema e role para módulo Services (futuro)
CREATE SCHEMA IF NOT EXISTS services;
CREATE ROLE services_role;
```yaml
### Migrations

```bash
# Gerar migration para módulo Users
dotnet ef migrations add InitialUsers --context UsersDbContext

# Aplicar migrations
dotnet ef database update --context UsersDbContext

# Remover última migration
dotnet ef migrations remove --context UsersDbContext
```sql
## 🔐 Configuração do Keycloak

### Realm MeAjudaAi

O arquivo `infrastructure/keycloak/realms/meajudaai-realm.dev.json` (dev) ou `meajudaai-realm.prod.json` (produção) contém:

#### Clients Configurados
- **meajudaai-api**: Cliente backend com client credentials
- **meajudaai-web**: Cliente frontend (público)

#### Roles Definidas
- **customer**: Usuários regulares
- **service-provider**: Prestadores de serviço
- **admin**: Administradores
- **super-admin**: Super administradores

#### Usuários de Teste (Desenvolvimento Local)

> ⚠️ **AVISO DE SEGURANÇA**: As credenciais abaixo são EXCLUSIVAMENTE para desenvolvimento local. NUNCA utilize essas credenciais em produção.

- **admin** / admin123 (admin, super-admin) - **DEV ONLY**
- **customer1** / customer123 (customer) - **DEV ONLY**
- **provider1** / provider123 (service-provider) - **DEV ONLY**

**Apenas para desenvolvimento local. Altere imediatamente em ambientes compartilhados/produção.**

### Configuração de Cliente API

```json
{
  "clientId": "meajudaai-api",
  "secret": "your-client-secret-here",
  "serviceAccountsEnabled": true,
  "standardFlowEnabled": true,
  "directAccessGrantsEnabled": true
}
```

## 📨 Sistema de Messaging

### Estratégia por Ambiente

#### Desenvolvimento/Testes: RabbitMQ
```csharp
// Configuração automática via Aspire
builder.AddRabbitMQ("messaging");
```

### Factory Pattern

```csharp
public class MessageBusFactory : IMessageBusFactory
{
    public IMessageBus CreateMessageBus()
    {
        return _serviceProvider.GetRequiredService<RabbitMqMessageBus>();
    }
}
```

## 🔧 Scripts de Utilitários

### Setup Completo

```bash
# Setup completo do ambiente de desenvolvimento
./scripts/setup-dev.sh

# Setup apenas para CI
./setup-ci-only.ps1

# Setup com deploy Azure
./setup-cicd.ps1
```

### Backup e Restore

```bash
# Backup do banco de desenvolvimento
docker exec postgres-dev pg_dump -U postgres meajudaai_dev > backup.sql

# Restore
docker exec -i postgres-dev psql -U postgres -d meajudaai_dev < backup.sql
```bash
### Logs e Monitoramento

```bash
# Logs do Aspire
dotnet run --project src/Aspire/MeAjudaAi.AppHost

# Logs Docker Compose
docker compose -f infrastructure/compose/environments/development.yml logs -f

# Logs Azure Container Apps
az containerapp logs show --name meajudaai-api --resource-group rg-meajudaai
```text
## 🚨 Troubleshooting

### Problemas Comuns

#### 1. Keycloak não inicia
```bash
# Verificar se a porta 8080 está livre
netstat -an | grep 8080

# Restart do container
docker compose restart keycloak
```bash
#### 2. PostgreSQL connection refused
```bash
# Verificar status do container
docker ps | grep postgres

# Verificar logs
docker logs postgres-dev
```text
#### 3. Aspire não conecta aos serviços
```bash
# Limpar containers anteriores
docker system prune -f

# Restart do Aspire
dotnet run --project src/Aspire/MeAjudaAi.AppHost
```bash
### Verificação de Saúde

```bash
# Health checks via API
curl https://localhost:7032/health

# Status dos containers
docker compose ps

# Status dos recursos Azure
azd show
```text
## 📋 Checklist de Deploy

### Desenvolvimento
- [ ] .NET 10 SDK instalado
- [ ] Docker Desktop executando
- [ ] Ports 5432, 6379, 8080, 15672 livres
- [ ] Aspire Dashboard acessível

### Produção
- [ ] Azure CLI instalado e autenticado
- [ ] Subscription Azure ativa
- [ ] Resource Group criado
- [ ] Bicep templates validados
- [ ] Secrets configurados no Key Vault

---

📞 **Suporte**: Para problemas específicos, abra uma issue no [repositório do projeto](https://github.com/frigini/MeAjudaAi/issues).