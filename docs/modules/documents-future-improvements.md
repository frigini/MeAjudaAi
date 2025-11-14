# Documents Module - Future Improvements

Este documento registra melhorias sugeridas no code review que foram **intencionalmente adiadas** para iterações futuras, com justificativas para cada decisão.

## 1. IDocumentRepository - DocumentId Type-Safety

### Sugestão
Usar `DocumentId` ao invés de `Guid` nas assinaturas do repositório:

```csharp
// Atual
Task<Document?> GetByIdAsync(Guid id, ...);

// Sugerido
Task<Document?> GetByIdAsync(DocumentId id, ...);
```

### Status
⏸️ **Adiado para v2**

### Justificativa
- **Prós**: Maior type-safety, evita confusão entre diferentes GUIDs
- **Contras**: 
  - Quebra consistência com outros repositórios (Users, Providers usam Guid)
  - Requer mudanças em handlers e controllers
  - Value objects já fornecem conversão implícita (funciona bem atualmente)
  
### Decisão
Manter `Guid` por enquanto para:
1. Consistência entre módulos
2. Simplicidade na camada de aplicação
3. Conversão implícita já protege contra erros

**Revisar quando**: Próxima refatoração cross-module de repositórios.

---

## 2. Document Entity - IDateTimeProvider Injection

### Sugestão
Injetar `IDateTimeProvider` ao invés de usar `DateTime.UtcNow`:

```csharp
// Atual
public static Document Create(...) 
{
    // ...
    UploadedAt = DateTime.UtcNow
}

// Sugerido
public static Document Create(..., IDateTimeProvider dateTimeProvider) 
{
    // ...
    UploadedAt = dateTimeProvider.UtcNow
}
```

### Status
⏸️ **Adiado para v2**

### Justificativa
- **Prós**: 
  - Agregados completamente determínisticos
  - Facilitaria testes de timestamp
  
- **Contras**:
  - Viola princípios DDD (agregados devem ser self-contained)
  - Adiciona complexidade na criação de entidades
  - Testes atuais já validam timestamps com tolerância (`BeCloseTo`)
  - Alternativa: usar factories que encapsulam o clock

### Decisão
Manter `DateTime.UtcNow` porque:
1. Agregados DDD devem ser independentes de serviços externos
2. Testes atuais já cobrem adequadamente com `BeCloseTo`
3. Se necessário no futuro, criar factory com clock injetado

**Revisar quando**: Surgir necessidade de testes com timestamps exatos ou time travel testing.

---

## 3. Document.MarkAsFailed - Domain Event

### Sugestão
Adicionar `DocumentFailedDomainEvent` e considerar separar `FailureReason` de `RejectionReason`:

```csharp
public void MarkAsFailed(string failureReason)
{
    Status = EDocumentStatus.Failed;
    FailureReason = failureReason; // Novo campo dedicado
    AddDomainEvent(new DocumentFailedDomainEvent(...)); // Novo evento
}
```

### Status
⏸️ **Adiado para v2**

### Justificativa
- **Contexto atual**: `Failed` é usado apenas para erros técnicos (OCR timeout, service unavailable).
- **Rejected** é para rejeição de negócio (documento ilegível, inválido).

**Prós da separação**:
- Semântica mais clara
- Eventos permitiriam rastreamento de falhas técnicas

**Contras**:
- Overengineering para MVP
- Casos de uso atuais não requerem eventos de falha
- Adiciona complexidade sem benefício imediato

### Decisão
Manter implementação atual porque:
1. MVP não requer rastreamento detalhado de falhas técnicas
2. `RejectionReason` serve adequadamente para logs/debug
3. Se eventos de falha forem necessários, adicionar incrementalmente

**Revisar quando**: 
- Implementar dashboard de monitoramento
- Precisar rastrear padrões de falha para SLA
- Adicionar retry automático

---

## 4. Integration Tests - Strict Error Handling

### Sugestão
Eliminar branches error-tolerant em testes de integração:

```csharp
// Atual
if (response.StatusCode == HttpStatusCode.OK) {
    // Assert success
} else {
    response.StatusCode.Should().BeOneOf(503, 500); // Tolerante
}

// Sugerido
response.StatusCode.Should().Be(HttpStatusCode.OK); // Sempre espera sucesso
```

### Status
✅ **Parcialmente Implementado**

### Justificativa
- **Problema**: Testes permitem falhas quando blob storage não está disponível
- **Solução sugerida**: Mock ou Azurite

**Status atual**:
- ✅ Adicionado TODO nos testes indicando necessidade de configurar Azurite
- ⏸️ Implementação de Azurite adiada

**Decisão**:
1. Manter fallback temporário para CI/CD funcionar
2. Adicionar task para configurar Azurite no TestContainerTestBase
3. Remover fallback quando Azurite estiver configurado

**Próximos passos**:
```csharp
// Em TestContainerTestBase.cs
private AzuriteContainer _azuriteContainer;

public override async Task InitializeAsync() 
{
    _azuriteContainer = new AzuriteBuilder().Build();
    await _azuriteContainer.StartAsync();
    
    // Configurar connection string...
}
```

**Issue criada**: #TBD - Configure Azurite in E2E tests

---

## 5. Document Aggregate - Input Guards

### Sugestão
Adicionar validações nos parâmetros do factory method:

```csharp
public static Document Create(
    Guid providerId,
    EDocumentType documentType,
    string fileName,
    string fileUrl)
{
    if (providerId == Guid.Empty)
        throw new ArgumentException("ProviderId cannot be empty");
    if (string.IsNullOrWhiteSpace(fileName))
        throw new ArgumentException("FileName is required");
    if (string.IsNullOrWhiteSpace(fileUrl))
        throw new ArgumentException("FileUrl is required");
        
    // ...
}
```

### Status
⏸️ **Adiado - Validação está na camada de Application**

### Justificativa
**Onde validação ocorre atualmente**:
1. `UploadDocumentCommandHandler` valida inputs antes de criar agregado
2. Handler já valida: providerId, documentType, fileName, contentType, fileSize

**Prós da validação no domínio**:
- Agregado sempre válido
- Proteção defense-in-depth

**Contras**:
- Duplicação de validação (handler + domínio)
- Agregado criado apenas após validação do handler
- Performance overhead mínimo mas desnecessário

### Decisão
**Não implementar** porque:
1. Validação já ocorre em `UploadDocumentCommandHandler`
2. Agregado nunca é criado com dados inválidos no fluxo atual
3. Command validation é responsabilidade da Application layer (CQRS pattern)

**Exceção**: Se factory method for exposto para uso direto (ex: testes, outros módulos), adicionar guards.

---

## Resumo de Status

| Melhoria | Status | Prioridade | Revisar quando |
|----------|--------|-----------|----------------|
| DocumentId no Repository | ⏸️ Adiado | Baixa | Refatoração cross-module |
| IDateTimeProvider no Domain | ⏸️ Adiado | Baixa | Time travel testing necessário |
| DocumentFailedDomainEvent | ⏸️ Adiado | Média | Dashboard de monitoramento |
| Azurite em E2E tests | ✅ TODO Criado | Alta | Sprint atual |
| Input guards no agregado | ❌ Não implementar | N/A | Se factory ficar público |

---

## Notas

Este documento deve ser revisado a cada sprint para reavaliar prioridades baseado em:
- Feedback de produção
- Novos requisitos
- Mudanças arquiteturais
- Technical debt acumulado

**Última revisão**: 2025-11-14
**Próxima revisão**: 2025-12-01
