# 📅 Módulo Bookings - Sistema de Agendamentos (Planejado)

> **⚠️ Status**: Este módulo está **em planejamento** e será implementado após o módulo Services.

## 🎯 Visão Geral

O módulo Bookings será o coração do sistema de agendamentos da plataforma MeAjudaAi, responsável por conectar clientes e prestadores por meio de um sistema robusto de reservas e execução de serviços.

### **Responsabilidades Planejadas**
- 🔄 **Agendamento de serviços** entre clientes e prestadores
- 🔄 **Gestão de disponibilidade** dos prestadores
- 🔄 **Workflow de aprovação** (automática/manual)
- 🔄 **Acompanhamento de execução** dos serviços
- 🔄 **Sistema de avaliações** e feedback
- 🔄 **Gestão de cancelamentos** e reagendamentos

## 🏗️ Arquitetura Planejada

### **Domain Model (Conceitual)**

> **📝 Nota de Implementação**: Os exemplos de código abaixo mostram a estrutura conceitual dos agregados. Durante a implementação, é essencial incluir métodos de domínio que demonstrem como as coleções são gerenciadas e os invariantes são protegidos (ex: `AddBookingMessage()`, `ChangeStatus()`, `BlockSlot()`). Isso garante que os limites do agregado sejam respeitados e as regras de negócio sejam aplicadas consistentemente.

#### **Agregado Principal: Booking**
```csharp
/// <summary>
/// Agregado raiz para agendamentos de serviços
/// </summary>
public sealed class Booking : AggregateRoot<BookingId>
{
    public Guid CustomerId { get; private set; }        // Cliente
    public Guid ProviderId { get; private set; }        // Prestador
    public Guid ServiceId { get; private set; }         // Serviço solicitado
    
    public BookingDetails Details { get; private set; } // Detalhes do agendamento
    public BookingSchedule Schedule { get; private set; } // Horário agendado
    public ServiceLocation Location { get; private set; } // Local do serviço
    public BookingPricing Pricing { get; private set; }  // Valores acordados
    
    public EBookingStatus Status { get; private set; }   // Status atual
    public BookingWorkflow Workflow { get; private set; } // Fluxo de aprovação
    
    // Histórico e acompanhamento
    public IReadOnlyCollection<BookingStatusChange> StatusHistory { get; }
    public IReadOnlyCollection<BookingMessage> Messages { get; }
    public BookingExecution? Execution { get; private set; }
    public BookingReview? Review { get; private set; }
}
```

#### **Agregado: ProviderSchedule**
```csharp
/// <summary>
/// Agenda e disponibilidade do prestador
/// </summary>
public sealed class ProviderSchedule : AggregateRoot<ProviderScheduleId>
{
    public Guid ProviderId { get; private set; }
    public ScheduleSettings Settings { get; private set; }
    
    // Disponibilidade
    public IReadOnlyCollection<AvailabilitySlot> AvailableSlots { get; }
    public IReadOnlyCollection<BlockedPeriod> BlockedPeriods { get; }
    public IReadOnlyCollection<RecurringAvailability> RecurringSchedule { get; }
    
    // Reservas
    public IReadOnlyCollection<BookingReservation> Reservations { get; }
}
```

### **Value Objects Planejados**

#### **BookingDetails**
```csharp
public class BookingDetails : ValueObject
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string? SpecialRequirements { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public BookingPriority Priority { get; private set; }
    public bool RequiresApproval { get; private set; }
}
```

#### **BookingSchedule**
```csharp
public class BookingSchedule : ValueObject
{
    public DateTime RequestedStartTime { get; private set; }
    public DateTime RequestedEndTime { get; private set; }
    public DateTime? ConfirmedStartTime { get; private set; }
    public DateTime? ConfirmedEndTime { get; private set; }
    public TimeZoneInfo TimeZone { get; private set; }
    public bool IsFlexible { get; private set; }
    public TimeSpan? FlexibilityWindow { get; private set; }
}
```

#### **ServiceLocation**
```csharp
public class ServiceLocation : ValueObject
{
    public EServiceLocationType Type { get; private set; } // OnSite, Remote, ProviderLocation
    public Address? ServiceAddress { get; private set; }
    public string? AccessInstructions { get; private set; }
    public GeoLocation? Coordinates { get; private set; }
    public string? RemoteConnectionDetails { get; private set; }
}
```

#### **BookingPricing**
```csharp
public class BookingPricing : ValueObject
{
    public decimal ServiceBasePrice { get; private set; }
    public decimal? NegotiatedPrice { get; private set; }
    public IReadOnlyList<PriceAdjustment> Adjustments { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string Currency { get; private set; }
    public EPricingStatus Status { get; private set; }
}
```

#### **BookingExecution**
```csharp
public class BookingExecution : ValueObject
{
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    public TimeSpan? ActualDuration { get; private set; }
    public string? ExecutionNotes { get; private set; }
    public IReadOnlyList<ExecutionCheckpoint> Checkpoints { get; private set; }
    public IReadOnlyList<string> CompletionPhotos { get; private set; }
    public EExecutionStatus Status { get; private set; }
}
```

### **Enumerações Planejadas**

