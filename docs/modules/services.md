# 📋 Módulo Services - Catálogo de Serviços (Planejado)

> **⚠️ Status**: Este módulo está **em planejamento** e será implementado na próxima fase do projeto.

## 🎯 Visão Geral

O módulo Services será responsável pelo **catálogo de serviços** oferecidos pelos prestadores na plataforma MeAjudaAi, implementando um Bounded Context dedicado para gestão de serviços e categorização.

### **Responsabilidades Planejadas**
- 🔄 **Catálogo de serviços** hierárquico por categorias
- 🔄 **Gestão de preços** e modelos de precificação
- 🔄 **Disponibilidade** e configuração de horários
- 🔄 **Duração** e estimativas de tempo
- 🔄 **Requisitos** e pré-condições
- 🔄 **Avaliações** e feedback dos serviços

## 🏗️ Arquitetura Planejada

### **Domain Model (Conceitual)**

#### **Agregado Principal: Service**
```csharp
/// <summary>
/// Agregado raiz para serviços oferecidos
/// </summary>
public sealed class Service : AggregateRoot<ServiceId>
{
    public Guid ProviderId { get; private set; }        // Prestador responsável
    public string Name { get; private set; }            // Nome do serviço
    public string Description { get; private set; }     // Descrição detalhada
    public CategoryId CategoryId { get; private set; }  // Categoria do serviço
    public PricingModel Pricing { get; private set; }   // Modelo de precificação
    public ServiceDuration Duration { get; private set; } // Duração estimada
    public ServiceArea ServiceArea { get; private set; } // Área de atendimento
    public ServiceStatus Status { get; private set; }   // Status do serviço
    
    // Coleções
    public IReadOnlyCollection<ServiceRequirement> Requirements { get; }
    public IReadOnlyCollection<ServiceReview> Reviews { get; }
}
```

#### **Agregado: Category**
```csharp
/// <summary>
/// Categoria hierárquica de serviços
/// </summary>
public sealed class Category : AggregateRoot<CategoryId>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string IconUrl { get; private set; }
    public CategoryId? ParentCategoryId { get; private set; }
    public int SortOrder { get; private set; }
    
    // Navegação hierárquica
    public IReadOnlyCollection<Category> SubCategories { get; }
}
```

### **Value Objects Planejados**

#### **PricingModel**
```csharp
public class PricingModel : ValueObject
{
    public EPricingType Type { get; private set; }      // Fixed, Hourly, Custom
    public decimal BasePrice { get; private set; }
    public decimal? MinPrice { get; private set; }
    public decimal? MaxPrice { get; private set; }
    public string Currency { get; private set; }
    public IReadOnlyList<PriceModifier> Modifiers { get; private set; }
}
```

#### **ServiceDuration**
```csharp
public class ServiceDuration : ValueObject
{
    public TimeSpan EstimatedDuration { get; private set; }
    public TimeSpan? MinDuration { get; private set; }
    public TimeSpan? MaxDuration { get; private set; }
    public EDurationType Type { get; private set; }     // Fixed, Variable, Negotiable
}
```

#### **ServiceArea**
```csharp
public class ServiceArea : ValueObject
{
    public EServiceAreaType Type { get; private set; }  // OnSite, Remote, Both
    public decimal? MaxRadius { get; private set; }     // Raio máximo (km)
    public IReadOnlyList<string> SupportedCities { get; private set; }
    public IReadOnlyList<string> SupportedStates { get; private set; }
}
```

### **Enumerações Planejadas**

#### **EPricingType**
```csharp
public enum EPricingType
{
    Fixed = 0,        // Preço fixo
    Hourly = 1,       // Por hora
    Daily = 2,        // Por dia
    PerItem = 3,      // Por item/unidade
    Custom = 4,       // Negociável
    Package = 5       // Pacote de serviços
}
```

#### **EServiceStatus**
```csharp
public enum EServiceStatus
{
    Draft = 0,        // Rascunho
    Active = 1,       // Ativo
    Inactive = 2,     // Inativo
    Suspended = 3,    // Suspenso
    UnderReview = 4   // Em análise
}
```

#### **EDurationType**
```csharp
public enum EDurationType
{
    Fixed = 0,        // Duração fixa
    Variable = 1,     // Duração variável
    Negotiable = 2,   // Negociável
    Depends = 3       // Depende do escopo
}
```

## 🔄 Domain Events Planejados

```csharp
// Eventos de serviços
public record ServiceCreatedDomainEvent(Guid ServiceId, Guid ProviderId, string Name);
public record ServicePriceUpdatedDomainEvent(Guid ServiceId, PricingModel OldPricing, PricingModel NewPricing);
public record ServiceActivatedDomainEvent(Guid ServiceId, DateTime ActivatedAt);
public record ServiceDeactivatedDomainEvent(Guid ServiceId, string Reason);

// Eventos de categorias
public record CategoryCreatedDomainEvent(Guid CategoryId, string Name, Guid? ParentId);
public record CategoryReorganizedDomainEvent(Guid CategoryId, Guid? OldParentId, Guid? NewParentId);
```

