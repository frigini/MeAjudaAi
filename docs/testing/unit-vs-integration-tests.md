# Unit vs Integration Tests - Best Practices

## Visão Geral

Este documento define as melhores práticas para testes unitários e de integração no projeto MeAjudaAi.

## Diferença entre Unit e Integration Tests

### Unit Tests
- **Objetivo**: Testar uma única unidade de código isoladamente
- **Infraestrutura**: Sem dependências externas (banco de dados, APIs, filas)
- **Velocidade**: Muito rápidos (< 100ms por teste)
- **Mocks**: Usa mocks/stubs para todas as dependências
- **Coverage**: Deve cobrir toda a lógica de negócio e edge cases

### Integration Tests
- **Objetivo**: Testar integração entre componentes e infraestrutura
- **Infraestrutura**: Usa banco de dados real, message brokers, etc.
- **Velocidade**: Mais lentos (100ms - 5s por teste)
- **Mocks**: Mínimo de mocks, apenas para serviços externos (APIs de terceiros)
- **Coverage**: Valida comportamento end-to-end com infraestrutura real

## ❌ Anti-Patterns Comuns

### 1. Usar InMemory Database para Testes Unitários de EF Core Configuration

**Problema**:
```csharp
// ❌ ERRADO - InMemory não valida configurações reais
[Fact]
public void Configure_ShouldConvertEnumsToInt()
{
    var options = new DbContextOptionsBuilder<MyContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    
    using var context = new MyContext(options);
    var entityType = context.Model.FindEntityType(typeof(MyEntity));
    
    // InMemory não popula GetValueConverter() mesmo com conversores configurados
    entityType.FindProperty("Status").GetValueConverter().Should().NotBeNull(); // ❌ FALHA
}
```

**Limitações do InMemory Provider**:
- ❌ Não valida constraints (FK, Unique, Check)
- ❌ Não executa migrations
- ❌ Não popula value converters
- ❌ Não valida tipos de coluna do banco real (jsonb, geography, etc.)
- ❌ Comportamento diferente do PostgreSQL (case sensitivity, null handling, etc.)

**Solução**:
```csharp
// ✅ CORRETO - Teste de integração com banco real
[Fact]
public async Task Configure_ShouldConvertEnumsToInt()
{
    // Usa PostgreSQL real via Testcontainers ou Docker
    await using var factory = new CustomWebApplicationFactory();
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<MyContext>();
    
    // Teste com banco real valida conversores, constraints, etc.
    var entity = new MyEntity { Status = MyStatus.Active };
    context.Add(entity);
    await context.SaveChangesAsync();
    
    // Verifica no banco que o enum foi convertido para int
    var rawSql = await context.Database.GetDbConnection()
        .QueryAsync<int>("SELECT status FROM my_table WHERE id = @id", new { id = entity.Id });
    rawSql.Should().Be((int)MyStatus.Active);
}
```

### 2. Testes E2E que apenas repetem testes de integração