#### **EBookingStatus**
```csharp
public enum EBookingStatus
{
    Draft = 0,           // Rascunho
    Requested = 1,       // Solicitado
    PendingApproval = 2, // Aguardando aprovação
    Confirmed = 3,       // Confirmado
    InProgress = 4,      // Em execução
    Completed = 5,       // Concluído
    Cancelled = 6,       // Cancelado
    Rejected = 7,        // Rejeitado
    Rescheduled = 8,     // Reagendado
    NoShow = 9           // Não comparecimento
}
```

#### **EServiceLocationType**
```csharp
public enum EServiceLocationType
{
    OnSite = 0,          // No local do cliente
    Remote = 1,          // Remoto/online
    ProviderLocation = 2, // Local do prestador
    Flexible = 3         // Flexível (a combinar)
}
```

#### **EBookingPriority**
```csharp
public enum EBookingPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
```

## 🔄 Domain Events Planejados

### **Estratégia de Versionamento e Rastreabilidade**

Os eventos do módulo Bookings implementarão versionamento semântico e IDs de correlação para rastreabilidade cross-module:

```csharp
public abstract record BookingDomainEvent
{
    public string EventVersion { get; init; } = "1.0.0";
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public string OriginModule { get; init; } = "Bookings";
    public DateTime EventTimestamp { get; init; } = SystemTime.UtcNow;
    public Dictionary<string, string> Metadata { get; init; } = new();
}

// Exemplo de implementação com versionamento
public record BookingRequestedDomainEvent : BookingDomainEvent
{
    public Guid BookingId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProviderId { get; init; }
    public DateTime RequestedTime { get; init; }
    
    // Metadados para correlação
    public Guid? ParentWorkflowId { get; init; }
    public string? SourceChannel { get; init; } // web, mobile, api
}
```

### **Eventos de Booking**
```csharp
// Ciclo de vida do agendamento
public record BookingRequestedDomainEvent(Guid BookingId, Guid CustomerId, Guid ProviderId, DateTime RequestedTime);
public record BookingConfirmedDomainEvent(Guid BookingId, DateTime ConfirmedStartTime, DateTime ConfirmedEndTime);
public record BookingStartedDomainEvent(Guid BookingId, DateTime ActualStartTime);
public record BookingCompletedDomainEvent(Guid BookingId, DateTime ActualEndTime, TimeSpan ActualDuration);
public record BookingCancelledDomainEvent(Guid BookingId, string Reason, Guid CancelledBy, DateTime CancelledAt);

// Eventos de comunicação
public record BookingMessageSentDomainEvent(Guid BookingId, Guid SenderId, string Message);
public record BookingRescheduledDomainEvent(Guid BookingId, DateTime OldTime, DateTime NewTime, Guid RequestedBy);

// Eventos de avaliação
public record BookingReviewSubmittedDomainEvent(Guid BookingId, Guid ReviewerId, int Rating, string? Comment);
```

### **Eventos de Schedule**
```csharp
public record ProviderAvailabilityUpdatedDomainEvent(Guid ProviderId, DateTime StartDate, DateTime EndDate);
public record AvailabilitySlotBlockedDomainEvent(Guid ProviderId, DateTime StartTime, DateTime EndTime, string Reason);
public record RecurringScheduleUpdatedDomainEvent(Guid ProviderId, ScheduleSettings NewSettings);
```

## ⚡ CQRS Planejado

### **Commands**
#### **Booking Management**
- 🔄 **CreateBookingCommand**: Criar agendamento
- 🔄 **ConfirmBookingCommand**: Confirmar agendamento
- 🔄 **StartBookingCommand**: Iniciar execução
- 🔄 **CompleteBookingCommand**: Finalizar serviço
- 🔄 **CancelBookingCommand**: Cancelar agendamento
- 🔄 **RescheduleBookingCommand**: Reagendar
- 🔄 **UpdateBookingPricingCommand**: Atualizar preços

#### **Schedule Management**
- 🔄 **UpdateProviderScheduleCommand**: Atualizar agenda
- 🔄 **BlockAvailabilitySlotCommand**: Bloquear horário
- 🔄 **SetRecurringAvailabilityCommand**: Configurar recorrência

#### **Communication**
- 🔄 **SendBookingMessageCommand**: Enviar mensagem
- 🔄 **SubmitBookingReviewCommand**: Avaliar serviço

### **Queries**
#### **Booking Queries**
- 🔄 **GetBookingByIdQuery**: Buscar agendamento
- 🔄 **GetBookingsByCustomerQuery**: Agendamentos do cliente
- 🔄 **GetBookingsByProviderQuery**: Agendamentos do prestador
- 🔄 **GetBookingsByStatusQuery**: Filtrar por status
- 🔄 **GetBookingHistoryQuery**: Histórico completo

#### **Schedule Queries**
- 🔄 **GetProviderAvailabilityQuery**: Disponibilidade do prestador
- 🔄 **FindAvailableSlotsQuery**: Encontrar horários livres
- 🔄 **GetProviderScheduleQuery**: Agenda completa
- 🔄 **CheckSlotAvailabilityQuery**: Verificar disponibilidade

