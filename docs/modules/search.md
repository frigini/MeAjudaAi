# 🔍 Módulo Search - Busca Geoespacial de Prestadores

Este documento detalha a implementação completa do módulo Search, responsável pela busca geoespacial de prestadores de serviços na plataforma MeAjudaAi.

## 🎯 Visão Geral

O módulo Search implementa um **read model otimizado** para buscas geoespaciais de prestadores, utilizando **PostGIS** para queries eficientes baseadas em localização. Segue os princípios de **Domain-Driven Design (DDD)** e **Clean Architecture**.

### **Responsabilidades Principais**
- ✅ **Busca por proximidade** (raio de distância em quilômetros)
- ✅ **Filtros avançados** (serviços, avaliação mínima, tier de assinatura)
- ✅ **Ranking inteligente** (tier > rating > distância)
- ✅ **Paginação eficiente** com contagem total
- ✅ **Cache otimizado** para queries frequentes
- ✅ **Read model denormalizado** para performance

## 🏗️ Arquitetura do Módulo

### **Estrutura de Pastas**
```
src/Modules/Search/
├── API/                           # Camada de apresentação (endpoints)
│   └── Endpoints/                 # Minimal APIs
│       └── SearchProvidersEndpoint.cs
├── Application/                   # Camada de aplicação (CQRS)
│   ├── Queries/                   # SearchProvidersQuery
│   ├── Handlers/                  # SearchProvidersQueryHandler
│   ├── Validators/                # FluentValidation
│   └── DTOs/                      # Data Transfer Objects
├── Domain/                        # Camada de domínio
│   ├── Entities/                  # SearchableProvider (aggregate)
│   ├── ValueObjects/              # SearchResult, SearchableProviderId
│   ├── Enums/                     # ESubscriptionTier
│   └── Repositories/              # ISearchableProviderRepository
├── Infrastructure/                # Camada de infraestrutura
│   ├── Persistence/               # Entity Framework + PostGIS
│   │   ├── Configurations/        # EF Core entity configurations
│   │   ├── Migrations/            # Database migrations
│   │   └── Repositories/          # SearchableProviderRepository
│   └── Extensions.cs              # DI registration
└── Tests/                         # Testes do módulo
    ├── Unit/                      # Testes unitários
    │   ├── Domain/                # Entidades e value objects
    │   ├── Application/           # Handlers e validators
    └── Integration/               # Testes com Testcontainers + PostGIS
        └── SearchableProviderRepositoryIntegrationTests.cs
```

### **Padrão CQRS para Leitura**
O módulo implementa apenas o lado de **Query** do CQRS, pois é um read model:
- **Query**: `SearchProvidersQuery` com validação de parâmetros
- **Handler**: `SearchProvidersQueryHandler` executa busca geoespacial
- **Repository**: `SearchableProviderRepository` com PostGIS

### **Agregado Principal**

#### **SearchableProvider**
Read model denormalizado com dados necessários para busca:
- **Dados de identificação**: ProviderId (referência), Name
- **Geolocalização**: Location (GeoPoint com latitude/longitude)
- **Métricas**: AverageRating, TotalReviews
- **Classificação**: SubscriptionTier
- **Serviços**: ServiceIds (array para filtros)
- **Status**: IsActive (visibilidade na busca)
- **Endereço**: City, State, Description

---

## 🚀 Instalação e Configuração

### **Requisitos de Sistema**

#### **PostgreSQL com PostGIS**
O módulo Search requer PostgreSQL 12+ com a extensão PostGIS para queries geoespaciais.

**Instalação do PostGIS:**

```bash
# Via Aspire (Recomendado - automático)
cd src/Aspire/MeAjudaAi.AppHost
dotnet run

# Via Docker Compose
docker compose -f infrastructure/compose/environments/development.yml up -d

# Instalação manual no PostgreSQL
psql -U postgres -d MeAjudaAi -c "CREATE EXTENSION IF NOT EXISTS postgis;"
```

**Verificar instalação:**
```sql
SELECT PostGIS_Version();
-- Deve retornar algo como: 3.4 USE_GEOS=1 USE_PROJ=1...
```

### **Migrações de Banco de Dados**

O módulo usa schema isolado `search` com suporte PostGIS.

**Aplicar migrações:**

```powershell
# Aplicar migrações via Aspire (automático)
cd src\Aspire\MeAjudaAi.AppHost
dotnet run

# Ou aplicar manualmente
cd src\Modules\Search\Infrastructure
dotnet ef database update --startup-project ..\..\..\..\Bootstrapper\MeAjudaAi.ApiService

# Criar nova migração (se necessário)
dotnet ef migrations add <MigrationName> --startup-project ..\..\..\..\Bootstrapper\MeAjudaAi.ApiService
```

**Estrutura criada:**
- Schema: `search`
- Tabela: `searchable_providers` (snake_case)
- Extensão: `postgis` (geolocalização)
- Índices: GIST spatial index na coluna `location`

