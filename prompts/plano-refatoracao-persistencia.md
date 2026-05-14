# Plano de Refatoração — Persistência com EF Core como UoW e Repository

## Contexto e objetivos

O objetivo central é eliminar as wrapping classes de repositório — que hoje introduzem o anti-pattern de `SaveChangesAsync` por método — e usar o próprio `DbContext` como implementação concreta de `IUnitOfWork` e `IRepository<TAggregate, TKey>`, preservando o isolamento da camada de aplicação do EF Core.

Queries complexas migram para interfaces `IXxxQueries` próprias. O `IOutboxRepository` e o `AddIfNoOverlapAsync` de Bookings recebem tratamento especial e não seguem o padrão geral.

### Definições arquiteturais

| Conceito EF Core                       | Papel no novo modelo                               |
| -------------------------------------- | -------------------------------------------------- |
| `DbSet<T>`                             | Repositório                                        |
| LINQ to Entities                       | Especificação (sem Specification Pattern)          |
| `DbContext`                            | Unit of Work                                       |
| Partial classes do DbContext           | Implementação de `IRepository<T, K>` por aggregate |
| `IXxxQueries` + implementação na infra | CQRS para leituras complexas                       |

### Estado atual — inventário por módulo

| Módulo          | DbContext                  | Interfaces IRepository | Classes Repository |
| --------------- | -------------------------- | ---------------------- | ------------------ |
| Bookings        | `BookingsDbContext`        | 2                      | 2                  |
| Communications  | `CommunicationsDbContext`  | 3                      | 1 (Outbox)         |
| Documents       | `DocumentsDbContext`       | 1                      | 1                  |
| Locations       | `LocationsDbContext`       | 1                      | 1                  |
| Payments        | `PaymentsDbContext`        | 2                      | 2                  |
| Providers       | `ProvidersDbContext`       | 1                      | 1                  |
| Ratings         | `RatingsDbContext`         | 1                      | 1                  |
| SearchProviders | `SearchProvidersDbContext` | 1                      | 1                  |
| ServiceCatalogs | `ServiceCatalogsDbContext` | 2                      | 2                  |
| Users           | `UsersDbContext`           | 1                      | 1                  |

### Problemas concretos que a refatoração resolve

**1. `SaveChangesAsync` chamado dentro de cada método de repositório**

Todos os repositórios atuais persistem individualmente. Exemplo em `AllowedCityRepository`:

```csharp
public async Task AddAsync(AllowedCity allowedCity, CancellationToken ct) {
    await context.AllowedCities.AddAsync(allowedCity, ct);
    await context.SaveChangesAsync(ct); // ← impossibilita transações compostas
}
```

**2. `SaveChangesAsync` exposto na interface de domínio**

```csharp
// ISearchableProviderRepository — SaveChanges não pertence ao domínio
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
```

**3. Wrapping classes desnecessárias** que apenas delegam para o DbContext, sem adicionar lógica.

**4. Scrutor via `AddModuleRepositories` quebrará** com o novo modelo, pois o DbContext não termina em `*Repository`. O scan automático precisa ser substituído por registro explícito.

---

## Visão geral das fases

| Fase  | Escopo                                | Dependência     |
| ----- | ------------------------------------- | --------------- |
| **0** | Shared — `IRepository`, `IUnitOfWork` | Nenhuma         |
| **1** | Locations (módulo piloto)             | Fase 0          |
| **2** | Ratings, Documents, ServiceCatalogs   | Fase 1 validada |
| **3** | Providers, Users, SearchProviders     | Fase 2          |
| **4** | Bookings, Communications, Payments    | Fase 2          |
| **5** | Limpeza e consolidação global         | Fases 3 e 4     |

> **Regra de branch:** criar uma branch por fase, com PR de validação antes de avançar. Locations como piloto é crítico — valida o padrão inteiro antes de comprometer os demais módulos.

---

## Fase 0 — Preparação do Shared

> Não toca em nenhum módulo. Apenas cria as interfaces base.

### Passo 0.1 — Criar `IRepository<TAggregate, TKey>`