#### **Analytics Queries**
- 🔄 **GetBookingStatisticsQuery**: Estatísticas de agendamentos
- 🔄 **GetProviderPerformanceQuery**: Desempenho do prestador
- 🔄 **GetPopularTimeSlotsQuery**: Horários mais populares

## 🌐 API Endpoints Planejados

### **Booking Endpoints**
```http
# Gestão de agendamentos
POST   /api/v1/bookings                    # Criar agendamento
GET    /api/v1/bookings                    # Listar agendamentos (filtros)
GET    /api/v1/bookings/{id}               # Obter agendamento
PUT    /api/v1/bookings/{id}               # Atualizar agendamento
DELETE /api/v1/bookings/{id}               # Cancelar agendamento

# Ações específicas
POST   /api/v1/bookings/{id}/confirm       # Confirmar agendamento
POST   /api/v1/bookings/{id}/start         # Iniciar serviço
POST   /api/v1/bookings/{id}/complete      # Finalizar serviço
POST   /api/v1/bookings/{id}/reschedule    # Reagendar
POST   /api/v1/bookings/{id}/cancel        # Cancelar

# Comunicação
GET    /api/v1/bookings/{id}/messages      # Mensagens do agendamento
POST   /api/v1/bookings/{id}/messages      # Enviar mensagem
POST   /api/v1/bookings/{id}/review        # Avaliar serviço
```

### **Schedule Endpoints**
```http
# Disponibilidade
GET    /api/v1/providers/{id}/availability      # Ver disponibilidade
PUT    /api/v1/providers/{id}/availability      # Atualizar disponibilidade
GET    /api/v1/providers/{id}/schedule          # Agenda completa
PUT    /api/v1/providers/{id}/schedule          # Configurar agenda

# Busca de horários
GET    /api/v1/availability/search              # Buscar horários disponíveis
GET    /api/v1/providers/{id}/slots/{date}      # Slots de um dia específico
POST   /api/v1/providers/{id}/slots/block       # Bloquear horário
```

## 🔌 Module API Planejada

### **Interface IBookingsModuleApi**
```csharp
public interface IBookingsModuleApi : IModuleApi
{
    // Booking operations
    Task<Result<ModuleBookingDto?>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleBookingBasicDto>>> GetBookingsByProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleBookingBasicDto>>> GetBookingsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Availability operations
    Task<Result<IReadOnlyList<AvailableSlotDto>>> GetProviderAvailabilityAsync(Guid providerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsSlotAvailableAsync(Guid providerId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<Result<ProviderBookingStatsDto>> GetProviderBookingStatsAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Result<bool>> BookingExistsAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
```

### **DTOs Planejados**
```csharp
public sealed record ModuleBookingDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid ProviderId { get; init; }
    public required Guid ServiceId { get; init; }
    public required string ServiceName { get; init; }
    public required DateTime ScheduledStartTime { get; init; }
    public required DateTime ScheduledEndTime { get; init; }
    public required EBookingStatus Status { get; init; }
    public required decimal TotalPrice { get; init; }
    public required string Currency { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed record ModuleBookingBasicDto
{
    public required Guid Id { get; init; }
    public required string ServiceName { get; init; }
    public required DateTime ScheduledStartTime { get; init; }
    public required EBookingStatus Status { get; init; }
    public required decimal TotalPrice { get; init; }
}

public sealed record AvailableSlotDto
{
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required bool IsBlocked { get; init; }
    public required string? BlockReason { get; init; }
}
```

## 🗄️ Schema de Banco Planejado

