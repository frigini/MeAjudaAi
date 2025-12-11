# Análise de Gaps de Cobertura - Caminho para 90%

**Data**: 9 de dezembro de 2025  
**Cobertura Atual**: 89.1%  
**Meta**: 90%  
**Gap**: +0.9%  
**Linhas Necessárias**: ~66 linhas adicionais (de 794 não cobertas)

---

## 📊 Sumário Executivo

Para aumentar a cobertura de **89.1% para 90%**, precisamos cobrir aproximadamente **66 linhas** adicionais. A estratégia recomendada é focar nas áreas de **maior impacto** que estão mais próximas de 90% ou têm muitas linhas não cobertas.

### Prioridades (Maior ROI):

1. **ApiService (85.1%)** - 794 linhas não cobertas
2. **Documents.Infrastructure (84.1%)** - Serviços Azure com baixa cobertura
3. **Shared (78.4%)** - Componentes de infraestrutura
4. **Users.API (79%)** - Extensions e Authorization

---

## 🎯 Áreas Críticas para Foco

### 1. ApiService (85.1% → 90%+) - **PRIORIDADE MÁXIMA**

#### Program.cs (28.1%) 🔴
**Impacto**: ALTO - Arquivo de entrada principal

**Linhas Não Cobertas**:
- Linhas 100-139: Configuração de middleware (try/catch, logging final)
- Método `ConfigureMiddlewareAsync` (linhas 100+)
- Método `LogStartupComplete` (não visualizado)
- Método `HandleStartupException` (não visualizado)
- Método `CloseLogging` (não visualizado)

**Solução**:
- Criar testes de integração para startup/shutdown
- Testar cenários de erro no startup
- Testes para ambiente Testing vs Production

**Estimativa**: +40 linhas cobertas

---

#### RateLimitingMiddleware.cs (42.2%) 🔴
**Impacto**: ALTO - Segurança e performance

**Linhas Não Cobertas** (estimadas):
- Método `GetEffectiveLimit` (linha 103+): Lógica de limites por endpoint
- Limites customizados por usuário autenticado
- Whitelist de IPs
- Cenários de rate limit excedido
- Warning threshold (80% do limite)

**Solução**:
```csharp
// Testes necessários:
// 1. Rate limit excedido para IP não autenticado
// 2. Rate limit excedido para usuário autenticado
// 3. IP whitelisted - bypass rate limit
// 4. Endpoint-specific limits
// 5. Approaching limit warning (80%)
// 6. Window expiration e reset
```

**Estimativa**: +60 linhas cobertas

---

#### ExampleSchemaFilter.cs (3.8%) 🔴
**Impacto**: BAIXO - Documentação OpenAPI

**Status**: Código comentado/desabilitado (NotImplementedException)

**Linhas Não Cobertas**:
- Todo o método `Apply` (linha 21+)
- Métodos privados comentados
- Migração pendente para Swashbuckle 10.x

**Solução**:
- **Opção 1**: Implementar migração para Swashbuckle 10.x e testar
- **Opção 2**: Excluir do coverage (código temporariamente desabilitado)
- **Recomendação**: Excluir do coverage por enquanto

**Estimativa**: N/A (código desabilitado)

---

### 2. Documents.Infrastructure (84.1% → 95%+)

#### AzureDocumentIntelligenceService.cs (33.3%) 🔴
**Impacto**: ALTO - Funcionalidade crítica de OCR

**Linhas Não Cobertas** (estimadas):
- Cenários de erro na análise de documentos
- Timeout handling
- Retry logic
- Parsing de resultados de OCR
- Validação de campos extraídos

**Solução**:
```csharp
// Testes com Mock do Azure Document Intelligence:
// 1. AnalyzeDocumentAsync - sucesso
// 2. AnalyzeDocumentAsync - timeout
// 3. AnalyzeDocumentAsync - erro de autenticação
// 4. Parsing de campos extraídos (CPF, RG, CNH)
// 5. Documento inválido/ilegível
```

