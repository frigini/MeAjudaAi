# 🔧 Módulo Providers - Prestadores de Serviços

Este documento detalha a implementação completa do módulo Providers, responsável pela gestão de prestadores de serviços na plataforma MeAjudaAi.

## 🎯 Visão Geral

O módulo Providers implementa um **Bounded Context** dedicado para gestão de prestadores de serviços, seguindo os princípios de **Domain-Driven Design (DDD)** e **Clean Architecture**.

### **Responsabilidades Principais**
- ✅ **Registro de prestadores** (Individual ou Company)
- ✅ **Gestão de perfil empresarial** (razão social, contato, endereço)
- ✅ **Verificação e documentação** (CPF, CNPJ, certificações)
- ✅ **Qualificações profissionais** (cursos, habilitações)
- ✅ **Status de verificação** (Pending, Verified, Rejected, etc.)
- ✅ **Soft delete** e gestão de lifecycle

## 🏗️ Arquitetura do Módulo

### **Estrutura de Pastas**
```
src/Modules/Providers/
├── API/                           # Camada de apresentação (endpoints)
│   ├── Endpoints/                 # Minimal APIs organizados por contexto
│   │   └── ProviderAdmin/         # Endpoints administrativos
│   ├── Extensions.cs              # Registro de serviços
│   └── Mappers/                   # Mapeamento entre camadas
├── Application/                   # Camada de aplicação (CQRS)
│   ├── Commands/                  # Commands para modificações
│   ├── Queries/                   # Queries para consultas
│   ├── Handlers/                  # Handlers para Commands/Queries
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Services/                  # Module API e serviços de aplicação
│   └── Mappers/                   # Mapeadores para DTOs
├── Domain/                        # Camada de domínio (regras de negócio)
│   ├── Entities/                  # Agregados e entidades
│   ├── ValueObjects/              # Value Objects
│   ├── Events/                    # Domain Events
│   ├── Enums/                     # Enumerações de domínio
│   ├── Exceptions/                # Exceções específicas do domínio
│   └── Repositories/              # Interfaces de repositório
├── Infrastructure/                # Camada de infraestrutura
│   ├── Persistence/               # Entity Framework e configurações
│   │   ├── Configurations/        # Configurações de entidade
│   │   └── Migrations/            # Migrações de banco
│   └── Repositories/              # Implementações de repositório
└── Tests/                         # Testes unitários do módulo
    ├── Unit/                      # Testes unitários por camada
    │   ├── Domain/                # Testes de entidades e value objects
    │   ├── Application/           # Testes de handlers e services
    │   └── Infrastructure/        # Testes de repositórios
    └── Builders/                  # Test builders para criação de objetos
```

## 🎭 Domain Model

### **Agregado Principal: Provider**

```csharp
/// <summary>
/// Agregado raiz para gestão de prestadores de serviços
/// </summary>
public sealed class Provider : AggregateRoot<ProviderId>
{
    public Guid UserId { get; private set; }                    // Referência ao usuário
    public string Name { get; private set; }                    // Nome do prestador
    public EProviderType Type { get; private set; }             // Individual ou Company
    public BusinessProfile BusinessProfile { get; private set; } // Perfil empresarial
    public EVerificationStatus VerificationStatus { get; private set; } // Status de verificação
    
    // Coleções gerenciadas
    public IReadOnlyCollection<Document> Documents { get; }
    public IReadOnlyCollection<Qualification> Qualifications { get; }
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
}
```

### **Value Objects Principais**

#### **BusinessProfile**
```csharp
public class BusinessProfile : ValueObject
{
    public string LegalName { get; private set; }           // Razão social
    public string? FantasyName { get; private set; }        // Nome fantasia
    public string? Description { get; private set; }        // Descrição
    public ContactInfo ContactInfo { get; private set; }    // Informações de contato
    public Address PrimaryAddress { get; private set; }     // Endereço principal
}
```

#### **ContactInfo**
```csharp
public class ContactInfo : ValueObject
{
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Website { get; private set; }
}
```

#### **Address**
```csharp
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
}
```

#### **Document**
```csharp
public class Document : ValueObject
{
    public string Number { get; private set; }
    public EDocumentType DocumentType { get; private set; }
}
```

