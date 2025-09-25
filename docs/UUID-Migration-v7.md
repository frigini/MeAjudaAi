# UUID v7 Migration

## Overview
Migração de UUID v4 (Guid.NewGuid()) para UUID v7 (Guid.CreateVersion7()) para melhorar performance e ordenação temporal.

## Implementação

### UuidGenerator
Classe central para geração de identificadores únicos usando UUID v7, localizada em `MeAjudaAi.Shared.Time`:

```csharp
using MeAjudaAi.Shared.Time;

public static class UuidGenerator
{
    public static Guid NewId() => Guid.CreateVersion7();
    public static string NewIdString() => Guid.CreateVersion7().ToString();
    public static string NewIdStringCompact() => Guid.CreateVersion7().ToString("N");
}
```

### Componentes Migrados
- ✅ **BaseEntity.cs**: ID da entidade base
- ✅ **UserId.cs**: Identificador de usuário
- ✅ **Command.cs**: CorrelationId de comandos
- ✅ **Query.cs**: CorrelationId de queries
- ✅ **DomainEvent.cs**: ID de eventos de domínio
- ✅ **ServiceBusMessageBus.cs**: MessageId do Service Bus
- ✅ **CorrelationIdEnricher.cs**: Enriquecedor de logs
- ✅ **RequestLoggingMiddleware.cs**: Logging de requisições
- ✅ **LoggingContextMiddleware.cs**: Contexto de logging

## Benefícios

### Performance
- **PostgreSQL 18**: Suporte nativo para UUID v7
- **Índices**: Melhor performance devido à ordenação temporal
- **Clustering**: Dados relacionados temporalmente ficam próximos

### Temporal Ordering
- **Ordenação natural**: UUIDs seguem ordem cronológica
- **Troubleshooting**: Facilita análise de logs e debug
- **Auditoria**: Ordem de criação preservada automaticamente

## Compatibilidade
- ✅ **Backward Compatible**: UUID v7 são válidos como Guid .NET
- ✅ **Database**: PostgreSQL 18+ com suporte nativo
- ✅ **Serialization**: JSON/XML mantém formato string padrão
- ✅ **APIs**: Endpoints continuam usando mesmo formato

## Validation
- **560 testes** passaram após migração
- **Zero breaking changes** identificadas
- **Produção ready**: Migração não invasiva

## Next Steps
- Considere migrar dados existentes gradualmente se necessário
- Monitor performance improvements em production
- APIs de módulos (Phase 2) podem usar mesma estratégia