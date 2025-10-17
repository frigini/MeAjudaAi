# Guia de Configuração por Ambiente

Este guia explica como configurar a aplicação MeAjudaAi para diferentes ambientes usando os templates de configuração fornecidos.

## 📋 Visão Geral

A aplicação suporta configuração específica para dois ambientes principais:
- **Development** - Desenvolvimento local
- **Production** - Ambiente de produção

## 🔧 Templates Disponíveis

### 1. Development (`appsettings.Development.template.json`)
- **Propósito**: Desenvolvimento local e testes
- **Características**:
  - Logging detalhado (Debug level)
  - CORS permissivo para frontend local
  - Keycloak sem HTTPS (desenvolvimento)
  - Rate limiting relaxado
  - Swagger UI habilitado
  - Messaging in-memory

### 2. Production (`appsettings.Production.template.json`)
- **Propósito**: Ambiente de produção
- **Características**:
  - Logging mínimo (Warning level)
  - CORS muito restrito
  - Keycloak com configurações de segurança máximas
  - Rate limiting conservador
  - Swagger UI desabilitado
  - Todos os recursos de segurança habilitados

### 3. Dead Letter Queue Templates

#### Development Dead Letter (`appsettings.Development.deadletter.json`)
- **Propósito**: Configuração de dead letter queue para desenvolvimento
- **Características**:
  - RabbitMQ como provider de messaging
  - Retry policy relaxado (3 tentativas)
  - Logging detalhado habilitado
  - Notificações de admin desabilitadas

#### Production Dead Letter (`appsettings.Production.deadletter.json`)
- **Propósito**: Configuração de dead letter queue para produção
- **Características**:
  - ServiceBus como provider de messaging
  - Retry policy mais agressivo (5 tentativas)
  - Logging detalhado desabilitado
  - Notificações de admin habilitadas
  - TTL estendido (72 horas)

### 4. Authorization Example (`appsettings.authorization.example.json`)
- **Propósito**: Template completo de configuração de autorização
- **Características**:
  - Configurações Keycloak completas
  - Políticas de autorização pré-definidas
  - Claims customizados configurados

## 🚀 Como Usar os Templates

### Passo 1: Copiar o Template
```bash
# Para desenvolvimento
cp docs/configuration-templates/appsettings.Development.template.json src/Bootstrapper/MeAjudaAi.ApiService/appsettings.Development.json

# Para produção
cp docs/configuration-templates/appsettings.Production.template.json src/Bootstrapper/MeAjudaAi.ApiService/appsettings.Production.json
```csharp
### Passo 2: Configurar Variáveis de Ambiente

#### Development
```bash
# Não requer variáveis de ambiente - usa valores padrão
```csharp
#### Production
```bash
export DATABASE_CONNECTION_STRING="Host=prod-db.meajudaai.com;Database=meajudaai_prod;Username=${DB_USER};Password=${DB_PASSWORD};Port=5432;SslMode=Require;"
export REDIS_CONNECTION_STRING="prod-redis.meajudaai.com:6380,ssl=True"
export KEYCLOAK_BASE_URL="https://auth.meajudaai.com"
export KEYCLOAK_CLIENT_ID="meajudaai-prod"
export KEYCLOAK_CLIENT_SECRET="${KEYCLOAK_SECRET}"
export SERVICEBUS_CONNECTION_STRING="${AZURE_SERVICEBUS_CONNECTION}"
export RABBITMQ_HOSTNAME="prod-rabbitmq.meajudaai.com"
export RABBITMQ_USERNAME="${RABBITMQ_USER}"
export RABBITMQ_PASSWORD="${RABBITMQ_PASS}"
```text
## 🔒 Configurações de Segurança por Ambiente

### Development
- **HTTPS**: Opcional
- **CORS**: Permissivo (`*` para origins locais)
- **Rate Limiting**: 60 req/min (anônimo), 200 req/min (autenticado)
- **Logging**: Debug completo
- **Swagger**: Habilitado com documentação completa

### Production
- **HTTPS**: Obrigatório com HSTS
- **CORS**: Muito restrito (apenas domínios oficiais)
- **Rate Limiting**: 20 req/min (anônimo), 60 req/min (autenticado)
- **Logging**: Warning level apenas
- **Swagger**: Desabilitado por segurança

## 📊 Monitoramento e Health Checks

### Development
```json
{
  "HealthChecks": {
    "UI": {
      "Enabled": true,
      "Path": "/health-ui"
    }
  }
}
```csharp
### Production
```json
{
  "HealthChecks": {
    "UI": {
      "Enabled": false,
      "Path": "/health-ui"
    }
  }
}
```text
## 🔧 Configuração Específica por Componente

### 1. Banco de Dados

#### Development
- Host local (localhost)
- Sem SSL
- Timeouts relaxados

#### Production
- Host externo
- SSL obrigatório
- Connection pooling otimizado
- Timeouts configurados para performance

### 2. Messaging (Service Bus / RabbitMQ)

#### Development
- In-memory para testes rápidos
- Sem configuração de cluster

#### Production
- Serviços externos
- Configuração de cluster
- Retry policies
- Dead letter queues

### 3. Cache (Redis)

#### Development
- Redis local sem autenticação
- Configuração básica

#### Production
- Redis externo com SSL
- Autenticação obrigatória
- Configuração de cluster

## 🚀 Deploy por Ambiente

### Docker Compose (Development)
```yaml
version: '3.8'
services:
  api:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./appsettings.Development.json:/app/appsettings.Development.json
```yaml
### Azure Container Apps (Production)
```bash
# Production
az containerapp update \
  --name meajudaai-api \
  --resource-group meajudaai-prod \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production
```bash
## ⚠️ Importantes Considerações de Segurança

### 1. Secrets Management
- **Development**: Secrets no arquivo (apenas para desenvolvimento)
- **Produção**: Azure Key Vault ou similar

### 2. Connection Strings
- **Development**: Secrets no arquivo (apenas para desenvolvimento)
- **Production**: Azure Key Vault ou similar

### 3. API Keys
- Keycloak client secrets devem estar em Key Vault
- Service Bus connection strings protegidas
- Redis passwords em secrets

### 4. CORS
- **Development**: Permissivo para facilitar desenvolvimento
- **Production**: Apenas domínios oficiais e verificados

### 5. Rate Limiting
- **Development**: Relaxado para não atrapalhar desenvolvimento
- **Production**: Conservador para proteger recursos

## 🔍 Troubleshooting

### Problemas Comuns

1. **CORS Errors**
   - Verificar `AllowedOrigins` no ambiente correto
   - Confirmar que o frontend está usando HTTPS em produção

2. **Authentication Issues**
   - Verificar `RequireHttpsMetadata` está correto para o ambiente
   - Confirmar que o Keycloak está acessível

3. **Rate Limiting**
   - Ajustar limites conforme necessário
   - Monitorar logs para identificar padrões

4. **Database Connection**
   - Verificar connection string e variáveis de ambiente
   - Confirmar que SSL está configurado corretamente

### Logs Úteis

```bash
# Ver logs de autenticação
docker logs meajudaai-api | grep "Authentication"

# Ver logs de CORS
docker logs meajudaai-api | grep "CORS"

# Ver logs de Rate Limiting
docker logs meajudaai-api | grep "RateLimit"
```text
## 📞 Suporte

Para dúvidas sobre configuração:
- **Development**: Consulte a documentação técnica
- **Produção**: Entre em contato com a equipe DevOps

---

**Nota**: Sempre teste as configurações em ambiente de desenvolvimento antes de aplicar em produção!