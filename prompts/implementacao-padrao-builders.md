# Implementação do Padrão Builder — Domínio Compartilhado

## Objetivo

Criar builders de domínio reutilizáveis em `MeAjudaAi.Shared.Tests` para eliminar criação inline de entidades nos testes, padronizar o padrão de construção e servir como base para testes unitários, integração e E2E.

## Contexto

### Problema atual

- **Bookings**: ~88 ocorrências de criação inline (`Booking.Create(...)`, `new Booking(...)`, `ProviderSchedule.Create(...)`)
- **Communications**: ~76 ocorrências de criação inline (`EmailTemplate.Create(...)`, `CommunicationLog.CreateSuccess/Failure(...)`, `OutboxMessage.Create(...)`, `EmailOutboxPayload.Create(...)`, etc.)
- Métodos helper privados duplicados (`CreatePendingBooking`, `BringBookingToStatus`)
- Não existe padronização entre módulos — cada um cria entidades de forma diferente

### Base existente

- `BaseBuilder<T>` em `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/BaseBuilder.cs` — fornece `Build()`, `BuildMany()`, `BuildList()`, `WithCustomAction()`, conversão implícita
- Users, Providers e ServiceCatalogs já possuem builders nos seus projetos de teste
- `MeAjudaAi.Shared.Tests` já tem acesso transitivo a todas as entidades de domínio via `ApiService → API → Application → Domain`
- Todos os módulos de teste já referenciam `Shared.Tests`

### Convenção de nomes de arquivos de prompts

Seguir o padrão existente: `plano-{topic}.md` ou `{topic}.md` — usar `implementacao-padrao-builders.md`

---

## Estrutura de Diretório

```
tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/
├── Bookings/
│   ├── BookingBuilder.cs
│   ├── ProviderScheduleBuilder.cs
│   ├── TimeSlotBuilder.cs
│   └── AvailabilityBuilder.cs
├── Communications/
│   ├── EmailTemplateBuilder.cs
│   ├── CommunicationLogBuilder.cs
│   ├── OutboxMessageBuilder.cs
│   ├── EmailOutboxPayloadBuilder.cs
│   ├── SmsOutboxPayloadBuilder.cs
│   └── PushOutboxPayloadBuilder.cs
├── Users/
│   ├── UserBuilder.cs           (migrado de Users/Tests/Builders/)
│   ├── UsernameBuilder.cs       (migrado)
│   └── EmailBuilder.cs          (migrado)
├── Providers/
│   └── ProviderBuilder.cs       (migrado de Providers/Tests/Builders/)
└── ServiceCatalogs/
    ├── ServiceBuilder.cs        (migrado de ServiceCatalogs/Tests/Builders/)
    └── ServiceCategoryBuilder.cs (migrado)
```

**Namespace:** `MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.{Module}`

---

## Fase 1 — Builders Novos: Bookings (Piloto)

### BookingBuilder

**Arquivo:** `Builders/Modules/Bookings/BookingBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Bookings.Domain.Entities.Booking`
**Método de criação:** `Booking.Create(providerId, clientId, serviceId, date, timeSlot)`

```csharp
public class BookingBuilder : BaseBuilder<Booking>
{
    private Guid? _id;
    private Guid? _providerId;
    private Guid? _clientId;
    private Guid? _serviceId;
    private DateOnly? _date;
    private TimeSlot? _timeSlot;
    private EBookingStatus? _status;

    // CustomInstantiator: usa Booking.Create() para status Pending,
    // ou construtor público para outros status

    public BookingBuilder WithId(Guid id);
    public BookingBuilder WithProviderId(Guid providerId);
    public BookingBuilder WithClientId(Guid clientId);
    public BookingBuilder WithServiceId(Guid serviceId);
    public BookingBuilder WithDate(DateOnly date);
    public BookingBuilder WithTimeSlot(TimeSlot timeSlot);
    public BookingBuilder WithStatus(EBookingStatus status);
    public BookingBuilder AsPending();   // Default via Booking.Create()
    public BookingBuilder AsConfirmed(); // Create + Confirm()
    public BookingBuilder AsRejected();  // Create + Reject("reason")
    public BookingBuilder AsCancelled(); // Create + Cancel("reason")
    public BookingBuilder AsCompleted(); // Create + Confirm() + Complete()
}
```

### ProviderScheduleBuilder

**Arquivo:** `Builders/Modules/Bookings/ProviderScheduleBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Bookings.Domain.Entities.ProviderSchedule`
**Método de criação:** `ProviderSchedule.Create(providerId, timeZoneId?)`