#### **Qualification**
```csharp
public class Qualification : ValueObject
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? IssuingOrganization { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? DocumentNumber { get; private set; }
}
```

### **Enumerações**

#### **EProviderType**
```csharp
public enum EProviderType
{
    Individual = 0,  // Pessoa física
    Company = 1      // Pessoa jurídica
}
```

#### **EVerificationStatus**
```csharp
public enum EVerificationStatus
{
    None = 0,
    Pending = 1,      // Aguardando verificação
    InProgress = 2,   // Em processo de verificação
    Verified = 3,     // Verificado
    Rejected = 4,     // Rejeitado
    Suspended = 5     // Suspenso
}
```

#### **EDocumentType**
```csharp
public enum EDocumentType
{
    Cpf = 0,
    Cnpj = 1,
    Rg = 2,
    Cnh = 3,
    Certificate = 4,
    Other = 5
}
```

## 🔄 Domain Events

### **Eventos Implementados**
- ✅ **ProviderRegisteredDomainEvent**: Novo prestador registrado
- ✅ **ProviderDocumentAddedDomainEvent**: Documento adicionado
- ✅ **ProviderQualificationRemovedDomainEvent**: Qualificação removida
- ✅ **ProviderDeletedDomainEvent**: Prestador excluído (soft delete)

```csharp
public sealed record ProviderRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    Guid UserId,
    string Name,
    EProviderType Type,
    string Email
) : DomainEvent(AggregateId, Version);
```

## ⚡ CQRS Implementation

### **Commands Implementados**

#### **CreateProviderCommand**
```csharp
public sealed record CreateProviderCommand(
    Guid UserId,
    string Name,
    EProviderType Type,
    BusinessProfileDto BusinessProfile
) : ICommand<Result<Guid>>;
```

#### **UpdateProviderCommand**
```csharp
public sealed record UpdateProviderCommand(
    Guid ProviderId,
    string Name,
    BusinessProfileDto BusinessProfile
) : ICommand<Result>;
```

#### **DeleteProviderCommand**
```csharp
public sealed record DeleteProviderCommand(Guid ProviderId) : ICommand<Result>;
```

### **Queries Implementadas**

#### **GetProviderByIdQuery**
```csharp
public sealed record GetProviderByIdQuery(Guid ProviderId) : IQuery<Result<ProviderDto?>>;
```

#### **GetProvidersByTypeQuery**
```csharp
public sealed record GetProvidersByTypeQuery(EProviderType Type) : IQuery<Result<IReadOnlyList<ProviderDto>>>;
```

#### **GetProvidersByVerificationStatusQuery**
```csharp
public sealed record GetProvidersByVerificationStatusQuery(
    EVerificationStatus Status
) : IQuery<Result<IReadOnlyList<ProviderDto>>>;
```

## 🌐 API Endpoints

### **Endpoints Administrativos**
- ✅ `POST /api/v1/providers` - Criar prestador
- ✅ `GET /api/v1/providers` - Listar prestadores
- ✅ `GET /api/v1/providers/{id}` - Obter prestador por ID
- ✅ `PUT /api/v1/providers/{id}` - Atualizar prestador
- ✅ `DELETE /api/v1/providers/{id}` - Excluir prestador (soft delete)
- ✅ `GET /api/v1/providers/by-type/{type}` - Filtrar por tipo
- ✅ `GET /api/v1/providers/by-verification-status/{status}` - Filtrar por status

### **Exemplo de Uso da API**

#### **Criar Prestador**
```http
POST /api/v1/providers
Content-Type: application/json

{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "João Silva",
  "type": 0,
  "businessProfile": {
    "legalName": "João Silva ME",
    "fantasyName": "JS Serviços",
    "description": "Prestador de serviços domésticos",
    "contactInfo": {
      "email": "joao@exemplo.com",
      "phoneNumber": "+55 11 99999-9999",
      "website": "https://jsservicos.com"
    },
    "primaryAddress": {
      "street": "Rua das Flores",
      "number": "123",
      "complement": "Apt 45",
      "neighborhood": "Centro",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01234-567",
      "country": "Brasil"
    }
  }
}
```