**Estimativa**: +50 linhas cobertas

---

#### DocumentsDbContextFactory.cs (0%) 🔴
**Impacto**: BAIXO - Usado apenas em design-time

**Solução**:
- **Opção 1**: Criar teste de factory para migrations
- **Opção 2**: Excluir do coverage (código de design-time)
- **Recomendação**: Excluir do coverage

**Estimativa**: N/A (design-time code)

---

#### Documents.API.Extensions (37%) 🟡
**Impacto**: MÉDIO

**Linhas Não Cobertas**:
- Registro de serviços não testado
- Configuração de DI container

**Solução**:
```csharp
// Teste de integração:
// 1. Verificar se todos os serviços estão registrados
// 2. Verificar se endpoints estão mapeados
// 3. Health checks configurados
```

**Estimativa**: +15 linhas cobertas

---

### 3. Shared (78.4% → 85%+)

#### PostgreSqlExceptionProcessor.cs (18.1%) 🔴
**Impacto**: ALTO - Tratamento de erros de banco

**Linhas Não Cobertas**:
- Processamento de diferentes códigos de erro PostgreSQL
- Foreign key violations
- Unique constraint violations
- Not null violations
- Outros erros específicos do PostgreSQL

**Solução**:
```csharp
// Testes unitários:
// 1. ProcessException - ForeignKeyViolation (23503)
// 2. ProcessException - UniqueViolation (23505)
// 3. ProcessException - NotNullViolation (23502)
// 4. ProcessException - CheckViolation (23514)
// 5. ProcessException - UnknownError
```

**Estimativa**: +40 linhas cobertas

---

#### GlobalExceptionHandler.cs (43.3%) 🟡
**Impacto**: ALTO - Tratamento global de erros

**Linhas Não Cobertas**:
- Diferentes tipos de exceções
- Formatação de respostas de erro
- Logging de exceções

**Solução**:
```csharp
// Testes:
// 1. Handle ValidationException
// 2. Handle NotFoundException
// 3. Handle ForbiddenAccessException
// 4. Handle BusinessRuleException
// 5. Handle Exception genérica
// 6. Verificar logs e status codes
```

**Estimativa**: +35 linhas cobertas

---

#### Extensions e Registration (20-50%)
**Impacto**: MÉDIO

**Classes**:
- `ModuleServiceRegistrationExtensions` (20%)
- `ServiceCollectionExtensions` (78.5%)
- `Database.Extensions` (52.8%)
- `Logging.LoggingConfigurationExtensions` (56.9%)

**Solução**:
- Testes de integração para verificar registro de serviços
- Mock de IServiceCollection para validar chamadas

**Estimativa**: +30 linhas cobertas

---

### 4. DbContextFactory Classes (0%) - **BAIXA PRIORIDADE**

**Classes com 0% Coverage**:
- DocumentsDbContextFactory
- ProvidersDbContextFactory  
- SearchProvidersDbContextFactory
- ServiceCatalogsDbContextFactory
- UsersDbContextFactory

**Análise**: Todas são classes de design-time usadas para migrations do EF Core.

**Recomendação**: **Excluir do coverage** adicionando ao `.runsettings`:

```xml
<ModulePaths>
  <Exclude>
    <ModulePath>.*DbContextFactory\.cs</ModulePath>
  </Exclude>
</ModulePaths>
```

**Impacto**: Isso aumentaria a cobertura em ~0.3-0.5% instantaneamente sem criar testes.

---

### 5. Outras Áreas de Baixa Cobertura

#### SearchProvidersDbContext (43.4%) 🟡
**Solução**: Testes de queries e configurações

#### Providers.Infrastructure.ProviderRepository (87.5%) 🟢
**Solução**: Testar métodos específicos não cobertos

#### SearchProviders.Application.ModuleApi (73.9%) 🟡
**Solução**: Testar cenários de erro na API

---

## 📋 Plano de Ação Recomendado