```csharp
public class ProviderScheduleBuilder : BaseBuilder<ProviderSchedule>
{
    private Guid? _providerId;
    private string? _timeZoneId;
    private readonly List<Availability> _availabilities = [];

    public ProviderScheduleBuilder WithProviderId(Guid providerId);
    public ProviderScheduleBuilder WithTimeZoneId(string timeZoneId);
    public ProviderScheduleBuilder WithAvailabilities(params Availability[] availabilities);
    public ProviderScheduleBuilder WithSingleSlot(DayOfWeek day, TimeOnly start, TimeOnly end);
}
```

### TimeSlotBuilder

**Arquivo:** `Builders/Modules/Bookings/TimeSlotBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Bookings.Domain.ValueObjects.TimeSlot`
**Método de criação:** `TimeSlot.Create(start, end)`

```csharp
public class TimeSlotBuilder : BaseBuilder<TimeSlot>
{
    private TimeOnly? _start;
    private TimeOnly? _end;

    public TimeSlotBuilder WithStart(TimeOnly start);
    public TimeSlotBuilder WithEnd(TimeOnly end);
    public TimeSlotBuilder WithDuration(int startHour, int durationHours);
    public TimeSlotBuilder Morning();   // 09:00 - 12:00
    public TimeSlotBuilder Afternoon(); // 13:00 - 17:00
}
```

### AvailabilityBuilder

**Arquivo:** `Builders/Modules/Bookings/AvailabilityBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Bookings.Domain.ValueObjects.Availability`
**Método de criação:** `Availability.Create(dayOfWeek, slots)`

```csharp
public class AvailabilityBuilder : BaseBuilder<Availability>
{
    private DayOfWeek? _dayOfWeek;
    private readonly List<TimeSlot> _slots = [];

    public AvailabilityBuilder WithDayOfWeek(DayOfWeek day);
    public AvailabilityBuilder WithSlots(params TimeSlot[] slots);
    public AvailabilityBuilder WithSingleSlot(TimeOnly start, TimeOnly end);
    public AvailabilityBuilder Monday();
    public AvailabilityBuilder Weekday(); // Seg-Sex com slots padrão
}
```

### Substituições nos testes (~88 ocorrências)

| Padrão Atual | Substituição |
|-------------|-------------|
| `Booking.Create(providerId, clientId, serviceId, date, timeSlot)` | `new BookingBuilder().WithProviderId(providerId)...Build()` |
| `new Booking(id, providerId, clientId, serviceId, date, timeSlot, status, rowVersion)` | `new BookingBuilder().WithId(id)...WithStatus(status).Build()` |
| `ProviderSchedule.Create(providerId)` | `new ProviderScheduleBuilder().WithProviderId(providerId).Build()` |
| `private static CreatePendingBooking()` | `new BookingBuilder()` (método removido) |
| `private static BringBookingToStatus(status)` | `new BookingBuilder().As{Status}()` (método removido) |

### Arquivos de teste a atualizar (Bookings)

- `Unit/Domain/Entities/BookingTests.cs` — remover `CreatePendingBooking()` e `BringBookingToStatus()`
- `Unit/Domain/Entities/ProviderScheduleTests.cs`
- `Unit/Application/Handlers/CancelBookingCommandHandlerTests.cs`
- `Unit/Application/Handlers/CompleteBookingCommandHandlerTests.cs`
- `Unit/Application/Handlers/ConfirmBookingCommandHandlerTests.cs`
- `Unit/Application/Handlers/CreateBookingCommandHandlerTests.cs`
- `Unit/Application/Handlers/GetBookingByIdQueryHandlerTests.cs`
- `Unit/Application/Handlers/GetBookingsByClientQueryHandlerTests.cs`
- `Unit/Application/Handlers/GetBookingsByProviderQueryHandlerTests.cs`
- `Unit/Application/Handlers/GetProviderAvailabilityQueryHandlerTests.cs`
- `Unit/Application/Handlers/RejectBookingCommandHandlerTests.cs`
- `Unit/Application/Handlers/SetProviderScheduleCommandHandlerTests.cs`
- `Unit/Application/Services/TimeZoneResolverTests.cs`
- `Unit/Application/ModuleApi/BookingsModuleApiTests.cs`
- `Unit/Infrastructure/Services/DbContextBookingCommandServiceTests.cs`
- `Unit/Infrastructure/Queries/DbContextBookingQueriesTests.cs`
- `Unit/Infrastructure/Queries/DbContextProviderScheduleQueriesTests.cs`

---