#### **Resposta de Sucesso**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "data": {
    "id": "987fcdeb-51a2-43d1-b456-789012345678",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "name": "João Silva",
    "type": "Individual",
    "verificationStatus": "Pending",
    "businessProfile": {
      "legalName": "João Silva ME",
      "fantasyName": "JS Serviços",
      "description": "Prestador de serviços domésticos",
      "contactInfo": {
        "email": "joao@exemplo.com",
        "phoneNumber": "+55 11 99999-9999",
        "website": "https://jsservicos.com"
      },
      "primaryAddress": {
        "street": "Rua das Flores",
        "number": "123",
        "complement": "Apt 45",
        "neighborhood": "Centro",
        "city": "São Paulo",
        "state": "SP",
        "zipCode": "01234-567",
        "country": "Brasil"
      }
    },
    "documents": [],
    "qualifications": [],
    "createdAt": "2024-11-04T10:30:00Z",
    "updatedAt": null
  }
}
```

## 🔌 Module API

O módulo expõe uma **Module API** para comunicação type-safe com outros módulos:

### **Interface IProvidersModuleApi**
```csharp
public interface IProvidersModuleApi : IModuleApi
{
    Task<Result<ModuleProviderDto?>> GetProviderByIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Result<ModuleProviderDto?>> GetProviderByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleProviderBasicDto>>> GetProvidersBatchAsync(IReadOnlyList<Guid> providerIds, CancellationToken cancellationToken = default);
    Task<Result<bool>> ProviderExistsAsync(Guid providerId, CancellationToken cancellationToken = default);
}
```

### **DTOs para Module API**
```csharp
public sealed record ModuleProviderDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Document { get; init; }
    public required string? Phone { get; init; }
    public required EProviderType ProviderType { get; init; }
    public required EVerificationStatus VerificationStatus { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required bool IsActive { get; init; }
}
```

## 🗄️ Persistência

### **Configuração Entity Framework**
```csharp
public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers", "providers");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new ProviderId(value));
            
        // Configuração do Value Object BusinessProfile
        builder.OwnsOne(p => p.BusinessProfile, bp =>
        {
            bp.Property(b => b.LegalName).HasMaxLength(200).IsRequired();
            bp.Property(b => b.FantasyName).HasMaxLength(200);
            bp.Property(b => b.Description).HasMaxLength(1000);
            
            // Configuração do ContactInfo
            bp.OwnsOne(b => b.ContactInfo, ci =>
            {
                ci.Property(c => c.Email).HasMaxLength(255).IsRequired();
                ci.Property(c => c.PhoneNumber).HasMaxLength(20);
                ci.Property(c => c.Website).HasMaxLength(500);
            });
            
            // Configuração do Address
            bp.OwnsOne(b => b.PrimaryAddress, addr =>
            {
                addr.Property(a => a.Street).HasMaxLength(200).IsRequired();
                addr.Property(a => a.Number).HasMaxLength(20).IsRequired();
                addr.Property(a => a.Complement).HasMaxLength(100);
                addr.Property(a => a.Neighborhood).HasMaxLength(100).IsRequired();
                addr.Property(a => a.City).HasMaxLength(100).IsRequired();
                addr.Property(a => a.State).HasMaxLength(50).IsRequired();
                addr.Property(a => a.ZipCode).HasMaxLength(20).IsRequired();
                addr.Property(a => a.Country).HasMaxLength(100).IsRequired();
            });
        });
        
        // Configuração das coleções
        builder.OwnsMany(p => p.Documents, d =>
        {
            d.Property(doc => doc.Number).HasMaxLength(50).IsRequired();
            d.Property(doc => doc.DocumentType).HasConversion<int>().IsRequired();
        });
        
        builder.OwnsMany(p => p.Qualifications, q =>
        {
            q.Property(qual => qual.Name).HasMaxLength(200).IsRequired();
            q.Property(qual => qual.Description).HasMaxLength(1000);
            q.Property(qual => qual.IssuingOrganization).HasMaxLength(200);
            q.Property(qual => qual.DocumentNumber).HasMaxLength(100);
        });
    }
}
```

## 🧪 Estratégia de Testes

### **Cobertura de Testes**
- ✅ **Testes Unitários**: 95%+ de cobertura em Domain/Application
- ✅ **Testes de Integração**: Endpoints completos com banco real
- ✅ **Testes Arquiteturais**: Validação de dependências e padrões
- ✅ **Test Builders**: Criação facilitada de objetos para testes

### **Estrutura de Testes**
```
Tests/
├── Unit/
│   ├── Domain/
│   │   ├── Entities/ProviderTests.cs
│   │   ├── ValueObjects/BusinessProfileTests.cs
│   │   └── Events/ProviderRegisteredDomainEventTests.cs
│   ├── Application/
│   │   ├── Commands/CreateProviderCommandHandlerTests.cs
│   │   ├── Queries/GetProviderByIdQueryHandlerTests.cs
│   │   └── Services/ProvidersModuleApiTests.cs
│   └── Infrastructure/
│       └── Repositories/ProvidersRepositoryTests.cs
└── Builders/
    ├── ProviderBuilder.cs
    ├── BusinessProfileBuilder.cs
    └── DocumentBuilder.cs