### **Tabelas Principais**
```sql
-- Agendamentos
CREATE TABLE bookings.Bookings (
    Id uuid PRIMARY KEY,
    CustomerId uuid NOT NULL, -- FK to Users
    ProviderId uuid NOT NULL, -- FK to Providers
    ServiceId uuid NOT NULL, -- FK to Services
    
    -- Detalhes do agendamento
    Title varchar(200) NOT NULL,
    Description text,
    SpecialRequirements text,
    EstimatedDurationMinutes int NOT NULL,
    Priority int NOT NULL DEFAULT 1,
    RequiresApproval boolean NOT NULL DEFAULT false,
    
    -- Horários
    RequestedStartTime timestamp NOT NULL,
    RequestedEndTime timestamp NOT NULL,
    ConfirmedStartTime timestamp,
    ConfirmedEndTime timestamp,
    TimeZone varchar(50) NOT NULL,
    IsFlexible boolean NOT NULL DEFAULT false,
    FlexibilityWindowMinutes int,
    
    -- Local do serviço
    LocationType int NOT NULL, -- EServiceLocationType
    ServiceAddress_Street varchar(200),
    ServiceAddress_Number varchar(20),
    ServiceAddress_City varchar(100),
    ServiceAddress_State varchar(50),
    ServiceAddress_ZipCode varchar(20),
    ServiceAddress_Country varchar(100),
    AccessInstructions text,
    RemoteConnectionDetails text,
    
    -- Preços
    ServiceBasePrice decimal(10,2) NOT NULL,
    NegotiatedPrice decimal(10,2),
    TotalPrice decimal(10,2) NOT NULL,
    Currency varchar(3) NOT NULL DEFAULT 'BRL',
    PricingStatus int NOT NULL DEFAULT 0,
    
    -- Status e controle
    Status int NOT NULL DEFAULT 1, -- EBookingStatus
    
    -- Execução
    ActualStartTime timestamp,
    ActualEndTime timestamp,
    ExecutionNotes text,
    ExecutionStatus int,
    
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    UpdatedAt timestamp,
    
    CONSTRAINT fk_bookings_customer FOREIGN KEY (CustomerId) REFERENCES users.Users(Id),
    CONSTRAINT fk_bookings_provider FOREIGN KEY (ProviderId) REFERENCES providers.Providers(Id)
);

-- Agenda dos prestadores
CREATE TABLE bookings.ProviderSchedules (
    Id uuid PRIMARY KEY,
    ProviderId uuid NOT NULL UNIQUE,
    
    -- Configurações gerais
    TimeZone varchar(50) NOT NULL,
    BookingWindow_MinHours int NOT NULL DEFAULT 24, -- Antecedência mínima
    BookingWindow_MaxDays int NOT NULL DEFAULT 30,  -- Prazo máximo
    AutoConfirm boolean NOT NULL DEFAULT false,
    
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    UpdatedAt timestamp,
    
    CONSTRAINT fk_schedules_provider FOREIGN KEY (ProviderId) REFERENCES providers.Providers(Id)
);

-- Disponibilidade recorrente
CREATE TABLE bookings.RecurringAvailability (
    Id uuid PRIMARY KEY,
    ProviderScheduleId uuid NOT NULL,
    
    DayOfWeek int NOT NULL, -- 0=Sunday, 1=Monday, etc.
    StartTime time NOT NULL,
    EndTime time NOT NULL,
    IsAvailable boolean NOT NULL DEFAULT true,
    
    CONSTRAINT fk_recurring_schedule FOREIGN KEY (ProviderScheduleId) REFERENCES bookings.ProviderSchedules(Id)
);

-- Bloqueios específicos
CREATE TABLE bookings.BlockedPeriods (
    Id uuid PRIMARY KEY,
    ProviderScheduleId uuid NOT NULL,
    
    StartDateTime timestamp NOT NULL,
    EndDateTime timestamp NOT NULL,
    Reason varchar(500),
    IsRecurring boolean NOT NULL DEFAULT false,
    
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_blocked_schedule FOREIGN KEY (ProviderScheduleId) REFERENCES bookings.ProviderSchedules(Id)
);

-- Histórico de mudanças de status
CREATE TABLE bookings.BookingStatusHistory (
    Id uuid PRIMARY KEY,
    BookingId uuid NOT NULL,
    
    FromStatus int,
    ToStatus int NOT NULL,
    Reason varchar(500),
    ChangedBy uuid NOT NULL, -- FK to Users
    ChangedAt timestamp NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_status_booking FOREIGN KEY (BookingId) REFERENCES bookings.Bookings(Id),
    CONSTRAINT fk_status_user FOREIGN KEY (ChangedBy) REFERENCES users.Users(Id)
);

-- Mensagens do agendamento
CREATE TABLE bookings.BookingMessages (
    Id uuid PRIMARY KEY,
    BookingId uuid NOT NULL,
    
    SenderId uuid NOT NULL, -- FK to Users
    Message text NOT NULL,
    MessageType int NOT NULL DEFAULT 0, -- Text, System, Attachment
    
    SentAt timestamp NOT NULL DEFAULT NOW(),
    ReadAt timestamp,
    
    CONSTRAINT fk_messages_booking FOREIGN KEY (BookingId) REFERENCES bookings.Bookings(Id),
    CONSTRAINT fk_messages_sender FOREIGN KEY (SenderId) REFERENCES users.Users(Id)
);

-- Avaliações
CREATE TABLE bookings.BookingReviews (
    Id uuid PRIMARY KEY,
    BookingId uuid NOT NULL UNIQUE,
    
    ReviewerId uuid NOT NULL, -- Quem avalia (customer ou provider)
    RevieweeId uuid NOT NULL, -- Quem é avaliado
    
    Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment text,
    
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_reviews_booking FOREIGN KEY (BookingId) REFERENCES bookings.Bookings(Id),
    CONSTRAINT fk_reviews_reviewer FOREIGN KEY (ReviewerId) REFERENCES users.Users(Id),
    CONSTRAINT fk_reviews_reviewee FOREIGN KEY (RevieweeId) REFERENCES users.Users(Id)
);
```

### **Índices para Desempenho**
```sql
-- Índices para consultas frequentes
CREATE INDEX idx_bookings_customer ON bookings.Bookings(CustomerId, Status);
CREATE INDEX idx_bookings_provider ON bookings.Bookings(ProviderId, Status);
CREATE INDEX idx_bookings_service ON bookings.Bookings(ServiceId);
CREATE INDEX idx_bookings_schedule ON bookings.Bookings(RequestedStartTime, RequestedEndTime);
CREATE INDEX idx_bookings_status ON bookings.Bookings(Status, CreatedAt);

-- Índices para disponibilidade
CREATE INDEX idx_recurring_provider_day ON bookings.RecurringAvailability(ProviderScheduleId, DayOfWeek);
CREATE INDEX idx_blocked_provider_period ON bookings.BlockedPeriods(ProviderScheduleId, StartDateTime, EndDateTime);

-- Índices para mensagens e histórico
CREATE INDEX idx_messages_booking ON bookings.BookingMessages(BookingId, SentAt);
CREATE INDEX idx_status_history ON bookings.BookingStatusHistory(BookingId, ChangedAt);
```