## Fase 2 — Builders Novos: Communications (Piloto)

### EmailTemplateBuilder

**Arquivo:** `Builders/Modules/Communications/EmailTemplateBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Communications.Domain.Entities.EmailTemplate`
**Método de criação:** `EmailTemplate.Create(key, subject, htmlBody, textBody, language?, overrideKey?, isSystemTemplate?)`

```csharp
public class EmailTemplateBuilder : BaseBuilder<EmailTemplate>
{
    private string? _key;
    private string? _subject;
    private string? _htmlBody;
    private string? _textBody;
    private string? _language;
    private string? _overrideKey;
    private bool _isSystemTemplate;
    private bool _isActive = true;

    public EmailTemplateBuilder WithKey(string key);
    public EmailTemplateBuilder WithSubject(string subject);
    public EmailTemplateBuilder WithHtmlBody(string htmlBody);
    public EmailTemplateBuilder WithTextBody(string textBody);
    public EmailTemplateBuilder WithLanguage(string language);
    public EmailTemplateBuilder WithOverrideKey(string overrideKey);
    public EmailTemplateBuilder AsSystemTemplate();
    public EmailTemplateBuilder AsActive();
    public EmailTemplateBuilder AsInactive(); // Chama Deactivate() após criação
}
```

### CommunicationLogBuilder

**Arquivo:** `Builders/Modules/Communications/CommunicationLogBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Communications.Domain.Entities.CommunicationLog`
**Método de criação:** `CommunicationLog.CreateSuccess(...)` / `CommunicationLog.CreateFailure(...)`

```csharp
public class CommunicationLogBuilder : BaseBuilder<CommunicationLog>
{
    private string? _correlationId;
    private ECommunicationChannel? _channel;
    private string? _recipient;
    private bool _isSuccess = true;
    private string? _errorMessage;
    private int _attemptCount = 1;
    private Guid? _outboxMessageId;
    private string? _templateKey;

    public CommunicationLogBuilder WithCorrelationId(string correlationId);
    public CommunicationLogBuilder WithChannel(ECommunicationChannel channel);
    public CommunicationLogBuilder WithRecipient(string recipient);
    public CommunicationLogBuilder AsSuccess();
    public CommunicationLogBuilder AsFailure(string errorMessage);
    public CommunicationLogBuilder WithAttemptCount(int count);
    public CommunicationLogBuilder WithOutboxMessageId(Guid id);
    public CommunicationLogBuilder WithTemplateKey(string key);
}
```

### OutboxMessageBuilder

**Arquivo:** `Builders/Modules/Communications/OutboxMessageBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage`
**Método de criação:** `OutboxMessage.Create(channel, payload, priority?, scheduledAt?, maxRetries?, correlationId?)`

```csharp
public class OutboxMessageBuilder : BaseBuilder<OutboxMessage>
{
    private ECommunicationChannel? _channel;
    private string? _payload;
    private ECommunicationPriority? _priority;
    private DateTime? _scheduledAt;
    private int _maxRetries = 3;
    private string? _correlationId;

    public OutboxMessageBuilder WithChannel(ECommunicationChannel channel);
    public OutboxMessageBuilder WithPayload(string payload);
    public OutboxMessageBuilder WithMaxRetries(int maxRetries);
    public OutboxMessageBuilder AsScheduled(DateTime scheduledAt);
    public OutboxMessageBuilder WithCorrelationId(string correlationId);
}
```

### EmailOutboxPayloadBuilder

**Arquivo:** `Builders/Modules/Communications/EmailOutboxPayloadBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Communications.Application.DTOs.EmailOutboxPayload`
**Método de criação:** `EmailOutboxPayload.Create(to, subject, htmlBody?, textBody?, from?, templateKey?, templateData?)`

```csharp
public class EmailOutboxPayloadBuilder : BaseBuilder<EmailOutboxPayload>
{
    private string? _to;
    private string? _subject;
    private string? _htmlBody;
    private string? _textBody;
    private string? _from;
    private string? _templateKey;
    private IReadOnlyDictionary<string, string>? _templateData;

    public EmailOutboxPayloadBuilder WithTo(string to);
    public EmailOutboxPayloadBuilder WithSubject(string subject);
    public EmailOutboxPayloadBuilder WithHtmlBody(string htmlBody);
    public EmailOutboxPayloadBuilder AsTemplate(string templateKey, IDictionary<string, string>? data = null);
}
```

### SmsOutboxPayloadBuilder

**Arquivo:** `Builders/Modules/Communications/SmsOutboxPayloadBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Communications.Application.DTOs.SmsOutboxPayload`

