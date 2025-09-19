# MeAjudaAi Aspire Extensions

## 📁 Estrutura das Extensions

Esta pasta contém as extension methods customizadas para simplificar a configuração da infraestrutura do MeAjudaAi no Aspire AppHost.

### Arquivos

- **PostgreSqlExtensions.cs**: Configuração otimizada do PostgreSQL para teste/dev/produção
- **RedisExtensions.cs**: Configuração do Redis com otimizações por ambiente
- **RabbitMQExtensions.cs**: Configuração do RabbitMQ/Service Bus
- **KeycloakExtensions.cs**: Configuração do Keycloak com diferentes bancos de dados

## 🚀 Como Usar

### PostgreSQL
```csharp
// Detecção automática de ambiente
var postgresql = builder.AddMeAjudaAiPostgreSQL();

// Configuração manual
var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
{
    options.MainDatabase = "myapp-db";
    options.IncludePgAdmin = true;
});
```

### Redis
```csharp
var redis = builder.AddMeAjudaAiRedis(options =>
{
    options.MaxMemory = "512mb";
    options.IncludeRedisCommander = true;
});
```

### RabbitMQ
```csharp
// Desenvolvimento/Teste
var rabbitMq = builder.AddMeAjudaAiRabbitMQ();

// Produção (Service Bus)
var serviceBus = builder.AddMeAjudaAiServiceBus();
```

### Keycloak
```csharp
// Desenvolvimento
var keycloak = builder.AddMeAjudaAiKeycloak();

// Produção
var keycloak = builder.AddMeAjudaAiKeycloakProduction();
```

## 🎯 Benefícios

- **Detecção Automática de Ambiente**: Configurações otimizadas baseadas no ambiente
- **Configurações de Teste**: Otimizações para performance e rapidez nos testes
- **Ferramentas de Desenvolvimento**: PgAdmin, Redis Commander, RabbitMQ Management
- **Produção Pronta**: Configurações Azure e parâmetros seguros
- **Código Limpo**: Program.cs reduzido em 45% (220 → 120 linhas)