# Guia de Configuração e Constantes

Este guia explica como configurar a aplicação MeAjudaAi para diferentes ambientes e como usar o sistema de constantes centralizadas.

## 📋 Visão Geral da Configuração por Ambiente

A aplicação suporta configuração específica para dois ambientes principais:
- **Development** - Desenvolvimento local
- **Production** - Ambiente de produção

### Seções de Configuração Principais

#### 1. DocumentUpload
Configurações para upload de documentos:
```json
"DocumentUpload": {
  "MaxFileSizeBytes": 10485760,  // 10MB
  "AllowedContentTypes": [
    "image/jpeg",
    "image/png",
    "image/jpg",
    "application/pdf"
  ]
}
```

**Personalização por Ambiente:**
- **Development**: Limites maiores para testes (ex: 20MB)
- **Production**: Limites conservadores (10MB) para otimizar custos de storage

#### 2. Caching
- **Propósito**: Desenvolvimento local e testes
- **Características**:
  - Logging detalhado (Debug level)
  - CORS permissivo para frontend local
  - Keycloak sem HTTPS (desenvolvimento)
  - Rate limiting relaxado
  - Swagger UI habilitado
  - Messaging in-memory

#### 2. Production (`appsettings.Production.template.json`)
- **Propósito**: Ambiente de produção
- **Características**:
  - Logging mínimo (Warning level)
  - CORS muito restrito
  - Keycloak com configurações de segurança máximas
  - Rate limiting conservador
  - Swagger UI desabilitado
  - Todos os recursos de segurança habilitados

#### 3. Dead Letter Queue Templates

- **`appsettings.Development.deadletter.json`**: Configuração de dead letter queue para desenvolvimento com RabbitMQ.

#### 4. Authorization Example (`appsettings.authorization.example.json`)
- **Propósito**: Template completo de configuração de autorização.

### Como Usar os Templates

#### Passo 1: Copiar o Template
```bash
# Para desenvolvimento
cp docs/configuration-templates/appsettings.Development.template.json src/Bootstrapper/MeAjudaAi.ApiService/appsettings.Development.json

# Para produção
cp docs/configuration-templates/appsettings.Production.template.json src/Bootstrapper/MeAjudaAi.ApiService/appsettings.Production.json
```

#### Passo 2: Configurar Variáveis de Ambiente (Produção)
```bash
export DATABASE_CONNECTION_STRING="Host=prod-db.meajudaai.com;..."
export REDIS_CONNECTION_STRING="prod-redis.meajudaai.com:6380,ssl=True"
export KEYCLOAK_BASE_URL="https://auth.meajudaai.com"
# ... e outras variáveis de ambiente
```

## 🔧 Constantes Centralizadas

O projeto MeAjudaAi utiliza um sistema de constantes centralizadas para melhor organização e manutenção.

### Estrutura das Constantes

Todas as constantes estão localizadas em `src/Shared/MeAjudai.Shared/Constants/`:

```csharp
Constants/
├── ApiEndpoints.cs          # Endpoints da API por módulo
├── AuthConstants.cs         # Constantes de autorização
├── ValidationConstants.cs   # Limites de validação
└── ValidationMessages.cs    # Mensagens de erro padronizadas
```

### Como Usar

#### 1. ApiEndpoints - Endpoints da API

```csharp
// Em vez de: app.MapGet("/users/{id:guid}", ...)
app.MapGet(ApiEndpoints.Users.GetById, GetUserAsync)
```

#### 2. AuthConstants - Autorização

```csharp
// Em vez de: .RequireAuthorization("AdminOnly")
.RequireAuthorization(AuthConstants.Policies.AdminOnly)
```

#### 3. ValidationConstants - Limites de Validação

```csharp
// Em vez de: [StringLength(50, MinimumLength = 2)]
[StringLength(ValidationConstants.UserLimits.MaxFirstNameLength,
              MinimumLength = ValidationConstants.UserLimits.MinFirstNameLength)]
```

#### 4. ValidationMessages - Mensagens de Erro

```csharp
// Em vez de: .WithMessage("O email é obrigatório")
.WithMessage(ValidationMessages.Required.Email)
```

### Benefícios

- **Consistência**: Evita "números mágicos" e strings duplicadas.
- **Manutenibilidade**: Um único lugar para atualizar valores.
- **Legibilidade**: Código mais limpo e semântico.

## 📝 Diretrizes de Uso

- **Use sempre as constantes** em vez de valores hardcoded.
- **Mantenha as constantes organizadas** por contexto.
- **Documente novas constantes** com XML comments.
- **Não duplique valores** em locais diferentes.