### **Configuração de Conexão**

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MeAjudaAi;Username=postgres;Password=postgres"
  }
}
```

**Environment Variables (produção):**
```bash
export DB_CONNECTION_STRING="Host=prod-server;Database=MeAjudaAi;..."
```

### **Registro de Serviços**

O módulo é registrado automaticamente no `Program.cs`:

```csharp
// Adiciona infraestrutura do Search (DbContext, Repositories)
builder.Services.AddSearchInfrastructure(builder.Configuration);

// Adiciona aplicação do Search (Handlers, Validators)
builder.Services.AddSearchApplication();

// Adiciona endpoints do Search
app.MapSearchEndpoints();
```

---

## 📡 API e Endpoints

### **GET /api/v1/search/providers**

Busca prestadores de serviço por proximidade e filtros.

#### **Parâmetros de Query**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `latitude` | `double` | ✅ | Latitude do ponto de busca (-90 a 90) |
| `longitude` | `double` | ✅ | Longitude do ponto de busca (-180 a 180) |
| `radiusInKm` | `double` | ✅ | Raio de busca em quilômetros (> 0, máx 500) |
| `serviceIds` | `Guid[]` | ❌ | IDs dos serviços desejados |
| `minRating` | `decimal` | ❌ | Avaliação mínima (0-5) |
| `subscriptionTiers` | `ESubscriptionTier[]` | ❌ | Tiers de assinatura (Free, Standard, Gold, Platinum) |
| `pageNumber` | `int` | ❌ | Número da página (padrão: 1) |
| `pageSize` | `int` | ❌ | Itens por página (padrão: 20, máx: 100) |

#### **Algoritmo de Busca**

1. **Filtro espacial**: Providers dentro do raio especificado
2. **Filtros opcionais**: Serviços, rating mínimo, tiers
3. **Ordenação (ranking)**:
   - **1º**: Subscription tier (Platinum > Gold > Standard > Free)
   - **2º**: Average rating (maior para menor)
   - **3º**: Distância (mais próximo primeiro)
4. **Paginação**: Skip e Take aplicados após ordenação

#### **Exemplo de Request**

```bash
curl -X GET "https://localhost:7032/api/v1/search/providers?\
latitude=-23.5505&\
longitude=-46.6333&\
radiusInKm=10&\
serviceIds=123e4567-e89b-12d3-a456-426614174000&\
minRating=4.0&\
subscriptionTiers=Gold&subscriptionTiers=Platinum&\
pageNumber=1&\
pageSize=20"
```

#### **Exemplo de Response (200 OK)**

```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "providerId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
      "name": "João Silva Eletricista",
      "location": {
        "latitude": -23.5505,
        "longitude": -46.6333
      },
      "distanceInKm": 2.5,
      "averageRating": 4.8,
      "totalReviews": 127,
      "subscriptionTier": "Gold",
      "serviceIds": [
        "123e4567-e89b-12d3-a456-426614174000"
      ],
      "description": "Eletricista com 15 anos de experiência",
      "city": "São Paulo",
      "state": "SP"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

#### **Casos de Uso**

**1. Buscar prestadores próximos:**
```
GET /api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=5
```

**2. Buscar eletricistas bem avaliados:**
```
GET /api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=10
    &serviceIds=<electrician-service-id>&minRating=4.5
```

**3. Buscar apenas prestadores Premium:**
```
GET /api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=20
    &subscriptionTiers=Gold&subscriptionTiers=Platinum
```

---

## ⚙️ Configuração Avançada

### **Cache de Queries**

O módulo implementa cache automático com chaves baseadas em parâmetros:

```csharp
// Cache key format
search:providers:lat:-23.5505:lng:-46.6333:radius:10:services:all:rating:4.5:tiers:Gold-Platinum:page:1:size:20

// TTL: 5 minutos (dados atualizados frequentemente)
```

**Invalidação de cache:**
- Automática após 5 minutos
- Manual via tags: `["search", "providers", "search-results"]`

### **Limites e Validação**

| Campo | Mínimo | Máximo | Padrão |
|-------|--------|--------|--------|
| Latitude | -90 | 90 | - |
| Longitude | -180 | 180 | - |
| RadiusInKm | 0.1 | 500 | - |
| MinRating | 0 | 5 | - |
| PageNumber | 1 | ∞ | 1 |
| PageSize | 1 | 100 | 20 |

### **Spatial Index Performance**

O módulo usa índice GIST para queries espaciais eficientes:

```sql
-- Índice criado automaticamente pela migração
CREATE INDEX ix_searchable_providers_location 
ON search.searchable_providers 
USING GIST (location);
```

**Performance esperada:**
- < 100ms para raio de 10km com 10k providers
- < 500ms para raio de 50km com 100k providers
- Cache hit rate > 70% em produção

---

## 🔗 Integração com Outros Módulos

### **Providers Module**
O módulo Search é um **read model** sincronizado com o módulo Providers:
- Eventos de domínio disparam atualização do SearchableProvider
- Sincronização via domain events ou mensageria (futura implementação)

**Fluxo de sincronização (planejado):**
```
Providers Module                Search Module
     │                               │
     ├─ Provider.Activate()          │
     ├─ ProviderActivatedEvent ─────>│
     │                          UpdateSearchableProvider
     │                          MarkAsActive()
```