### **Estratégia de Retenção de Dados e Conformidade LGPD**

#### **Políticas de Retenção**
```sql
-- Configuração de retenção por tipo de dado
CREATE TABLE bookings.DataRetentionPolicies (
    TableName varchar(100) PRIMARY KEY,
    RetentionPeriodMonths int NOT NULL,
    ArchivalRequired boolean NOT NULL DEFAULT false,
    AnonymizationFields text[], -- Campos que devem ser anonimizados
    PurgeAfterMonths int, -- Exclusão definitiva após arquivamento
    
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    UpdatedAt timestamp NOT NULL DEFAULT NOW()
);

-- Inserir políticas padrão
INSERT INTO bookings.DataRetentionPolicies VALUES 
('Bookings', 36, true, '{"SpecialInstructions", "Location"}', 84),
('BookingMessages', 24, false, '{"Message"}', 36),
('BookingStatusHistory', 60, true, '{}', 120),
('BookingReviews', 60, true, '{"Comment"}', NULL); -- Reviews mantidas indefinidamente (anonimizadas)
```

#### **Jobs de Arquivamento e LGPD**
```csharp
public class BookingDataRetentionService
{
    public async Task ExecuteRetentionPolicy()
    {
        // 1. Arquivar dados antigos
        await ArchiveExpiredBookings();
        
        // 2. Anonimizar dados sensíveis
        await AnonymizePersonalData();
        
        // 3. Processar solicitações LGPD
        await ProcessDataSubjectRequests();
    }
    
    private async Task AnonymizePersonalData()
    {
        // BookingMessages após 2 anos
        await _dbContext.BookingMessages
            .Where(m => m.SentAt < DateTime.UtcNow.AddMonths(-24))
            .UpdateAsync(m => new BookingMessage 
            { 
                Message = "[ANONIMIZADO]",
                UpdatedAt = DateTime.UtcNow 
            });
            
        // Campos sensíveis em Bookings após 3 anos
        await _dbContext.Bookings
            .Where(b => b.CreatedAt < DateTime.UtcNow.AddMonths(-36))
            .UpdateAsync(b => new Booking 
            { 
                SpecialInstructions = "[ANONIMIZADO]",
                Location = "[ANONIMIZADO]"
            });
    }
}
```

#### **Conformidade com Direitos do Titular**
```csharp
public interface IBookingLgpdService
{
    // Art. 18 - Direito de confirmação e acesso
    Task<BookingPersonalDataReport> GetPersonalDataReportAsync(Guid userId);
    
    // Art. 18 - Direito de correção
    Task<Result> CorrectPersonalDataAsync(Guid userId, BookingDataCorrectionRequest request);
    
    // Art. 18 - Direito de eliminação
    Task<Result> DeletePersonalDataAsync(Guid userId, string legalBasis);
    
    // Art. 18 - Direito de anonimização
    Task<Result> AnonymizePersonalDataAsync(Guid userId);
}
```

## 🔗 Integração com Outros Módulos

### **Dependências**
```csharp
// Booking usa informações de múltiplos módulos
public class Booking : AggregateRoot<BookingId>
{
    public Guid CustomerId { get; private set; }  // Users module
    public Guid ProviderId { get; private set; }  // Providers module
    public Guid ServiceId { get; private set; }   // Services module
}

// Domain Services que integram com outros módulos
public interface IBookingValidationDomainService
{
    Task<Result<bool>> ValidateBookingRequest(CreateBookingRequest request);
    Task<Result<bool>> ValidateProviderAvailability(Guid providerId, DateTime startTime, DateTime endTime);
    Task<Result<ServicePricingInfo>> GetServicePricing(Guid serviceId);
}
```

### **Event Integration**
```csharp
// Listening to events from other modules
public class ProviderVerificationStatusHandler : INotificationHandler<ProviderVerificationStatusUpdatedDomainEvent>
{
    public async Task Handle(ProviderVerificationStatusUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Se provider foi suspenso, cancelar bookings futuros
        if (notification.NewStatus == EVerificationStatus.Suspended)
        {
            await CancelFutureBookingsForProvider(notification.AggregateId);
        }
    }
}
```

## � Transações Distribuídas e Fluxos de Compensação

### **Padrão Saga para Workflows Cross-Module**

O módulo Bookings implementará **Saga Pattern** para coordenar transações distribuídas que envolvem múltiplos módulos (Users, Providers, Services). Utilizamos uma abordagem **híbrida** combinando orquestração para workflows críticos e coreografia para eventos de notificação.