```csharp
public class SmsOutboxPayloadBuilder : BaseBuilder<SmsOutboxPayload>
{
    private string? _phoneNumber;
    private string? _body;

    public SmsOutboxPayloadBuilder WithPhoneNumber(string phoneNumber);
    public SmsOutboxPayloadBuilder WithBody(string body);
}
```

### PushOutboxPayloadBuilder

**Arquivo:** `Builders/Modules/Communications/PushOutboxPayloadBuilder.cs`
**Entidade:** `MeAjudaAi.Modules.Communications.Application.DTOs.PushOutboxPayload`

```csharp
public class PushOutboxPayloadBuilder : BaseBuilder<PushOutboxPayload>
{
    private string? _deviceToken;
    private string? _title;
    private string? _body;
    private IDictionary<string, string>? _data;

    public PushOutboxPayloadBuilder WithDeviceToken(string token);
    public PushOutboxPayloadBuilder WithTitle(string title);
    public PushOutboxPayloadBuilder WithBody(string body);
}
```

### Substituições nos testes (~76 ocorrências)

| Padrão Atual | Substituição |
|-------------|-------------|
| `EmailTemplate.Create("key", "Sub", "Html", "Text")` | `new EmailTemplateBuilder().WithKey("key")...Build()` |
| `CommunicationLog.CreateSuccess(corrId, channel, recipient, 1)` | `new CommunicationLogBuilder().WithCorrelationId(corrId)...AsSuccess().Build()` |
| `CommunicationLog.CreateFailure(corrId, channel, recipient, "err", 1)` | `new CommunicationLogBuilder()...AsFailure("err").Build()` |
| `OutboxMessage.Create(ECommunicationChannel.Email, "payload")` | `new OutboxMessageBuilder().WithChannel(Email).WithPayload("payload").Build()` |
| `EmailOutboxPayload.Create(to: "x", subject: "y", htmlBody: "z")` | `new EmailOutboxPayloadBuilder().WithTo("x")...Build()` |
| `new SmsOutboxPayload("+55...", "Hello")` | `new SmsOutboxPayloadBuilder().WithPhoneNumber("+55...").WithBody("Hello").Build()` |
| `new PushOutboxPayload("token", "Title", "Body")` | `new PushOutboxPayloadBuilder().WithDeviceToken("token")...Build()` |

### Arquivos de teste a atualizar (Communications)

- `Unit/Domain/Entities/EmailTemplateTests.cs`
- `Unit/Domain/Entities/CommunicationLogTests.cs`
- `Unit/Domain/Entities/OutboxMessageTests.cs`
- `Unit/Application/Handlers/Commands/EmailTemplateCommandHandlerTests.cs`
- `Unit/Application/Handlers/Queries/GetAllEmailTemplatesHandlerTests.cs`
- `Unit/Application/Handlers/Queries/GetEmailTemplateByKeyHandlerTests.cs`
- `Unit/Application/Services/OutboxProcessorServiceTests.cs`
- `Unit/Application/ModuleApi/CommunicationsModuleApiTests.cs`
- `Unit/Infrastructure/Queries/DbContextEmailTemplateQueriesTests.cs`
- `Unit/Infrastructure/Queries/DbContextCommunicationLogQueriesTests.cs`
- `Unit/Infrastructure/Persistence/OutboxMessageRepositoryTests.cs`
- `Unit/TestInfrastructure/SerializerMockBuilder.cs` — reavaliar se ainda é necessário

---

## Fase 3 — Migração de Builders Existentes

### Migrar para Shared.Tests

| Builder | Origem | Destino |
|---------|--------|---------|
| `UserBuilder` | `src/Modules/Users/Tests/Builders/UserBuilder.cs` | `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/Users/UserBuilder.cs` |
| `UsernameBuilder` | `src/Modules/Users/Tests/Builders/UsernameBuilder.cs` | `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/Users/UsernameBuilder.cs` |
| `EmailBuilder` | `src/Modules/Users/Tests/Builders/EmailBuilder.cs` | `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/Users/EmailBuilder.cs` |
| `ProviderBuilder` | `src/Modules/Providers/Tests/Builders/ProviderBuilder.cs` | `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/Providers/ProviderBuilder.cs` |
| `ServiceBuilder` | `src/Modules/ServiceCatalogs/Tests/Builders/ServiceBuilder.cs` | `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/ServiceCatalogs/ServiceBuilder.cs` |
| `ServiceCategoryBuilder` | `src/Modules/ServiceCatalogs/Tests/Builders/ServiceCategoryBuilder.cs` | `tests/MeAjudaAi.Shared.Tests/TestInfrastructure/Builders/Modules/ServiceCatalogs/ServiceCategoryBuilder.cs` |