```csharp
// src/Shared/Database/IRepository.cs
namespace MeAjudaAi.Shared.Database;

public interface IRepository<TAggregate, TKey>
{
    Task<TAggregate?> TryFindAsync(TKey key, CancellationToken cancellationToken = default);
    void Add(TAggregate aggregate);
    void Delete(TAggregate aggregate);

    // Implementação default — evita duplicação nos DbContexts parciais
    async Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var aggregate = await TryFindAsync(key, cancellationToken)
            ?? throw new ArgumentException($"Aggregate with key '{key}' not found.");
        Delete(aggregate);
    }
}
```

### Passo 0.2 — Criar `IUnitOfWork`

```csharp
// src/Shared/Database/IUnitOfWork.cs
namespace MeAjudaAi.Shared.Database;

public interface IUnitOfWork
{
    IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Passo 0.3 — Verificar e Atualizar `BaseDbContext`

Garantir que `BaseDbContext` suporte a coleta e despacho de domain events para `AggregateRoot<TKey>` de qualquer `TKey` (não apenas `Guid`). 
- Atualizar `GetDomainEventsAsync` e `ClearDomainEvents` para iterar sobre entidades tipadas como `BaseEntity` (ou `AggregateRoot<T>`) de forma genérica.
- Validar via teste unitário que agregados com IDs fortemente tipados (ex: `ServiceId`, `BookingId`) têm seus eventos capturados e despachados corretamente.
- Confirmar que o override de `SaveChangesAsync` chama o dispatch antes de persistir.

### Passo 0.4 — Marcar scan do Scrutor como Obsoleto

Localizar o `ModuleServiceRegistrationExtensions` (ou equivalente) que usa Scrutor com `AddModuleRepositories` e marcar o método como `[Obsolete]`. Esse scan automático será substituído por registro explícito do DbContext como `IUnitOfWork` em cada módulo nas fases seguintes. O método deve ser removido completamente apenas na Fase 5.

---

## Fase 1 — Módulo piloto: Locations

> Escopo mínimo: 1 aggregate, handlers simples. Valida o padrão completo antes de escalar.

### Passo 1.1 — Criar `IAllowedCityQueries`

Identificar todos os métodos de `IAllowedCityRepository` que são leituras puras (sem change tracking) e criar a interface na camada de aplicação do módulo:

```csharp
// Módulo Locations — Application layer
public interface IAllowedCityQueries
{
    Task<bool> ExistsAsync(string cityName, string stateSigla, CancellationToken ct = default);
    // demais leituras identificadas no repositório atual
}
```

### Passo 1.2 — Refatorar `LocationsDbContext` em partial classes

**Arquivo principal** (`LocationsDbContext.cs`):

```csharp
public partial class LocationsDbContext(
    DbContextOptions<LocationsDbContext> options,
    IDomainEventProcessor domainEventProcessor)
    : BaseDbContext(options, domainEventProcessor), IUnitOfWork
{
    public DbSet<AllowedCity> AllowedCities => Set<AllowedCity>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>() =>
        (IRepository<TAggregate, TKey>)this;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AllowedCityConfiguration());
    }
}
```

**Arquivo do aggregate** (`LocationsDbContext.AllowedCity.cs`):

```csharp
public partial class LocationsDbContext : IRepository<AllowedCity, Guid>
{
    async Task<AllowedCity?> IRepository<AllowedCity, Guid>.TryFindAsync(
        Guid key, CancellationToken ct) =>
        await AllowedCities.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<AllowedCity, Guid>.Add(AllowedCity aggregate) =>
        AllowedCities.Add(aggregate);