#### **Outbox Pattern para Entrega Confiável**
```csharp
// Implementação do Outbox Pattern
public class BookingOutboxEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? FailureReason { get; set; }
}

// Handler idempotente com retry
public class BookingEventProcessor
{
    public async Task<Result> ProcessOutboxEvents()
    {
        var pendingEvents = await _outboxRepository.GetPendingEventsAsync();
        
        foreach (var outboxEvent in pendingEvents)
        {
            try
            {
                await ProcessEventWithRetry(outboxEvent);
                await _outboxRepository.MarkAsProcessedAsync(outboxEvent.Id);
            }
            catch (Exception ex)
            {
                await HandleEventFailure(outboxEvent, ex);
            }
        }
    }
    
    private async Task ProcessEventWithRetry(BookingOutboxEvent outboxEvent)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff
                
        await retryPolicy.ExecuteAsync(async () =>
        {
            await _eventBus.PublishAsync(DeserializeEvent(outboxEvent));
        });
    }
}
```

### **Cenários de Compensação Específicos**

#### **1. Cancelamento de Reserva com Reembolso**

**Sequência de Comandos/Eventos:**
```csharp
// 1. Comando inicial
public record CancelBookingWithRefundCommand(Guid BookingId, string Reason);

// 2. Sequência de eventos coordenada
public class BookingCancellationSaga
{
    public async Task Handle(CancelBookingWithRefundCommand command)
    {
        var sagaId = Guid.NewGuid();
        
        // Step 1: Cancelar booking
        var cancelResult = await CancelBooking(command.BookingId, command.Reason);
        if (cancelResult.IsFailure)
        {
            await PublishSagaFailedEvent(sagaId, "BookingCancellation", cancelResult.Error);
            return;
        }
        
        // Step 2: Liberar slot no Provider Schedule
        var releaseSlotCmd = new ReleaseProviderSlotCommand(
            booking.ProviderId, booking.ScheduledTime, booking.Duration);
        var releaseResult = await _providerModuleApi.ReleaseSlotAsync(releaseSlotCmd);
        
        if (releaseResult.IsFailure)
        {
            // Compensação: Reverter cancelamento
            await CompensateBookingCancellation(command.BookingId);
            return;
        }
        
        // Step 3: Processar reembolso
        var refundCmd = new ProcessRefundCommand(booking.PaymentId, booking.Amount);
        var refundResult = await _paymentModuleApi.ProcessRefundAsync(refundCmd);
        
        if (refundResult.IsFailure)
        {
            // Compensação: Re-bloquear slot + reverter cancelamento
            await CompensateSlotRelease(booking.ProviderId, booking.ScheduledTime);
            await CompensateBookingCancellation(command.BookingId);
            return;
        }
        
        // Sucesso: Publicar evento final
        await PublishBookingCancelledWithRefundEvent(sagaId, command.BookingId);
    }
}
```

**Invariantes de Domínio:**
- Booking só pode ser cancelado se status for `Confirmed` ou `Pending`
- Reembolso só processa se não exceder janela de cancelamento (24h)
- Slot só é liberado após confirmação de cancelamento no módulo Bookings

**Tratamento de Falhas:**
```csharp
public class BookingCancellationCompensationHandler
{
    public async Task CompensateBookingCancellation(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking != null && booking.Status == BookingStatus.Cancelled)
        {
            booking.Reinstate("Compensação de saga falhada");
            await _bookingRepository.UpdateAsync(booking);
            
            // Log para monitoramento
            _logger.LogWarning("Booking {BookingId} reinstated due to saga compensation", bookingId);
        }
    }
}
```

#### **2. Suspensão de Prestador Afetando Reservas**

**Sequência de Eventos:**
```csharp
// Evento originado no módulo Providers
public record ProviderSuspendedDomainEvent(Guid ProviderId, string Reason, DateTime SuspendedAt);

// Handler no módulo Bookings
public class ProviderSuspensionHandler : INotificationHandler<ProviderSuspendedDomainEvent>
{
    public async Task Handle(ProviderSuspendedDomainEvent notification)
    {
        var sagaId = Guid.NewGuid();
        
        // 1. Buscar todas as reservas futuras do prestador
        var futureBookings = await _bookingRepository
            .GetFutureBookingsByProviderAsync(notification.ProviderId);
        
        foreach (var booking in futureBookings)
        {
            // 2. Cancelar reserva individual
            var cancelResult = await CancelBookingDueToProviderIssue(booking.Id, notification.Reason);
            
            if (cancelResult.IsSuccess)
            {
                // 3. Processar reembolso automático
                await ProcessAutomaticRefund(booking.PaymentId, booking.Amount);
                
                // 4. Notificar cliente
                await NotifyCustomerOfCancellation(booking.CustomerId, booking.Id, notification.Reason);
            }
            else
            {
                // Marcar para retry manual
                await _deadLetterQueue.AddFailedBookingCancellation(booking.Id, cancelResult.Error);
            }
        }
        
        await PublishProviderSuspensionProcessedEvent(sagaId, notification.ProviderId, futureBookings.Count);
    }
}
```

#### **3. Remoção de Serviço Afetando Agendamentos**