## ⚡ CQRS Planejado

### **Commands**
- 🔄 **CreateServiceCommand**: Criar novo serviço
- 🔄 **UpdateServiceCommand**: Atualizar serviço
- 🔄 **UpdateServicePricingCommand**: Atualizar preços
- 🔄 **ActivateServiceCommand**: Ativar serviço
- 🔄 **DeactivateServiceCommand**: Desativar serviço
- 🔄 **CreateCategoryCommand**: Criar categoria
- 🔄 **ReorganizeCategoryCommand**: Reorganizar hierarquia

### **Queries**
- 🔄 **GetServiceByIdQuery**: Buscar serviço por ID
- 🔄 **GetServicesByProviderQuery**: Serviços de um prestador
- 🔄 **GetServicesByCategoryQuery**: Serviços por categoria
- 🔄 **SearchServicesQuery**: Busca com filtros
- 🔄 **GetCategoriesTreeQuery**: Árvore de categorias
- 🔄 **GetPopularServicesQuery**: Serviços populares

## 🌐 API Endpoints Planejados

### **Serviços**
- 🔄 `POST /api/v1/services` - Criar serviço
- 🔄 `GET /api/v1/services` - Listar serviços (com filtros)
- 🔄 `GET /api/v1/services/{id}` - Obter serviço
- 🔄 `PUT /api/v1/services/{id}` - Atualizar serviço
- 🔄 `DELETE /api/v1/services/{id}` - Excluir serviço
- 🔄 `GET /api/v1/services/search` - Buscar serviços
- 🔄 `GET /api/v1/providers/{providerId}/services` - Serviços do prestador

### **Categorias**
- 🔄 `GET /api/v1/categories` - Listar categorias
- 🔄 `GET /api/v1/categories/tree` - Árvore hierárquica
- 🔄 `GET /api/v1/categories/{id}/services` - Serviços da categoria
- 🔄 `POST /api/v1/categories` - Criar categoria (admin)
- 🔄 `PUT /api/v1/categories/{id}` - Atualizar categoria (admin)

## 🔌 Module API Planejada

### **Interface IServicesModuleApi**
```csharp
public interface IServicesModuleApi : IModuleApi
{
    Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleServiceBasicDto>>> GetServicesByProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ModuleServiceBasicDto>>> SearchServicesAsync(SearchServicesRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ServiceExistsAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<Result<ModuleCategoryDto?>> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
```

### **DTOs Planejados**
```csharp
public sealed record ModuleServiceDto
{
    public required Guid Id { get; init; }
    public required Guid ProviderId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string CategoryName { get; init; }
    public required decimal BasePrice { get; init; }
    public required string Currency { get; init; }
    public required TimeSpan EstimatedDuration { get; init; }
    public required bool IsActive { get; init; }
}

public sealed record ModuleServiceBasicDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal BasePrice { get; init; }
    public required string Currency { get; init; }
    public required bool IsActive { get; init; }
}
```

## 🗄️ Schema de Banco Planejado

### **Tabelas Principais**
```sql
-- Categorias hierárquicas
CREATE TABLE services.Categories (
    Id uuid PRIMARY KEY,
    Name varchar(200) NOT NULL,
    Description text,
    IconUrl varchar(500),
    ParentCategoryId uuid REFERENCES services.Categories(Id),
    SortOrder int NOT NULL DEFAULT 0,
    IsActive boolean NOT NULL DEFAULT true,
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    UpdatedAt timestamp NULL
);

-- Serviços
CREATE TABLE services.Services (
    Id uuid PRIMARY KEY,
    ProviderId uuid NOT NULL, -- Referência ao Providers module
    CategoryId uuid NOT NULL REFERENCES services.Categories(Id),
    Name varchar(200) NOT NULL,
    Description text NOT NULL,
    
    -- Pricing
    PricingType int NOT NULL, -- EPricingType
    BasePrice decimal(10,2) NOT NULL,
    MinPrice decimal(10,2),
    MaxPrice decimal(10,2),
    Currency varchar(3) NOT NULL DEFAULT 'BRL',
    
    -- Duration
    EstimatedDurationMinutes int NOT NULL,
    MinDurationMinutes int,
    MaxDurationMinutes int,
    DurationType int NOT NULL, -- EDurationType
    
    -- Service Area
    ServiceAreaType int NOT NULL, -- EServiceAreaType
    MaxRadiusKm decimal(8,2),
    
    Status int NOT NULL DEFAULT 0, -- EServiceStatus
    IsDeleted boolean NOT NULL DEFAULT false,
    DeletedAt timestamp NULL,
    CreatedAt timestamp NOT NULL DEFAULT NOW(),
    UpdatedAt timestamp NULL
);

-- Requisitos de serviço
CREATE TABLE services.ServiceRequirements (
    Id uuid PRIMARY KEY,
    ServiceId uuid NOT NULL REFERENCES services.Services(Id),
    Name varchar(200) NOT NULL,
    Description text,
    IsRequired boolean NOT NULL DEFAULT true,
    SortOrder int NOT NULL DEFAULT 0
);

-- Modificadores de preço
CREATE TABLE services.PriceModifiers (
    Id uuid PRIMARY KEY,
    ServiceId uuid NOT NULL REFERENCES services.Services(Id),
    Name varchar(200) NOT NULL,
    Type int NOT NULL, -- EPriceModifierType
    Value decimal(10,2) NOT NULL,
    Unit varchar(50) -- '%', 'fixed', 'per_hour', etc.
);
```