    void IRepository<AllowedCity, Guid>.Delete(AllowedCity aggregate) =>
        AllowedCities.Remove(aggregate);
}
```

### Passo 1.3 — Criar `DbContextAllowedCityQueries` na camada de infraestrutura

```csharp
// Módulo Locations — Infrastructure layer
public class DbContextAllowedCityQueries(LocationsDbContext dbContext) : IAllowedCityQueries
{
    public async Task<bool> ExistsAsync(string cityName, string stateSigla, CancellationToken ct) =>
        await dbContext.AllowedCities
            .AsNoTracking()
            .AnyAsync(x => x.Name == cityName && x.StateSigla == stateSigla, ct);
}
```

### Passo 1.4 — Atualizar o registro de DI do módulo

Remover o registro de `IAllowedCityRepository` / `AllowedCityRepository`. Adicionar:

```csharp
services.AddDbContext<LocationsDbContext>(options => /* ... */);
// Importante: IUnitOfWork deve resolver a mesma instância do DbContext
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LocationsDbContext>());
services.AddScoped<IAllowedCityQueries, DbContextAllowedCityQueries>();
```

**Validação de DI:** Garantir que todos os agregados do módulo tenham suas interfaces `IXxxQueries` registradas para evitar `InvalidOperationException` em runtime.

### Passo 1.5 — Refatorar os handlers do módulo

Substituir `IAllowedCityRepository` por `IUnitOfWork` (commands) e `IAllowedCityQueries` (reads):

```csharp
public sealed class CreateAllowedCityHandler(
    IUnitOfWork uow,
    IAllowedCityQueries queries,
    IGeocodingService geocodingService) : ICommandHandler<CreateAllowedCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateAllowedCityCommand command, CancellationToken ct)
    {
        if (await queries.ExistsAsync(command.CityName, command.StateSigla, ct))
            return Result<Guid>.Failure(Error.Conflict(...));

        var city = new AllowedCity(...);
        uow.GetRepository<AllowedCity, Guid>().Add(city);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(city.Id);
    }
}
```

### Passo 1.6 — Deletar arquivos obsoletos

- `IAllowedCityRepository.cs`
- `AllowedCityRepository.cs`
- Testes unitários do repositório

### Passo 1.7 — Adaptar testes de integração

Substituir injeção de `IAllowedCityRepository` por `IUnitOfWork` e `IAllowedCityQueries` nos testes existentes. A semântica dos testes permanece — apenas o ponto de entrada muda.

### Passo 1.8 — Validação da fase

Rodar todos os testes de integração do módulo Locations. Verificar que migrations e schema Postgres não foram afetados. Somente após aprovação do PR avançar para a Fase 2.

---

## Fase 2 — Módulos simples

> Seguir o mesmo roteiro da Fase 1 para cada módulo. Ordem recomendada: **Ratings → Documents → ServiceCatalogs**.

### Checklist de Completude por Módulo

Para cada módulo, executar em sequência:

1. Mapear métodos do repositório atual: separar command (tracking necessário) de query (leitura pura).
2. Criar `IXxxQueries` para as leituras puras na camada de aplicação.
3. Transformar o DbContext em partial com um arquivo por aggregate.
4. Criar `DbContextXxxQueries` na camada de infraestrutura.
5. Atualizar o registro de DI do módulo (garantir `IUnitOfWork` compartilhado e todas as queries registradas).
6. Refatorar handlers substituindo `IXxxRepository` por `IUnitOfWork` + `IXxxQueries`.
7. **Verificação Mandatória de Resíduos:** Realizar busca por injeções de interfaces legadas (ex: `rg "IServiceRepository|IServiceCategoryRepository"`) e garantir que 100% dos handlers (commands e queries) foram migrados.
8. Deletar `IXxxRepository.cs`, `XxxRepository.cs` e testes unitários do repositório.
9. Adaptar testes de integração.
10. **Validação de Eventos de Domínio:** Adicionar teste de integração que persista um agregado, emita um evento e confirme que o respectivo handler foi invocado.
11. **Verificação de Isolamento de DB:** Garantir que o módulo não faça chamadas diretas ao banco de dados de outros módulos e que toda comunicação inter-módulos ocorra exclusivamente via `IModuleApi`.

> **Atenção — ServiceCatalogs:** possui 2 aggregates. Criar um arquivo partial separado no DbContext para cada aggregate. Garantir que `BaseDbContext` processe `AggregateRoot<ServiceId>` e `AggregateRoot<ServiceCategoryId>`.

---

## Fase 3 — Módulos com queries complexas

### Providers

O `IProviderRepository` atual possui 12+ métodos. A maioria são queries puras que devem migrar para `IProviderQueries`. O `ProviderQueryService` existente já implementa parte desse padrão — expandir para cobrir todos os métodos listados abaixo.

**Mapeamento completo de `IProviderRepository`:**

| Método atual                   | Destino no novo modelo                           |
| ------------------------------ | ------------------------------------------------ |
| `GetBySlugAsync`               | `IProviderQueries`                               |
| `GetByIdsAsync`                | `IProviderQueries`                               |
| `GetByCityAsync`               | `IProviderQueries`                               |
| `GetByStateAsync`              | `IProviderQueries`                               |
| `GetByVerificationStatusAsync` | `IProviderQueries`                               |
| `GetByTypeAsync`               | `IProviderQueries`                               |
| `GetProviderStatusAsync`       | `IProviderQueries`                               |
| `HasProvidersWithServiceAsync` | `IProviderQueries`                               |
| `GetByUserIdAsync`             | `IProviderQueries`                               |
| `ExistsAsync`                  | `IProviderQueries` ou lógica inline no handler   |
| `GetByIdAsync` (command side)  | `IRepository<Provider, ProviderId>.TryFindAsync` |
| `AddAsync`                     | `IRepository<Provider, ProviderId>.Add`          |
| `DeleteAsync`                  | `IRepository<Provider, ProviderId>.Delete`       |

### Users

O `IUserRepository` distingue explicitamente `GetByIdAsync` (NoTracking) de `GetByIdTrackedAsync` (Tracking). Adotar a convenção definitiva:

- `TryFindAsync` → sempre tracked (suporta change tracking do UoW)
- Métodos em `IUserQueries` → sempre `AsNoTracking()` (otimizados para leitura)

### SearchProviders

Verificar se há queries de busca que podem ser expostas via `ISearchProviderQueries` separada. Aplicar o roteiro padrão das Fases 1–2.

---

## Fase 4 — Módulos com casos especiais

### Bookings — `AddIfNoOverlapAsync`

Esta operação **não pode** ser simplificada para `IRepository<Booking, BookingId>.Add()` porque envolve lógica transacional complexa: transação serializável, retry com backoff exponencial, detecção de erros Npgsql `40001`/`40P01`, e idempotência por ID.

**Solução:** criar `IBookingCommandService` na camada de aplicação.

```csharp
// Application layer — interface
public interface IBookingCommandService
{
    Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken ct = default);
}