**Compensação Coordenada:**
```csharp
public class ServiceRemovalSaga
{
    public async Task Handle(ServiceRemovedDomainEvent serviceRemovedEvent)
    {
        var sagaId = Guid.NewGuid();
        var affectedBookings = new List<Booking>();
        
        try
        {
            // 1. Identificar bookings afetados
            affectedBookings = await _bookingRepository
                .GetBookingsByServiceAsync(serviceRemovedEvent.ServiceId);
            
            // 2. Para cada booking, executar compensação
            foreach (var booking in affectedBookings)
            {
                // 2a. Cancelar booking
                booking.CancelDueToServiceUnavailability("Serviço removido da plataforma");
                
                // 2b. Liberar slot do prestador
                await _providerModuleApi.ReleaseSlotAsync(
                    booking.ProviderId, booking.ScheduledTime, booking.Duration);
                
                // 2c. Processar reembolso
                await ProcessFullRefund(booking.PaymentId, booking.Amount);
                
                // 2d. Registrar no histórico
                await RecordServiceRemovalImpact(booking.Id, serviceRemovedEvent.ServiceId);
            }
            
            await PublishServiceRemovalCompensationCompletedEvent(sagaId, affectedBookings.Count);
        }
        catch (Exception ex)
        {
            // Rollback parcial se necessário
            await HandleServiceRemovalSagaFailure(sagaId, affectedBookings, ex);
        }
    }
}
```

### **Monitoramento e Observabilidade**

#### **Dead Letter Queue Management**
```csharp
public class BookingSagaMonitoringService
{
    public async Task ProcessDeadLetterEvents()
    {
        var deadLetterEvents = await _deadLetterRepository.GetUnprocessedEventsAsync();
        
        foreach (var deadEvent in deadLetterEvents)
        {
            // Análise automática do erro
            var errorCategory = CategorizeSagaError(deadEvent.FailureReason);
            
            switch (errorCategory)
            {
                case SagaErrorCategory.TransientFailure:
                    await RetryWithBackoff(deadEvent);
                    break;
                case SagaErrorCategory.BusinessRuleViolation:
                    await TriggerManualReview(deadEvent);
                    break;
                case SagaErrorCategory.ExternalServiceUnavailable:
                    await ScheduleDelayedRetry(deadEvent, TimeSpan.FromMinutes(30));
                    break;
            }
        }
    }
}
```

#### **Reconciliation Jobs**
```csharp
public class BookingSagaReconciliationJob
{
    public async Task ReconcileIncompleteSagas()
    {
        // 1. Identificar sagas órfãs (iniciadas há mais de 1 hora sem finalização)
        var orphanedSagas = await _sagaRepository.GetOrphanedSagasAsync(TimeSpan.FromHours(1));
        
        foreach (var saga in orphanedSagas)
        {
            // 2. Verificar estado atual dos recursos
            var reconciliationResult = await ReconcileSagaState(saga);
            
            if (reconciliationResult.RequiresCompensation)
            {
                await TriggerCompensationWorkflow(saga);
            }
            else if (reconciliationResult.CanComplete)
            {
                await CompleteSaga(saga);
            }
            else
            {
                await MarkSagaForManualIntervention(saga);
            }
        }
    }
}
```

#### **Métricas de Observabilidade**
```csharp
public class BookingSagaMetrics
{
    private readonly IMetrics _metrics;
    
    public void RecordSagaStarted(string sagaType)
    {
        _metrics.IncrementCounter("booking_saga_started", new[] { ("type", sagaType) });
    }
    
    public void RecordSagaCompleted(string sagaType, TimeSpan duration)
    {
        _metrics.RecordHistogram("booking_saga_duration", duration.TotalMilliseconds, 
            new[] { ("type", sagaType), ("status", "completed") });
    }
    
    public void RecordSagaFailed(string sagaType, string errorCategory)
    {
        _metrics.IncrementCounter("booking_saga_failed", 
            new[] { ("type", sagaType), ("error_category", errorCategory) });
    }
    
    public void RecordCompensationTriggered(string sagaType, string compensationAction)
    {
        _metrics.IncrementCounter("booking_saga_compensation", 
            new[] { ("type", sagaType), ("action", compensationAction) });
    }
}
```

## �📊 Business Rules e Validações

### **Regras de Agendamento**
1. **Antecedência Mínima**: Não permitir agendamentos com menos de X horas de antecedência
2. **Prazo Máximo**: Limitar agendamentos a X dias no futuro
3. **Sobreposição**: Não permitir agendamentos sobrepostos para o mesmo prestador
4. **Horário Comercial**: Respeitar horários de funcionamento do prestador
5. **Provider Verification**: Só prestadores verificados podem receber agendamentos

### **Regras de Cancelamento**
1. **Prazo de Cancelamento**: Diferentes prazos conforme proximidade do agendamento
2. **Política de Reembolso**: Baseada no prazo de cancelamento
3. **Penalidades**: Para cancelamentos recorrentes ou em cima da hora
4. **Reagendamento**: Limites de reagendamentos por booking

### **Regras de Preço**
1. **Preço Base**: Definido pelo serviço
2. **Ajustes**: Por urgência, horário, localização
3. **Negociação**: Possibilidade de preço negociado
4. **Taxa da Plataforma**: Percentual sobre o valor do serviço