### Atualizações necessárias

1. **Namespace**: `MeAjudaAi.Modules.{Module}.Tests.Builders` → `MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.{Module}`
2. **Using statements**: Atualizar em todos os arquivos de teste que importam esses builders
3. **Remover** as pastas `Builders/` dos projetos de teste dos módulos (após migração)
4. **DocumentBuilder** (Documents): Não extende `BaseBuilder<T>` — manter como está ou padronizar (decisão pendente)

---

## Fase 4 — Validação e Limpeza

### Checklist de Validação

- [ ] Todos os testes unitários passam (Bookings: ~208, Communications: ~189, Users: ~748, Providers: ~657)
- [ ] Todos os testes E2E passam (189/189)
- [ ] Nenhuma ocorrência restante de criação inline não-builder nos módulos piloto
- [ ] Builders são usados em pelo menos 2 módulos diferentes (validação de reuso)
- [ ] `SerializerMockBuilder` reavaliado — remover se payload builders cobrirem uso
- [ ] `DocumentBuilder` padronizado (se decisão for sim)
- [ ] `using` statements limpos (sem imports não utilizados)
- [ ] Testes de integração em `tests/MeAjudaAi.Integration.Tests/` também usam builders quando aplicável

### Comandos de Validação

```bash
# Build
dotnet build

# Testes unitários por módulo
dotnet test src/Modules/Bookings/Tests/MeAjudaAi.Modules.Bookings.Tests.csproj
dotnet test src/Modules/Communications/Tests/MeAjudaAi.Modules.Communications.Tests.csproj
dotnet test src/Modules/Users/Tests/MeAjudaAi.Modules.Users.Tests.csproj
dotnet test src/Modules/Providers/Tests/MeAjudaAi.Modules.Providers.Tests.csproj

# Testes E2E
dotnet test tests/MeAjudaAi.E2E.Tests/MeAjudaAi.E2E.Tests.csproj

# Testes de integração
dotnet test tests/MeAjudaAi.Integration.Tests/MeAjudaAi.Integration.Tests.csproj
```

---

## Decisões Técnicas

| Decisão | Escolha | Justificativa |
|---------|---------|---------------|
| Localização dos builders | `Shared.Tests/TestInfrastructure/Builders/Modules/` | Acesso transitivo a todos os domínios; todos os módulos já referenciam Shared.Tests |
| Namespace | `MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.{Module}` | Consistente com `BaseBuilder<T>` existente |
| Estrutura interna | Flat por módulo (sem subpastas `Entities/`) | Simplicidade; subpastas podem ser adicionadas depois se surgirem builders de DTOs/Commands |
| Builder pattern | `BaseBuilder<T>` com `Faker<T>.CustomInstantiator` | Padrão existente em Users/Providers/ServiceCatalogs; garante validação do domínio |
| Estado de entidades | Via métodos fluentes (`AsConfirmed()`, `AsInactive()`) | Mais legível que parâmetros numéricos |
| Escopo piloto | Bookings + Communications | Módulos com mais criação inline; validam o padrão antes de migrar outros |

---

## Estimativa de Esforço

| Fase | Arquivos a Criar/Modificar | Ocorrências a Substituir | Esforço |
|------|---------------------------|-------------------------|---------|
| Fase 1 (Bookings builders) | 4 builders + ~17 test files | ~88 | Alto |
| Fase 2 (Communications builders) | 6 builders + ~12 test files | ~76 | Alto |
| Fase 3 (Migração existentes) | 6 builders movidos + ~20 test files (using updates) | 0 (mudança de namespace) | Médio |
| Fase 4 (Validação) | 0 | 0 | Baixo |
| **Total** | **16 builders + ~50 test files** | **~164** | **Alto** |

---

## Riscos e Mitigações

| Risco | Impacto | Mitigação |
|-------|---------|-----------|
| Builders quebram com mudança de domínio | Alto | Testes unitários dos próprios builders; `CustomInstantiator` usa factory methods do domínio |
| Namespace migration causa erros de compilação | Médio | Usar `using` aliases se necessário; migrar arquivo por arquivo com build entre cada um |
| Performance de testes com Bogus | Baixo | `BaseBuilder<T>` já é usado em produção de testes sem problemas |
| `DocumentBuilder` não padronizado | Baixo | Manter como está; padronizar futuramente se necessário |