**Problema**:
```csharp
// ❌ REDUNDANTE - Este teste E2E apenas repete o teste de integração
[Fact]
public async Task CreateDocument_ShouldReturn201()
{
    var client = _factory.CreateClient();
    var response = await client.PostAsync("/api/documents", content);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

Testes E2E devem validar **fluxos completos** (múltiplos endpoints, eventos, side effects), não apenas endpoints individuais já cobertos por testes de integração.

## ✅ Quando Usar Cada Tipo de Teste

### Use Unit Tests para:

1. **Lógica de Domínio** (Entities, Value Objects, Domain Events)
```csharp
[Fact]
public void Document_Verify_ShouldSetVerifiedAt()
{
    var document = Document.Create(providerId, documentType, fileUrl, fileName);
    var now = DateTime.UtcNow;
    
    document.Verify();
    
    document.VerifiedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    document.Status.Should().Be(EDocumentStatus.Verified);
}
```

2. **Application Handlers** (Commands, Queries)
```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateDocument()
{
    // Arrange - Mock repositories
    var repositoryMock = new Mock<IDocumentRepository>();
    var handler = new CreateDocumentCommandHandler(repositoryMock.Object);
    
    // Act
    var result = await handler.Handle(command);
    
    // Assert
    repositoryMock.Verify(x => x.AddAsync(It.IsAny<Document>()), Times.Once);
}
```

3. **Domain Event Handlers**
```csharp
[Fact]
public async Task HandleAsync_ShouldPublishIntegrationEvent()
{
    // Arrange - Mock message bus
    var messageBusMock = new Mock<IMessageBus>();
    var handler = new DocumentVerifiedDomainEventHandler(messageBusMock.Object);
    
    // Act
    await handler.HandleAsync(domainEvent);
    
    // Assert
    messageBusMock.Verify(x => x.PublishAsync(
        It.Is<DocumentVerifiedIntegrationEvent>(e => e.DocumentId == domainEvent.AggregateId)),
        Times.Once);
}
```

4. **Validators, Extensions, Helpers**
```csharp
[Fact]
public void ToGeoPoint_ValidCoordinates_ShouldConvert()
{
    var result = GeometryExtensions.ToGeoPoint(-23.5505, -46.6333);
    result.Coordinate.X.Should().Be(-46.6333);
    result.Coordinate.Y.Should().Be(-23.5505);
}
```

### Use Integration Tests para:

1. **EF Core Configuration** (Entities, Mappings, Migrations)
```csharp
[Fact]
public async Task DocumentConfiguration_ShouldStoreEnumsAsIntegers()
{
    // Usa banco real
    await using var factory = new CustomWebApplicationFactory();
    var context = factory.Services.GetRequiredService<DocumentsDbContext>();
    
    var document = Document.Create(providerId, EDocumentType.RG, url, name);
    context.Add(document);
    await context.SaveChangesAsync();
    
    // Verifica no banco
    var connection = context.Database.GetDbConnection();
    var type = await connection.ExecuteScalarAsync<int>(
        "SELECT document_type FROM documents.documents WHERE id = @id",
        new { id = document.Id });
    
    type.Should().Be(1); // RG = 1
}
```

2. **Repository Implementations**
```csharp
[Fact]
public async Task GetByIdAsync_ExistingDocument_ShouldReturn()
{
    await using var factory = new CustomWebApplicationFactory();
    var repository = factory.Services.GetRequiredService<IDocumentRepository>();
    
    // Seed data
    var document = await SeedDocument();
    
    // Act
    var result = await repository.GetByIdAsync(document.Id);
    
    // Assert
    result.Should().NotBeNull();
    result.FileUrl.Should().Be(document.FileUrl);
}
```

3. **API Endpoints** (Controllers)
```csharp
[Fact]
public async Task CreateDocument_ValidRequest_ShouldReturn201()
{
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    var request = new CreateDocumentRequest { ... };
    var response = await client.PostAsJsonAsync("/api/documents", request);
    
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var document = await response.Content.ReadFromJsonAsync<DocumentResponse>();
    document.Id.Should().NotBeEmpty();
}
```

4. **Message Bus Integration** (Events, Queues)
```csharp
[Fact]
public async Task PublishAsync_ShouldSendMessageToQueue()
{
    await using var factory = new CustomWebApplicationFactory();
    var messageBus = factory.Services.GetRequiredService<IMessageBus>();
    
    await messageBus.PublishAsync(new DocumentVerifiedIntegrationEvent(...));
    
    // Verifica que a mensagem foi publicada no RabbitMQ/ServiceBus
    var consumer = factory.Services.GetRequiredService<TestMessageConsumer>();
    var message = await consumer.WaitForMessageAsync<DocumentVerifiedIntegrationEvent>();
    message.Should().NotBeNull();
}
```

### Use E2E Tests para:

1. **Fluxos Completos de Negócio**
```csharp
[Fact]
public async Task ProviderRegistration_CompleteFlow_ShouldSucceed()
{
    // 1. Criar provider
    var createResponse = await client.PostAsync("/api/providers", providerData);
    var provider = await createResponse.Content.ReadFromJsonAsync<ProviderResponse>();
    
    // 2. Upload de documento
    var uploadResponse = await client.PostAsync($"/api/documents", documentData);
    var document = await uploadResponse.Content.ReadFromJsonAsync<DocumentResponse>();
    
    // 3. Verificar documento
    var verifyResponse = await client.PatchAsync($"/api/documents/{document.Id}/verify", null);
    
    // 4. Verificar que provider foi atualizado (via evento)
    await Task.Delay(2000); // Aguarda processamento assíncrono
    var providerResponse = await client.GetAsync($"/api/providers/{provider.Id}");
    var updatedProvider = await providerResponse.Content.ReadFromJsonAsync<ProviderResponse>();
    updatedProvider.IsVerified.Should().BeTrue();
}
```

## Checklist de Decisão

Ao criar um novo teste, pergunte:

| Pergunta | Unit | Integration | E2E |
|----------|------|-------------|-----|
| Testa lógica isolada sem dependências? | ✅ | ❌ | ❌ |
| Precisa de banco de dados? | ❌ | ✅ | ✅ |
| Precisa de message broker? | ❌ | ✅ | ✅ |
| Testa múltiplos endpoints? | ❌ | ❌ | ✅ |
| Testa eventos assíncronos? | ❌ | ✅ | ✅ |
| Valida EF Core configuration? | ❌ | ✅ | ❌ |
| Executa em < 100ms? | ✅ | ❌ | ❌ |
| Usa mocks? | ✅ | Poucos | Mínimo |

## Resumo

- ✅ **Unit Tests**: Lógica pura, mocks para tudo, rápidos
- ✅ **Integration Tests**: Banco real, repositórios, APIs, eventos
- ✅ **E2E Tests**: Fluxos completos multi-endpoint
- ❌ **InMemory Database**: Nunca para testes de configuração EF Core
- ❌ **E2E Redundantes**: Não duplicar testes de integração