### **Services Module (futuro)**
Validação de `serviceIds` será integrada quando o módulo Services estiver implementado.

### **Reviews Module (futuro)**
Atualização de `AverageRating` e `TotalReviews` via eventos de review.

---

## 🧪 Testes

### **Estrutura de Testes**

```
Tests/
├── Unit/
│   ├── Domain/
│   │   └── Entities/
│   │       └── SearchableProviderTests.cs        # 29 testes
│   ├── Application/
│   │   ├── Handlers/
│   │   │   └── SearchProvidersQueryHandlerTests.cs  # 7 testes
│   │   ├── Queries/
│   │   │   └── SearchProvidersQueryTests.cs         # 2 testes
│   │   └── Validators/
│   │       └── SearchProvidersQueryValidatorTests.cs # 22 testes
└── Integration/
    ├── SearchIntegrationTestBase.cs
    └── SearchableProviderRepositoryIntegrationTests.cs  # 11 testes
```

### **Executar Testes**

```powershell
# Todos os testes do módulo Search
dotnet test src\Modules\Search\Tests\

# Apenas testes unitários
dotnet test src\Modules\Search\Tests\ --filter "Category=Unit"

# Apenas testes de integração (requer Docker)
dotnet test src\Modules\Search\Tests\ --filter "Category=Integration"
```

### **Testes de Integração com Testcontainers**

Os testes de integração usam **Testcontainers** com PostgreSQL 16 + PostGIS 3.4:

```csharp
// Container iniciado automaticamente
var container = new PostgreSqlBuilder()
    .WithImage("postgis/postgis:16-3.4")
    .WithDatabase("search_test")
    .Build();

await container.StartAsync();
```

**Cobertura de testes:**
- ✅ Busca por raio
- ✅ Filtros combinados (serviços, rating, tier)
- ✅ Ordenação (tier > rating > distância)
- ✅ Paginação
- ✅ Providers inativos não aparecem
- ✅ CRUD básico

---

## 🐛 Troubleshooting

### **Problema: PostGIS extension not available**

**Causa:** PostgreSQL sem extensão PostGIS instalada.

**Solução:**
```sql
-- Conectar ao banco e instalar PostGIS
psql -U postgres -d MeAjudaAi
CREATE EXTENSION IF NOT EXISTS postgis;
SELECT PostGIS_Version(); -- Verificar
```

### **Problema: Spatial queries lentas**

**Causa:** Índice GIST ausente ou não utilizado.

**Solução:**
```sql
-- Verificar índices
SELECT indexname, indexdef 
FROM pg_indexes 
WHERE tablename = 'searchable_providers';

-- Recriar índice se necessário
REINDEX INDEX search.ix_searchable_providers_location;

-- Analisar query plan
EXPLAIN ANALYZE 
SELECT * FROM search.searchable_providers 
WHERE ST_DWithin(location, ST_MakePoint(-46.6333, -23.5505)::geography, 10000);
```

### **Problema: Migration fails with NetTopologySuite error**

**Causa:** Pacote NuGet `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` ausente.

**Solução:**
```powershell
cd src\Modules\Search\Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
dotnet ef database update --startup-project ..\..\..\..\Bootstrapper\MeAjudaAi.ApiService
```

### **Problema: Cache não funciona**

**Causa:** Redis não configurado ou indisponível.

**Verificar:**
```powershell
# Verificar se Redis está rodando
docker ps | grep redis

# Testar conexão
redis-cli ping  # Deve retornar PONG
```

---

## 📈 Roadmap

### **Implementações Futuras**

- [ ] **Sincronização automática** via domain events
- [ ] **Elasticsearch** para full-text search em descriptions
- [ ] **Filtros adicionais**: disponibilidade, preço, especialidades
- [ ] **Busca por rotas** (multiple waypoints)
- [ ] **Clustering de resultados** para visualização de mapa
- [ ] **A/B testing** de algoritmos de ranking
- [ ] **ML-based ranking** (personalização por histórico do usuário)

### **Otimizações Planejadas**

- [ ] **Materialização incremental** do read model
- [ ] **Particionamento** da tabela por região geográfica
- [ ] **Cache distribuído** com Redis Cluster
- [ ] **Read replicas** para queries geoespaciais

---

## 📚 Referências

- **PostGIS Documentation**: https://postgis.net/documentation/
- **NetTopologySuite**: https://github.com/NetTopologySuite/NetTopologySuite
- **Npgsql Spatial**: https://www.npgsql.org/efcore/mapping/nts.html
- **EF Core Spatial Data**: https://learn.microsoft.com/ef/core/modeling/spatial
- **Testcontainers .NET**: https://dotnet.testcontainers.org/

---

## 🤝 Contribuindo

Para contribuir com o módulo Search:

1. Leia o [Guia de Desenvolvimento](../development.md)
2. Implemente testes (cobertura mínima: 80%)
3. Verifique performance com queries geoespaciais
4. Documente mudanças em `CHANGELOG.md`
5. Abra Pull Request com descrição detalhada

**Contato do Maintainer:** Equipe MeAjudaAi