## 🚀 Recursos Avançados Planejados

### **Smart Scheduling**
- 🔄 **Sugestão inteligente** de horários baseada em padrões
- 🔄 **Auto-agendamento** para serviços recorrentes
- 🔄 **Otimização de rota** para prestadores com múltiplos agendamentos
- 🔄 **Previsão de demanda** por horário e região

### **Communication Hub**
- 🔄 **Chat em tempo real** durante execução do serviço
- 🔄 **Notificações automáticas** (SMS, email, push)
- 🔄 **Status tracking** em tempo real
- 🔄 **Photo sharing** para validação de execução

### **Analytics & Intelligence**
- 🔄 **Métricas de desempenho** do prestador
- 🔄 **Customer behavior** analysis
- 🔄 **Peak time** identification
- 🔄 **Revenue optimization** suggestions

## 🧪 Estratégia de Testes

### **Testes de Domain Logic**
- ✅ **Booking State Machine**: Transições de status válidas
- ✅ **Schedule Validation**: Conflitos e disponibilidade
- ✅ **Pricing Calculation**: Cálculos corretos de preços
- ✅ **Business Rules**: Todas as regras de negócio

### **Testes de Integração**
- ✅ **End-to-End Booking Flow**: Fluxo completo de agendamento
- ✅ **Module Communication**: Integração com Users, Providers, Services
- ✅ **Event Handling**: Processamento de eventos de outros módulos
- ✅ **External Services**: Notificações, pagamentos

### **Testes de Desempenho**
- ✅ **Availability Search**: Desempenho com grandes volumes
- ✅ **Concurrent Bookings**: Operação de reservas simultâneas
- ✅ **Schedule Queries**: Otimização de consultas de agenda
- ✅ **Real-time Updates**: Desempenho de atualizações em tempo real

### **Testes de Chaos Engineering**
- ✅ **Double Booking Prevention**: Cenários de conflito
- ✅ **Provider Unavailability**: Operação em caso de indisponibilidade súbita
- ✅ **Network Partitions**: Resiliência a falhas de rede
- ✅ **Data Consistency**: Consistência em cenários de falha

## 📈 Métricas e KPIs

### **Business Metrics**
- **Booking Conversion Rate**: Taxa de conversão de solicitações para confirmações
- **Average Booking Value**: Valor médio por agendamento
- **Provider Utilization Rate**: Taxa de utilização da agenda dos prestadores
- **Customer Satisfaction Score**: Baseado nas avaliações
- **Cancellation Rate**: Taxa de cancelamentos por tipo

### **Operational Metrics**
- **Response Time**: Tempo de resposta para confirmações
- **System Availability**: Uptime do sistema de agendamentos
- **Peak Load Handling**: Desempenho em horários de pico
- **Data Consistency**: Métricas de consistência de dados

## 📋 Roadmap de Implementação

### **Fase 1: Core Booking System (Q1 2026)**
- 🔄 Agregados principais (Booking, ProviderSchedule)
- 🔄 CRUD básico de agendamentos
- 🔄 Sistema básico de disponibilidade
- 🔄 Estados e transições fundamentais

### **Fase 2: Advanced Scheduling (Q2 2026)**
- 🔄 Disponibilidade recorrente
- 🔄 Bloqueios e exceções
- 🔄 Busca inteligente de horários
- 🔄 Validações de conflito

### **Fase 3: Communication & Workflow (Q3 2026)**
- 🔄 Sistema de mensagens
- 🔄 Workflow de aprovação
- 🔄 Notificações automáticas
- 🔄 Tracking de execução

### **Fase 4: Intelligence & Optimization (Q4 2026)**
- 🔄 Analytics avançado
- 🔄 Otimização de rotas
- 🔄 Predição de demanda
- 🔄 Auto-scheduling

## 🚨 Considerações de Segurança

### **Data Protection**
- **Gerenciamento de PII**: Proteção de dados pessoais
- **Payment Security**: Integração segura com gateways
- **Location Privacy**: Proteção de dados de localização
- **Communication Privacy**: Criptografia de mensagens

### **Access Control**
- **Role-based Access**: Diferentes níveis de acesso
- **Booking Ownership**: Apenas donos podem modificar
- **Provider Boundaries**: Prestadores só veem seus agendamentos
- **Admin Controls**: Ferramentas administrativas seguras

### **Audit & Compliance**
- **Full Audit Trail**: Registro completo de mudanças
- **LGPD Compliance**: Conformidade com lei de proteção de dados
- **Data Retention**: Políticas de retenção de dados
- **Right to Deletion**: Capacidade de deletar dados pessoais

---

## 📚 Referências para Implementação

- **[Módulo Services](./services.md)** - Integração com catálogo de serviços
- **[Módulo Providers](./providers.md)** - Integração com prestadores
- **[Módulo Users](./users.md)** - Base de clientes
- **[Patterns](../patterns/)** - Padrões de design para sistemas complexos

---

*📅 Planejamento: Novembro 2025*  
*🎯 Implementação prevista: Q1-Q4 2026*  
*✨ Documentação mantida pela equipe de desenvolvimento MeAjudaAi*