## 🔗 Integração com Outros Módulos

### **Com Módulo Providers**
```csharp
// Services referencia Providers
public class Service : AggregateRoot<ServiceId>
{
    public Guid ProviderId { get; private set; }  // FK para Provider
    // Validação: só prestadores verificados podem criar serviços
}
```

### **Com Módulo Bookings (Futuro)**
```csharp
// Bookings usa Services para agendamentos
public class Booking : AggregateRoot<BookingId>
{
    public Guid ServiceId { get; private set; }   // FK para Service
    public Guid ProviderId { get; private set; }  // FK para Provider
    public Guid CustomerId { get; private set; }  // FK para User
}
```

## 📊 Métricas Planejadas

### **Business Metrics**
- **services_by_category**: Distribuição de serviços por categoria
- **average_service_price**: Preço médio por categoria
- **service_creation_rate**: Taxa de criação de novos serviços
- **popular_categories**: Categorias mais procuradas

### **Technical Metrics**
- **service_search_desempenho**: Desempenho de buscas
- **category_tree_load_time**: Tempo de carregamento da árvore
- **service_availability_uptime**: Disponibilidade dos serviços

## 🧪 Estratégia de Testes Planejada

### **Testes de Domínio**
- ✅ **Agregado Service**: Validações de negócio
- ✅ **Value Objects**: PricingModel, ServiceDuration, ServiceArea
- ✅ **Domain Events**: Criação, atualização, ativação

### **Testes de Integração**
- ✅ **API Endpoints**: CRUD completo de serviços
- ✅ **Search Engine**: Busca e filtros avançados
- ✅ **Module API**: Comunicação com outros módulos

### **Testes de Desempenho**
- ✅ **Busca de serviços**: Desempenho com grandes volumes
- ✅ **Árvore de categorias**: Carregamento eficiente
- ✅ **Filtros complexos**: Otimização de queries

## 🚀 Roadmap de Implementação

### **Fase 1: Core Services**
- 🔄 Criar estrutura básica do módulo
- 🔄 Implementar agregado Service
- 🔄 CRUD básico de serviços
- 🔄 Integração com Providers

### **Fase 2: Categorização**
- 🔄 Implementar sistema de categorias
- 🔄 Árvore hierárquica
- 🔄 Interface de administração

### **Fase 3: Search & Filters**
- 🔄 Sistema de busca avançada
- 🔄 Filtros por preço, localização, duração
- 🔄 Elasticsearch integration (opcional)

### **Fase 4: Advanced Features**
- 🔄 Sistema de avaliações
- 🔄 Recomendações inteligentes
- 🔄 Analytics e métricas avançadas

## 📋 Dependências e Pré-requisitos

### **Módulos Necessários**
- ✅ **Users**: Já implementado
- ✅ **Providers**: Já implementado
- 🔄 **Shared**: Extensões para search e caching

### **Infraestrutura**
- 🔄 **Search Engine**: Elasticsearch ou alternativa
- 🔄 **Cache**: Redis para consultas frequentes
- 🔄 **Image Storage**: Para ícones de categorias

## 📚 Referências para Implementação

- **[Módulo Providers](./providers.md)** - Integração com prestadores
- **[Módulo Users](./users.md)** - Base de usuários
- **[Arquitetura](../architecture.md)** - Padrões e estrutura
- **[Search Patterns](../patterns/search-patterns.md)** - Padrões de busca (a criar)

---

## 📝 Notas de Implementação

### **Decisões Técnicas Pendentes**
1. **Search Engine**: Elasticsearch vs. PostgreSQL Full-Text Search
2. **Categorization**: Hierarquia vs. Tags vs. Ambos
3. **Pricing**: Flexibilidade vs. Simplicidade
4. **Caching Strategy**: Níveis e invalidação

### **Considerações de Desempenho**
- **Indexação eficiente** para buscas
- **Cache de categorias** (raramente mudam)
- **Pagination** para grandes volumes
- **Lazy loading** de relacionamentos

---

*📅 Planejamento: Novembro 2025*  
*🎯 Implementação prevista: Q1 2026*  
*✨ Documentação mantida pela equipe de desenvolvimento MeAjudaAi*