```

### **Exemplo de Test Builder**
```csharp
public class ProviderBuilder : BuilderBase<Provider>
{
    public static ProviderBuilder Create() => new();
    
    public ProviderBuilder WithUserId(Guid userId) { /* ... */ }
    public ProviderBuilder WithName(string name) { /* ... */ }
    public ProviderBuilder AsIndividual() { /* ... */ }
    public ProviderBuilder AsCompany() { /* ... */ }
    public ProviderBuilder WithDocument(string number, EDocumentType type) { /* ... */ }
    
    public override Provider Build()
    {
        // Criação do Provider com valores padrão inteligentes
    }
}
```

## 📊 Métricas e Observabilidade

### **Logs Estruturados**
```csharp
[LoggerMessage(
    EventId = 2001,
    Level = LogLevel.Information,
    Message = "Provider {ProviderId} registered successfully (UserId: {UserId}, Type: {Type})")]
public static partial void ProviderRegistered(
    this ILogger logger, Guid providerId, Guid userId, EProviderType type);
```

### **Métricas Personalizadas**
- **provider_registrations_total**: Total de registros de prestadores
- **provider_verification_duration_ms**: Tempo de verificação
- **active_providers_by_type**: Prestadores ativos por tipo
- **verification_status_distribution**: Distribuição por status

## 🔧 Configuração e Registro

### **Registro no DI Container**
```csharp
public static class ProvidersModuleServiceCollectionExtensions
{
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ProvidersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Providers")));

        // Repositórios
        services.AddScoped<IProvidersRepository, ProvidersRepository>();

        // Handlers CQRS (registrados via Scrutor em ModuleExtensions)
        // Validators
        services.AddValidatorsFromAssembly(typeof(CreateProviderCommandValidator).Assembly);

        // Module API
        services.AddScoped<IProvidersModuleApi, ProvidersModuleApi>();

        return services;
    }
}
```

## 🚀 Próximos Passos

### **Funcionalidades Planejadas**
- 🔄 **Gestão de verificação avançada** (workflow de aprovação)
- 🔄 **Upload e gestão de documentos** (integração com storage)
- 🔄 **Geolocalização** (busca por proximidade)
- 🔄 **Avaliações e reviews** (sistema de reputação)
- 🔄 **Integração com serviços** (catálogo de serviços oferecidos)

### **Melhorias Técnicas**
- 🔄 **Cache distribuído** para consultas frequentes
- 🔄 **Event Sourcing** para auditoria completa
- 🔄 **Processamento em background** para verificações automáticas
- 🔄 **Notificações** (email/SMS para mudanças de status)

---

## 📚 Referências

- **[Arquitetura Geral](../architecture.md)** - Padrões e estrutura da aplicação
- **[Guia de Desenvolvimento](../development.md)** - Setup e diretrizes
- **[Módulo Users](./users.md)** - Integração com gestão de usuários
- **[Débito Técnico](../technical-debt.md)** - Itens pendentes e melhorias

---

*📅 Última atualização: Novembro 2025*  
*✨ Documentação mantida pela equipe de desenvolvimento MeAjudaAi*