### Fase 1: Quick Wins (Alcançar 90%) - **1-2 dias**

1. **Excluir DbContextFactory do coverage** (+0.5%)
   ```bash
   # Adicionar ao coverlet.runsettings
   <Exclude>[*]*DbContextFactory</Exclude>
   ```

2. **Testar RateLimitingMiddleware** (+0.3%)
   - Criar `RateLimitingMiddlewareTests.cs`
   - 10-15 testes cobrindo principais cenários

3. **Testar AzureDocumentIntelligenceService** (+0.2%)
   - Criar `AzureDocumentIntelligenceServiceTests.cs`
   - Mock do Azure SDK
   - Testar cenários de sucesso e erro

**Total Fase 1**: ~1.0% (89.1% → 90.1%) ✅

---

### Fase 2: Consolidação (Alcançar 92%) - **2-3 dias**

4. **Testar Program.cs startup** (+0.2%)
   - Integration tests para startup/shutdown
   - Testar diferentes ambientes

5. **Testar PostgreSqlExceptionProcessor** (+0.2%)
   - Todos os códigos de erro PostgreSQL
   - Cenários de fallback

6. **Testar GlobalExceptionHandler** (+0.2%)
   - Diferentes tipos de exceções
   - Validar respostas HTTP

7. **Testar Extensions de registro** (+0.2%)
   - ServiceCollectionExtensions
   - ModuleServiceRegistrationExtensions

**Total Fase 2**: ~0.8% (90.1% → 90.9%)

---

### Fase 3: Otimização (Alcançar 93%+) - **3-5 dias**

8. **Cobertura de Shared.Messaging** (+0.3%)
9. **Cobertura de Shared.Database** (+0.2%)
10. **Módulos API Extensions** (+0.2%)

**Total Fase 3**: ~0.7% (90.9% → 91.6%)

---

## 🎯 Resumo: Como Alcançar 90%

### Estratégia de Menor Esforço (Recomendada):

1. **Excluir DbContextFactory** (5 min)
   - Coverage: 89.1% → 89.6%

2. **Testar RateLimitingMiddleware** (4-6 horas)
   - Coverage: 89.6% → 89.9%

3. **Testar AzureDocumentIntelligenceService** (3-4 horas)
   - Coverage: 89.9% → 90.1%

**Total**: ~1 dia de trabalho para alcançar 90%+ ✅

---

## 📝 Notas Importantes

### Por que seus 27 testes não aumentaram coverage?

**DocumentsModuleApi já estava em 100%** devido a:
- Testes de integração E2E
- Testes de API endpoints
- Testes de handlers

Seus testes unitários cobriram os mesmos code paths já cobertos por testes de nível superior.

### Dica para Maximizar Coverage:

1. **Olhe o relatório HTML** (`coverage-github/report/index.html`)
2. **Identifique linhas vermelhas** (não cobertas)
3. **Foque em código de produção** (não DbContextFactory, Program.cs opcional)
4. **Teste cenários de erro** (onde está 70% do gap)

---

## 🔧 Ferramentas de Apoio

### Ver linhas não cobertas:
```bash
# Abrir relatório HTML
start coverage-github/report/index.html

# Ver resumo text
cat coverage-github/report/Summary.txt | Select-Object -First 100
```

### Gerar coverage local:
```bash
# Rodar pipeline localmente
./scripts/test-coverage-like-pipeline.ps1

# Gerar relatório HTML
reportgenerator `
  -reports:"coverage/aggregate/Cobertura.xml" `
  -targetdir:"coverage/report" `
  -reporttypes:"Html;TextSummary"
```

---

## 📚 Referências

- Relatório de Coverage Atual: `coverage-github/report/index.html` (gerado via CI/CD)
- [Pipeline CI/CD](`.github/workflows/ci-cd.yml`)
- [Configuração Coverlet](`config/coverlet.json`)
- [Script de Coverage Local](`scripts/test-coverage-like-pipeline.ps1`)
