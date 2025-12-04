# 📊 Análise de Gaps de Coverage

**Coverage Atual: 67.25%** (11,122 / 16,539 linhas)  
**Meta: ≥70%** (+2.75% necessário)  
**Linhas adicionais necessárias: ~455 linhas**

---

## 🔴 GAPS CRÍTICOS (<50% coverage)

### 1. **MeAjudaAi.Modules.SearchProviders.Application** - 45.32%
**PIOR MÓDULO** - Falta quase metade dos testes

**Classes com 0% coverage:**
- `PagedSearchResultDto<T>` - DTO genérico
- `SearchProvidersModuleApi` - API interna do módulo

### 2. **MeAjudaAi.Shared** - 45.53%
**Componente compartilhado crítico**

**Classes com <20% coverage:**
- `DapperConnection` - 9%
- `NoOpMessageBus` - 33.3%
- `NoOpBackgroundJobService` - 33.3%
- `PostgreSqlExceptionProcessor` - 18.1%

**Classes com 0% coverage (mas podem ser aceitáveis):**
- DTOs de messaging (ProviderActivatedIntegrationEvent, etc.)
- Database helpers (SchemaPermissionsManager, BaseDesignTimeDbContextFactory)
- Logging infrastructure (CorrelationIdEnricher, SerilogConfigurator, etc.)
- Dead Letter services (RabbitMqDeadLetterService, ServiceBusDeadLetterService)
- Monitoring/Metrics (MetricsCollectorService, BusinessMetrics)
- Jobs (HangfireBackgroundJobService, DocumentVerificationJob)

### 3. **MeAjudaAi.Modules.Locations.Infrastructure** - 47.99%

**Classes com <10% coverage:**
- `OpenCepClient` - 4.5%
- `ViaCepClient` - 4.5%
- `BrasilApiCepClient` - 4.7%
- `NominatimClient` - 9.8%

**Classes com <30% coverage:**
- `GeocodingService` - 23%
- `CepLookupService` - 26.5%

**Classes com 0% coverage:**
- Response DTOs: `ViaCepResponse`, `OpenCepResponse`, `NominatimResponse`, `BrasilApiCepResponse`

### 4. **MeAjudaAi.Modules.Documents.Infrastructure** - 49.24%

**Classes com <2% coverage:**
- `AzureDocumentIntelligenceService` - 1.5%

**Classes com <30% coverage:**
- `DocumentRepository` - 25%

**Classes com 0% coverage:**
- `DocumentsDbContextFactory`
- `DocumentVerificationJob`

---

## 🟡 GAPS MODERADOS (50-60% coverage)

### 5. **MeAjudaAi.Modules.ServiceCatalogs.API** - 52.21%

**Classes com <50% coverage:**
- `CreateServiceCategoryEndpoint` - 35.7%
- `GetServiceCategoryByIdEndpoint` - 41.6%
- `GetAllServiceCategoriesEndpoint` - 44.4%
- `GetServiceByIdEndpoint` - 45.4%
- `CreateServiceEndpoint` - 45.4%
- `Extensions` - 47%

### 6. **MeAjudaAi.Modules.SearchProviders.API** - 57.14%

**Classes com ~52% coverage:**
- `SearchProvidersEndpoint` - 52.2%

---

## 📈 ESTRATÉGIA RECOMENDADA PARA ATINGIR 70%

### **Prioridade ALTA** (Impacto máximo com menos esforço):

1. **Locations.Infrastructure External Clients** (~300 linhas descobertas)
   - ✅ Criar mocks para `ViaCepClient`, `OpenCepClient`, `BrasilApiCepClient`, `NominatimClient`
   - ✅ Testar `CepLookupService` e `GeocodingService` com fallback entre APIs
   - **Impacto estimado: +5% coverage**

2. **ServiceCatalogs.API Endpoints** (~150 linhas descobertas)
   - ✅ Testar todos os endpoints CRUD (Create, Get, Update, Delete, Activate, Deactivate)
   - ✅ Focar em validação de requests e responses
   - **Impacto estimado: +2% coverage**

3. **Shared - Database e Messaging** (~100 linhas)
   - ✅ Testar `DapperConnection`
   - ✅ Testar `PostgreSqlExceptionProcessor`
   - ✅ Testar `NoOp*` services (são simples, cobrem rápido)
   - **Impacto estimado: +1.5% coverage**

### **Prioridade MÉDIA** (Esforço médio):

4. **Documents.Infrastructure** (~80 linhas)
   - ⚠️ `AzureDocumentIntelligenceService` - pode precisar de mock complexo
   - ✅ `DocumentRepository` - testar CRUD básico
   - **Impacto estimado: +1% coverage**

5. **SearchProviders.Application** (~50 linhas)
   - ✅ Testar `PagedSearchResultDto<T>`
   - ✅ Testar `SearchProvidersModuleApi`
   - **Impacto estimado: +0.5% coverage**

### **Prioridade BAIXA** (Pode ignorar):

- ❌ **DTOs com 0%**: Muitos são apenas POCOs sem lógica
- ❌ **Migrations/DbContextFactory**: Não executam em runtime
- ❌ **Logging/Monitoring infrastructure**: Difícil testar, baixo ROI
- ❌ **Dead Letter services**: Complexos, testados indiretamente

---

## 📊 CÁLCULO DO IMPACTO

Para subir de **67.25%** para **70%**:

```
Linhas atuais cobertas: 11,122
Linhas totais: 16,539

Para 70%:
16,539 × 0.70 = 11,577 linhas precisam estar cobertas
Diferença: 11,577 - 11,122 = 455 linhas a mais
```

**Prioridades sugeridas cobrem ~600 linhas** → **Suficiente para atingir 70%+**

---

## ✅ AÇÕES IMEDIATAS

1. ✅ Criar testes para **Locations External Clients** (+5%)
2. ✅ Criar testes para **ServiceCatalogs.API Endpoints** (+2%)
3. ✅ Criar testes para **Shared Database/Messaging** (+1.5%)

**Total esperado: ~8.5%** → Coverage final: **~75.75%** ✅

---

## 🎯 PRÓXIMOS PASSOS

1. Criar testes conforme prioridades acima
2. Rodar coverage localmente para validar
3. Push para GitHub Actions
4. Verificar se atingiu ≥70%
5. Habilitar `STRICT_COVERAGE: true` no workflow