// Infrastructure layer — implementação
public class DbContextBookingCommandService(BookingsDbContext dbContext) : IBookingCommandService
{
    public async Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken ct)
    {
        // Lógica original: transação serializável + retry + detecção Npgsql permanece aqui
    }
}
```

O restante de `IBookingRepository` (leituras, `GetByIdAsync`, etc.) segue o roteiro padrão.

### Communications — `IOutboxRepository<TMessage>`

**Não migrar.** O Outbox é infraestrutura de mensageria, não um aggregate DDD. Possui seu próprio `SaveChangesAsync`, lida com transações serializáveis e processamento em batch — características incompatíveis com a interface genérica `IRepository<TAggregate, TKey>`.

Manter `IOutboxRepository` e sua implementação exatamente como estão. Remover apenas os demais repositórios do módulo Communications que seguem o padrão geral.

### Payments

Verificar se há operações transacionais complexas semelhantes ao `AddIfNoOverlapAsync` de Bookings. Se houver: aplicar o mesmo padrão de `IPaymentCommandService`. Caso contrário: seguir o roteiro padrão das Fases 1–2 (2 aggregates, criar um partial por aggregate).

---

## Fase 5 — Limpeza e consolidação global

### Passo 5.1 — Remover `AddModuleRepositories` completamente

Após todos os módulos migrados, o método de extensão com scan do Scrutor pode ser removido. Cada módulo agora registra seu DbContext explicitamente.

### Passo 5.2 — Varredura por `IXxxRepository` remanescentes

Busca global por interfaces terminadas em `Repository`. Apenas `IOutboxRepository` deve permanecer — qualquer outro é um item esquecido da migração.

### Passo 5.3 — Varredura por `SaveChangesAsync` fora do lugar

Busca global por chamadas a `SaveChangesAsync`. Devem existir apenas em: handlers/endpoints (via `IUnitOfWork`), `BaseDbContext` (dispatch de domain events), e `IOutboxRepository` (exceção justificada). Qualquer ocorrência dentro de classes de repositório ou serviços de infraestrutura é um resíduo a eliminar.

### Passo 5.4 — Revisão dos testes de integração

Confirmar que todos os testes adaptados ao longo das fases estão passando. Verificar cobertura nos módulos que tinham lógica mais complexa (Bookings, Providers).

### Passo 5.5 — Documentar convenções estabelecidas

Registrar no `README` ou `docs/architecture.md` do projeto:

- `TryFindAsync` → sempre tracked
- Queries em `IXxxQueries` → sempre `AsNoTracking()`
- `IOutboxRepository` → exceção arquitetural justificada (mensageria, não DDD aggregate)
- Operações transacionais complexas → `IXxxCommandService` dedicado
- Partial classes no DbContext → um arquivo por aggregate

---

## Referência rápida — padrões do novo modelo

### Endpoint/Handler — independente do EF Core

```csharp
public static async Task<IResult> AddInvoiceLine(
    IUnitOfWork uow, Guid invoiceId, AddInvoiceLineRequest request)
{
    var invoice = await uow.GetRepository<Invoice, Invoice.InvoiceId>()
        .TryFindAsync(new Invoice.InvoiceId(invoiceId));

    if (invoice == null) return Results.NotFound();

    invoice.AddLine(new Product.ProductId(request.ProductId), request.Amount);
    await uow.SaveChangesAsync();

    return Results.Ok();
}
```

### DbContext como UoW + Repository (arquivo principal)

```csharp
public partial class ApiDbContext(DbContextOptions<ApiDbContext> options,
    IDomainEventProcessor domainEventProcessor)
    : BaseDbContext(options, domainEventProcessor), IUnitOfWork
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Product> Products => Set<Product>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>() =>
        (IRepository<TAggregate, TKey>)this;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
    }
}
```

### Partial class por aggregate

```csharp
public partial class ApiDbContext : IRepository<Invoice, Invoice.InvoiceId>
{
    async Task<Invoice?> IRepository<Invoice, Invoice.InvoiceId>.TryFindAsync(
        Invoice.InvoiceId key, CancellationToken ct) =>
        await Invoices
            .Include("LinesCollection")
            .FirstOrDefaultAsync(i => i.PublicId == key, ct);

    void IRepository<Invoice, Invoice.InvoiceId>.Add(Invoice aggregate) =>
        Invoices.Add(aggregate);

    void IRepository<Invoice, Invoice.InvoiceId>.Delete(Invoice aggregate) =>
        Invoices.Remove(aggregate);
}
```

### Query service na infraestrutura

```csharp
public class DbContextProductQueries(ApiDbContext dbContext) : IProductQueries
{
    public async Task<IProductQueries.ProductConsumption[]> GetMaterialConsumptionAsync(
        DateOnly from, DateOnly to) =>
        await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.DueDate >= from && i.DueDate <= to)
            .SelectMany(i => EF.Property<List<InvoiceLine>>(i, "LinesCollection")
                .Select(il => new { il.ProductId, il.Amount }))
            .Join(dbContext.Products.OfType<Material>(), il => il.ProductId, p => p.PublicId,
                (il, m) => new { Material = m, Amount = il.Amount })
            .GroupBy(pair => pair.Material.PublicId,
                (key, group) => new IProductQueries.ProductConsumption
                {
                    ProductId = key.Value,
                    Name = group.First().Material.Name,
                    AmountSold = group.Sum(g => g.Amount)
                })
            .ToArrayAsync();
}
```
