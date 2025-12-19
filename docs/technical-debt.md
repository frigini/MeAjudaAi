# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia itens de débito técnico e melhorias planejadas identificadas durante o desenvolvimento que devem ser convertidas em issues do GitHub.

---

## 🔄 Sprint 5.5 - Refactor & Cleanup (19 Dez - 31 Dez 2025)

**Branch**: `feature/refactor-and-cleanup`  
**Objetivo**: Refatoração técnica e redução de débito técnico antes do desenvolvimento do frontend Blazor

### Itens Planejados para Resolução

Os itens abaixo estão planejados para serem resolvidos na Sprint 5.5. Após implementação, serão removidos deste documento ou movidos para seção "Concluído".

#### 🏗️ Refatoração MeAjudaAi.Shared.Messaging (8-10h)

**Situação**: ESTRUTURA DESORGANIZADA  
**Severidade**: MÉDIA (manutenibilidade)  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup)

**Problemas Identificados**:

1. **Arquivos com múltiplas classes**:
   - `DeadLetterServiceFactory.cs` contém: `NoOpDeadLetterService`, `IDeadLetterServiceFactory`, `EnvironmentBasedDeadLetterServiceFactory`
   - `IDeadLetterService.cs` contém: `DeadLetterStatistics`, `FailureRate`
   - `MessageRetryMiddleware.cs` contém: `IMessageRetryMiddlewareFactory`, `MessageRetryMiddlewareFactory`, `MessageRetryExtensions`
   - `MessageBusFactory.cs` contém: `IMessageBusFactory`, `EnvironmentBasedMessageBusFactory`
   - `RabbitMqInfrastructureManager.cs` não possui interface separada `IRabbitMqInfrastructureManager`

2. **Inconsistência de nomenclatura**:
   - Arquivo `DeadLetterServiceFactory.cs` mas classe principal é `EnvironmentBasedDeadLetterServiceFactory`
   - Arquivo `MessageBusFactory.cs` mas classe principal é `EnvironmentBasedMessageBusFactory`

3. **Integration Events ausentes**:
   - Documents, SearchProviders, ServiceCatalogs não possuem integration events em Messages/
   - Faltam event handlers para comunicação entre módulos

**Ações de Refatoração**:
- [ ] Separar `NoOpDeadLetterService` em arquivo próprio: `NoOpDeadLetterService.cs`
- [ ] Extrair `IDeadLetterServiceFactory` para: `IDeadLetterServiceFactory.cs`
- [ ] Renomear `DeadLetterServiceFactory.cs` → `EnvironmentBasedDeadLetterServiceFactory.cs`
- [ ] Extrair `DeadLetterStatistics` para: `DeadLetterStatistics.cs`
- [ ] Extrair `FailureRate` para: `FailureRate.cs`
- [ ] Extrair `IMessageRetryMiddlewareFactory` para: `IMessageRetryMiddlewareFactory.cs`
- [ ] Extrair `MessageRetryMiddlewareFactory` para: `MessageRetryMiddlewareFactory.cs`
- [ ] Extrair `MessageRetryExtensions` para: `MessageRetryExtensions.cs`
- [ ] Criar `IMessageBusFactory.cs` separado
- [ ] Renomear `MessageBusFactory.cs` → `EnvironmentBasedMessageBusFactory.cs`
- [ ] Extrair `IRabbitMqInfrastructureManager` para arquivo separado
- [ ] Reorganizar estrutura de pastas em Messaging/ (sugestão abaixo)
- [ ] Adicionar integration events para módulos faltantes
- [ ] Criar testes unitários para classes de messaging (>70% coverage)

**Estrutura Proposta** (após refatoração):
```
src/Shared/Messaging/
├── Abstractions/
│   ├── IMessageBus.cs
│   ├── IMessageBusFactory.cs
│   ├── IDeadLetterService.cs
│   ├── IDeadLetterServiceFactory.cs
│   ├── IMessageRetryMiddlewareFactory.cs
│   └── IRabbitMqInfrastructureManager.cs
├── DeadLetter/
│   ├── DeadLetterStatistics.cs
│   ├── FailureRate.cs
│   ├── DeadLetterOptions.cs
│   ├── NoOpDeadLetterService.cs
│   ├── EnvironmentBasedDeadLetterServiceFactory.cs
│   ├── RabbitMqDeadLetterService.cs
│   └── ServiceBusDeadLetterService.cs
├── Handlers/
│   ├── MessageRetryMiddleware.cs
│   ├── MessageRetryMiddlewareFactory.cs
│   └── MessageRetryExtensions.cs
├── RabbitMq/
│   ├── RabbitMqMessageBus.cs
│   ├── RabbitMqInfrastructureManager.cs
│   └── RabbitMqOptions.cs
├── ServiceBus/
│   ├── ServiceBusMessageBus.cs
│   ├── ServiceBusTopicManager.cs
│   ├── ServiceBusOptions.cs
│   └── ServiceBusInitializationService.cs
├── Messages/
│   ├── Documents/
│   │   ├── DocumentUploadedIntegrationEvent.cs
│   │   └── DocumentVerifiedIntegrationEvent.cs
│   ├── Providers/
│   ├── Users/
│   └── ...
├── EnvironmentBasedMessageBusFactory.cs
└── EventTypeRegistry.cs
```

**Prioridade**: MÉDIA  
**Estimativa**: 8-10 horas  
**Benefício**: Código mais organizado, manutenível e testável

---

#### 🔧 Refatoração Extensions (MeAjudaAi.Shared) (4-6h)

**Situação**: INCONSISTÊNCIA DE PADRÃO  
**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup)

**Problemas Identificados**:

1. **Extensions dentro de classes de implementação**:
   - `BusinessMetricsMiddlewareExtensions` está dentro de `BusinessMetricsMiddleware.cs`
   - Outros middlewares/serviços podem ter o mesmo padrão

2. **Falta de consolidação**:
   - Extensions espalhadas em múltiplos arquivos
   - Dificulta descoberta de métodos de extensão disponíveis
   - Falta padrão consistente com os módulos

**Ações de Refatoração**:
- [ ] Extrair `BusinessMetricsMiddlewareExtensions` para arquivo próprio
- [ ] Criar arquivo `MonitoringExtensions.cs` consolidando todas extensions de Monitoring
- [ ] Criar arquivo `CachingExtensions.cs` consolidando todas extensions de Caching
- [ ] Criar arquivo `MessagingExtensions.cs` consolidando todas extensions de Messaging
- [ ] Criar arquivo `AuthorizationExtensions.cs` consolidando todas extensions de Authorization
- [ ] Revisar pasta `Extensions/` - manter apenas extensions gerais/cross-cutting
- [ ] Documentar padrão: cada funcionalidade tem seu `<Funcionalidade>Extensions.cs`
- [ ] Aplicar padrão em todas as pastas do Shared

**Estrutura Proposta** (após refatoração):
```
src/Shared/
├── Monitoring/
│   ├── BusinessMetricsMiddleware.cs
│   ├── MetricsCollectorService.cs
│   └── MonitoringExtensions.cs ← NOVO (consolidado)
├── Caching/
│   ├── HybridCacheService.cs
│   └── CachingExtensions.cs ← NOVO (consolidado)
├── Messaging/
│   ├── ... (classes de messaging)
│   └── MessagingExtensions.cs ← NOVO (consolidado)
├── Authorization/
│   ├── ... (classes de autorização)
│   └── AuthorizationExtensions.cs ← NOVO (consolidado)
└── Extensions/
    ├── ServiceCollectionExtensions.cs (gerais)
    ├── ModuleServiceRegistrationExtensions.cs
    └── ... (apenas extensions cross-cutting)
```

**Padrão de Nomenclatura**:
- Arquivo: `<Funcionalidade>Extensions.cs` (e.g., `MonitoringExtensions.cs`)
- Classe: `public static class <Funcionalidade>Extensions`
- Namespace: `MeAjudaAi.Shared.<Funcionalidade>`

**Prioridade**: BAIXA  
**Estimativa**: 4-6 horas  
**Benefício**: Código mais organizado e consistente com padrão dos módulos

---

#### 🧪 Review Completo de Testes (6-8h)

**Situação**: NÃO AUDITADO  
**Severidade**: MÉDIA  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup)

**Descrição**:
Auditoria completa de todos os arquivos em `tests/` para identificar:
- Testes duplicados ou redundantes
- Testes obsoletos (funcionalidades removidas)
- Testes mal estruturados (AAA pattern não seguido)
- Gaps de cobertura
- Oportunidades de consolidação

**Ações**:
- [ ] Revisar todos os arquivos em `tests/MeAjudaAi.Shared.Tests/`
- [ ] Revisar todos os arquivos em `tests/MeAjudaAi.Integration.Tests/`
- [ ] Revisar todos os arquivos em `tests/MeAjudaAi.E2E.Tests/`
- [ ] Revisar testes unitários de cada módulo
- [ ] Identificar e remover testes duplicados
- [ ] Atualizar testes desatualizados
- [ ] Documentar padrões de teste para novos contribuidores
- [ ] Criar checklist de code review para testes

**Prioridade**: MÉDIA  
**Estimativa**: 6-8 horas  
**Benefício**: Suite de testes mais confiável e manutenível

---

## ⚠️ CRÍTICO: Hangfire + Npgsql 10.x Compatibility Risk

**Arquivo**: `Directory.Packages.props`  
**Linhas**: 45-103  
**Situação**: VALIDAÇÃO EM ANDAMENTO - BLOQUEIO DE DEPLOY  
**Severidade**: ALTA  
**Issue**: [Criar issue para rastreamento]

**Descrição**: 
Hangfire.PostgreSql 1.20.12 foi compilado contra Npgsql 6.x, mas o projeto está migrando para Npgsql 10.x, que introduz breaking changes. A compatibilidade em runtime não foi validada pelo mantenedor do Hangfire.PostgreSql.

**Problema Identificado**:
- Npgsql 10.x introduz mudanças incompatíveis (breaking changes)
- Hangfire.PostgreSql 1.20.12 não foi testado oficialmente com Npgsql 10.x
- Risco de falhas em: persistência de jobs, serialização, conexão, corrupção de dados
- Deploy para produção está BLOQUEADO até validação completa

**Mitigação Implementada**:
1. ✅ Documentação detalhada de estratégia de versões em `Directory.Packages.props`
2. ✅ Testes de integração removidos - monitoramento via health checks
3. ✅ CI/CD gating configurado (`.github/workflows/pr-validation.yml`)
4. ✅ Procedimentos de rollback documentados
5. ✅ Plano de monitoramento de produção definido

**Validação Necessária ANTES de Deploy para Produção**:
- [ ] Todos os testes de integração Hangfire passando no CI/CD
- [ ] Validação manual localmente com carga realística
- [ ] Monitoramento de produção configurado (alertas de taxa de falha >5%)
- [ ] Procedimento de rollback testado localmente
- [ ] Plano de comunicação para stakeholders aprovado

**Opções de Implementação**:

**OPÇÃO 1 (ATUAL)**: Manter Npgsql 10.x + Hangfire.PostgreSql 1.20.12
- Requer validação completa via testes de integração
- Monitorar: <https://github.com/frankhommers/Hangfire.PostgreSql/issues>
- Rollback para Opção 2 se falhas detectadas

**OPÇÃO 2 (FALLBACK SEGURO)**: Downgrade para Npgsql 8.x
- Versões conhecidas e compatíveis
- Trade-off: Adia benefícios da migração para .NET 10
- Implementação imediata se Opção 1 falhar

**OPÇÃO 3 (FUTURO)**: Aguardar Hangfire.PostgreSql 2.x
- Suporte oficial para Npgsql 10.x
- Timeline desconhecida

**OPÇÃO 4 (EMERGÊNCIA)**: Backend alternativo
- Hangfire.Pro.Redis (requer licença)
- Hangfire.SqlServer (requer infraestrutura SQL Server)

**Prioridade**: CRÍTICA  
**Dependências**: Testes de integração, validação local, monitoramento de produção  
**Prazo**: Antes de qualquer deploy para produção

**Critérios de Aceitação**:
- [x] Testes de integração implementados e passando
- [x] CI/CD gating configurado para bloquear deploy se testes falharem
- [x] Documentação de compatibilidade criada
- [x] Procedimento de rollback documentado e testado
- [ ] Validação local com simulação de carga de produção
- [ ] Monitoramento de produção configurado
- [ ] Equipe treinada em procedimento de rollback
- [ ] Stakeholders notificados sobre o risco e plano de mitigação

**Documentação**:
- Guia completo: Monitoramento via health checks em produção
- Testes: Removidos - validação via health checks
- CI/CD: `.github/workflows/pr-validation.yml` (step "CRITICAL - Hangfire Npgsql 10.x Compatibility Tests")
- Configuração: `Directory.Packages.props` (linhas 45-103)

---

## ⚠️ MÉDIO: Falta de Testes para Infrastructure Extensions

**Arquivos**: 
- `src/Aspire/MeAjudaAi.AppHost/Extensions/KeycloakExtensions.cs`
- `src/Aspire/MeAjudaAi.AppHost/Extensions/PostgreSqlExtensions.cs`
- `src/Aspire/MeAjudaAi.AppHost/Extensions/MigrationExtensions.cs`

**Situação**: SEM TESTES  
**Severidade**: MÉDIA  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup)  
**Issue**: [Será criado na Sprint 5.5]

**Descrição**: 
As classes de extensão do AppHost que configuram infraestrutura crítica (Keycloak, PostgreSQL, Migrations) não possuem testes unitários ou de integração. Isso representa risco para:
- Mudanças em configuração de produção
- Refatorações futuras
- Validação de comportamento em diferentes ambientes

**Componentes Sem Testes**:
1. **KeycloakExtensions** (~170 linhas):
   - `AddMeAjudaAiKeycloak()` - configuração de desenvolvimento
   - `AddMeAjudaAiKeycloakProduction()` - configuração de produção com validação de segurança

2. **PostgreSqlExtensions** (~260 linhas):
   - `AddMeAjudaAiPostgreSQL()` - configuração local/desenvolvimento
   - `AddMeAjudaAiAzurePostgreSQL()` - configuração Azure com managed identity

3. **MigrationExtensions** (~50 linhas):
   - `AddMeAjudaAiMigrations()` - registro de MigrationHostedService

**Risco Atual**:
- **BAIXO a MÉDIO**: Código é relativamente estável e usado em desenvolvimento
- Refatoração recente (Sprint 4) melhorou estrutura mas não adicionou testes
- Mudanças futuras podem introduzir regressões sem detecção

**Mitigação Atual**:
1. ✅ Código bem estruturado com separação clara (Options/Results/Services)
2. ✅ Comentários em português explicando lógica
3. ✅ Validações de segurança em produção (KeycloakProduction)
4. ✅ Logging detalhado de configuração
5. ⚠️ **SEM** testes automatizados

**Ações Recomendadas**:

**CURTO PRAZO** (antes de próximas mudanças em infraestrutura):
1. Criar testes de integração para KeycloakExtensions:
   - Validar que configuração de desenvolvimento funciona
   - Validar que configuração de produção rejeita senhas fracas
   - Validar URLs e endpoints gerados corretamente

2. Criar testes de integração para PostgreSqlExtensions:
   - Validar criação de databases e schemas
   - Validar connection strings geradas
   - Validar configuração Azure com managed identity

3. Criar testes unitários para MigrationExtensions:
   - Validar que MigrationHostedService é registrado
   - Validar que migrations não rodam em ambiente Testing

**MÉDIO PRAZO** (backlog):
- Adicionar testes E2E que validam stack completa do AppHost
- Configurar CI para validar mudanças em extensions

**Prioridade**: MÉDIA  
**Esforço Estimado**: 4-6 horas para cobertura básica  
**Dependências**: Nenhuma - pode ser feito incrementalmente

**Critérios de Aceitação**:
- [ ] Testes de integração para KeycloakExtensions (>70% coverage)
- [ ] Testes de integração para PostgreSqlExtensions (>70% coverage)
- [ ] Testes unitários para MigrationExtensions (>80% coverage)
- [ ] CI configurado para rodar testes de extensions
- [ ] Documentação de como testar extensions localmente

---

## ✅ ~~Swagger ExampleSchemaFilter - Migração para Swashbuckle 10.x~~ [REMOVIDO]

**Status**: REMOVIDO PERMANENTEMENTE (13 Dez 2025)  
**Razão**: Código problemático que sempre quebrava, difícil de testar, e não essencial

**Decisão**:
O `ExampleSchemaFilter` foi **removido completamente** do projeto por:
- Estar desabilitado desde a migração Swashbuckle 10.x (sempre quebrava)
- Causar erros de compilação frequentes no CI/CD
- Ser difícil de testar e manter
- Funcionalidade puramente cosmética (adicionar exemplos automáticos ao Swagger)
- Swagger funciona perfeitamente sem ele
- Exemplos podem ser adicionados manualmente via XML comments quando necessário

**Arquivos Removidos**:
- `src/Bootstrapper/MeAjudaAi.ApiService/Filters/ExampleSchemaFilter.cs` ❌
- `tests/MeAjudaAi.ApiService.Tests/Unit/Swagger/ExampleSchemaFilterTests.cs` ❌
- TODO em `DocumentationExtensions.cs` removido

**Alternativa**:
Use **XML documentation comments** para adicionar exemplos quando necessário:
```csharp
/// <summary>
/// Email do usuário
/// </summary>
/// <example>usuario@exemplo.com</example>
public string Email { get; set; }
```

**Commit**: [Adicionar hash após commit]

---
- Original PR/Issue que introduziu IOpenApiSchema: [A investigar]

---

## Melhorias nos Testes de Integração

### Melhoria do Teste de Status de Verificação de Prestador
**Arquivo**: `tests/MeAjudaAi.Integration.Tests/Providers/ProvidersIntegrationTests.cs`  
**Linha**: ~172-199  
**Situação**: Aguardando Implementação de Funcionalidade Base  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - TODO resolution  

**Descrição**: 
O teste `GetProvidersByVerificationStatus_ShouldReturnOnlyPendingProviders` atualmente apenas valida a estrutura da resposta devido à falta de endpoints de gerenciamento de status de verificação.

**Problema Identificado**:
- TODO comentário nas linhas 180-181 indica limitação atual
- Teste não pode verificar comportamento real de filtragem
- Não há como definir status de verificação durante criação de prestador

**Melhoria Necessária**:
- Implementar endpoints de gerenciamento de status de verificação de prestadores (aprovar/rejeitar/atualizar verificação)
- Criar prestadores de teste com diferentes status de verificação
- Melhorar o teste para verificar o comportamento real de filtragem (apenas prestadores com status Pending retornados)
- Adicionar testes similares para outros status de verificação (Approved, Rejected, etc.)

**Opções de Implementação**:
1. **Abrir nova issue** para rastrear implementação de endpoints de gerenciamento de status
2. **Implementar funcionalidade** de atualização de status de verificação
3. **Criar testes mais abrangentes** quando endpoints estiverem disponíveis

**Prioridade**: Média  
**Dependências**: Endpoints de API para gerenciamento de status de verificação de prestadores  

**Critérios de Aceitação**:
- [ ] Endpoints de gerenciamento de status de verificação de prestadores disponíveis
- [ ] Teste pode criar prestadores com diferentes status de verificação
- [ ] Teste verifica que a filtragem retorna apenas prestadores com o status especificado
- [ ] Teste inclui limpeza dos dados de teste criados
- [ ] Testes similares adicionados para todos os valores de status de verificação

---

## 🧪 Testes E2E Ausentes - Módulo SearchProviders

**Módulo**: `src/Modules/SearchProviders`  
**Tipo**: Débito de Teste  
**Severidade**: MÉDIA  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - BACKLOG (2-3 sprints)  
**Issue**: [Será criado na Sprint 5.5]

**Descrição**:
O módulo SearchProviders não possui testes E2E (end-to-end), apenas testes de integração e unitários. Testes E2E são necessários para validar o fluxo completo de busca de prestadores, incluindo integração com APIs externas (IBGE), filtros, paginação, e respostas HTTP completas.

**Contexto**:
- Identificado durante code review automatizado (CodeRabbit)
- Testes de integração existentes cobrem lógica de negócio e repositórios
- Faltam testes que validam endpoints HTTP completos com autenticação real

**Impacto**:
- Risco de regressões em endpoints de busca não detectadas até produção
- Falta de validação de integração completa API externa → Aplicação → Resposta HTTP
- Dificuldade em validar comportamento de autenticação e autorização em cenários reais

**Escopo de Testes E2E Necessários**:

1. **SearchProviders API Endpoints**:
   - [ ] `GET /api/search-providers/search` - Busca com múltiplos filtros
   - [ ] `GET /api/search-providers/search` - Paginação e ordenação
   - [ ] `GET /api/search-providers/search` - Busca com autenticação/autorização
   - [ ] `GET /api/search-providers/search` - Respostas de erro (400, 401, 404, 500)

2. **Integração com IBGE API**:
   - [ ] Validação de respostas da API do IBGE (mock ou real)
   - [ ] Tratamento de timeouts e erros de rede
   - [ ] Validação de mapeamento de dados geográficos (UF, município)

3. **Filtros e Busca**:
   - [ ] Busca por localização (estado, cidade)
   - [ ] Busca por tipo de serviço
   - [ ] Busca por status de verificação
   - [ ] Combinação de múltiplos filtros

4. **Desempenho e Carga**:
   - [ ] Busca com grande volume de resultados (1000+ prestadores)
   - [ ] Validação de tempos de resposta (<500ms para buscas simples)
   - [ ] Cache de resultados de API externa

**Arquivos Relacionados**:
- `src/Modules/SearchProviders/API/` - Endpoints a serem testados
- `tests/MeAjudaAi.E2E.Tests/` - Localização sugerida para novos testes
- `tests/MeAjudaAi.Integration.Tests/Infrastructure/WireMockFixture.cs` - Mock de IBGE API

**Prioridade**: Média  
**Estimativa**: 2-3 sprints  
**Dependências**: 
- Infraestrutura de testes E2E já estabelecida (`MeAjudaAi.E2E.Tests`)
- WireMock configurado para simulação de IBGE API
- TestContainers disponível para PostgreSQL e Redis

**Critérios de Aceitação**:
- [ ] Pelo menos 15 testes E2E cobrindo cenários principais de busca
- [ ] Cobertura de autenticação/autorização em todos os endpoints
- [ ] Testes validam códigos de status HTTP corretos
- [ ] Testes validam estrutura completa de resposta JSON
- [ ] Testes incluem cenários de erro e edge cases
- [ ] Testes executam em CI/CD com sucesso
- [ ] Documentação de testes E2E atualizada

**Notas Técnicas**:
- Utilizar `TestContainerTestBase` como base para testes E2E
- Configurar WireMock para simular respostas da API do IBGE
- Usar `ConfigurableTestAuthenticationHandler` para cenários de autenticação
- Validar integração com Redis (cache) e PostgreSQL (dados)

---

## 📦 Microsoft.OpenApi 2.3.0 - Bloqueio de Atualização para 3.x

**Arquivo**: `Directory.Packages.props` (linha ~46)  
**Situação**: BLOQUEADO - Incompatibilidade com ASP.NET Core Source Generators  
**Severidade**: BAIXA (não crítico, funciona perfeitamente)  
**Sprint**: N/A - Aguardar correção da Microsoft  
**Issue**: [Monitoramento contínuo]

**Descrição**:
Microsoft.OpenApi está pinado em versão 2.3.0 porque a versão 3.0.2 é incompatível com os source generators do ASP.NET Core 10.0 (`Microsoft.AspNetCore.OpenApi.SourceGenerators`).

**Problema Identificado**:
```
error CS0200: Property or indexer 'IOpenApiMediaType.Example' cannot be assigned to -- it is read only
```

**Testes Realizados**:
```text
- ✅ Testado com SDK 10.0.101 (Dez 2025) - ainda quebra
- ✅ Testado Microsoft.OpenApi 3.0.2 - incompatível
- ✅ Confirmado que 2.3.0 funciona perfeitamente
```

**Causa Raiz**:
- Microsoft.OpenApi 3.x mudou `IOpenApiMediaType.Example` para read-only (breaking change)
- ASP.NET Core source generator ainda gera código que tenta escrever nessa propriedade
- Source generator não foi atualizado para API do OpenApi 3.x

**Dependência**: Swashbuckle.AspNetCore
- Swashbuckle 10.x depende de Microsoft.OpenApi (transitivo)
- Projeto usa Swashbuckle para Swagger UI e customizações avançadas
- Swashbuckle v10 migration guide: [Swashbuckle v10 Migration](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)

**Opções de Resolução**:

**OPÇÃO 1 (ATUAL - RECOMENDADA)**: Manter Microsoft.OpenApi 2.3.0
- ✅ Funciona perfeitamente
- ✅ Zero impacto em funcionalidades
- ✅ Swagger UI completo e funcional
- ⚠️ Versão desatualizada (mas estável)

**OPÇÃO 2 (FUTURO)**: Aguardar correção da Microsoft
- Microsoft atualiza source generator para OpenApi 3.x
- Timeline: Desconhecida (provavelmente .NET 11 ou patch futuro)
- Monitorar: [ASP.NET Core Issues](https://github.com/dotnet/aspnetcore/issues)

**OPÇÃO 3 (COMPLEXA - NÃO RECOMENDADA AGORA)**: Migrar para ASP.NET Core OpenAPI nativo
- Remove Swashbuckle completamente
- Usa `Microsoft.AspNetCore.OpenApi` nativo (.NET 9+)
- **PROBLEMA**: Não inclui Swagger UI por padrão
  - Precisa adicionar Scalar/SwaggerUI/RapiDoc separadamente
  - Perde configurações avançadas de UI (InjectStylesheet, DocExpansion, etc)
- **ESFORÇO**: 5-8 horas de trabalho
  - Migrar CustomSchemaIds → transformers
  - Migrar CustomOperationIds → transformers  
  - Migrar ApiVersionOperationFilter → transformers
  - Configurar UI externa (Scalar recomendado)
  - Atualizar 3 arquivos de teste
- **ROI**: Baixo - funcionalidade atual é completa

**Monitoramento**:
- [ ] Verificar releases do .NET SDK para correções no source generator
- [ ] Testar Microsoft.OpenApi 3.x a cada atualização de SDK
- [ ] Monitorar Swashbuckle releases para melhor suporte OpenApi 3.x
- [ ] Avaliar migração para OpenAPI nativo quando UI nativo estiver disponível

**Prioridade**: BAIXA (não urgente)  
**Estimativa**: Aguardar correção oficial (sem ação necessária)  
**Workaround Atual**: Manter 2.3.0 (100% funcional)

**Critérios para Atualização**:
- [ ] Microsoft corrigir source generator para OpenApi 3.x, OU
- [ ] Swashbuckle suportar completamente OpenApi 3.x, OU
- [ ] Necessidade real de features do OpenApi 3.x (atualmente nenhuma)

**Documentação**:
- Comentário detalhado em `Directory.Packages.props` (linhas 46-49)
- Migration guide Swashbuckle: [Swashbuckle v10 Migration](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)
- ASP.NET Core OpenAPI docs: [OpenAPI in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi)

**Nota**: Esta limitação **NÃO afeta** funcionalidade, performance ou segurança. É puramente uma questão de versão de dependência.

---

## 📋 Padronização de Records (Para Próxima Sprint)

**Arquivo**: Múltiplos arquivos em `src/Shared/Contracts/**` e `src/Modules/**/Domain/**`  
**Situação**: INCONSISTÊNCIA - Dois padrões em uso  
**Severidade**: BAIXA (manutenibilidade)  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - Baixa prioridade  
**Issue**: [Será criado na Sprint 5.5]

**Descrição**: 
Atualmente existem dois padrões de sintaxe para records no projeto:

### Padrão 1: Positional Records (Sintaxe Concisa)

```csharp
public sealed record ModuleCoordinatesDto(
    double Latitude,
    double Longitude);
```

### Padrão 2: Property-based Records (Sintaxe Explícita)

```csharp
public sealed record ModuleLocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
```

**Análise**:

*Positional Records:*
- ✅ Mais conciso
- ✅ Gera automaticamente construtor, desconstrutor, Equals, GetHashCode
- ✅ Ideal para DTOs simples e imutáveis
- ❌ Menos flexível para validação/lógica customizada
- ❌ Ordem dos parâmetros importa

*Property-based Records:*
- ✅ Maior flexibilidade (validação, valores padrão complexos)
- ✅ Permite required e init-only de forma explícita
- ✅ Ordem não importa
- ❌ Mais verboso
- ❌ Não gera desconstrutor automaticamente

**Recomendação**:

*Para DTOs simples* (maioria dos casos em Contracts/Modules): Usar **Positional Records**
- São mais concisos
- Comunicação entre módulos não precisa de lógica complexa
- Imutabilidade garantida por design

*Para Value Objects e Domain Models*: Usar **Property-based Records**
- Permite validação no construtor
- Maior controle sobre comportamento

**Ação Sugerida**:
Na próxima sprint, padronizar todos os records em:
- `src/Shared/Contracts/**/*.cs` → Positional Records
- `src/Modules/**/Domain/**/*.cs` → Property-based Records (onde fizer sentido)

**Arquivos para Revisar**:
- [ ] Todos os DTOs em Contracts/Modules
- [ ] Value Objects em Domain
- [ ] Responses/Requests em Shared

**Prioridade**: BAIXA (não urgente, melhoria de consistência)  
**Estimativa**: 2-3 horas  

---

## Instruções para Mantenedores

1. **Conversão para Issues do GitHub**: 
   - Copiar a descrição da melhoria para um novo issue do GitHub
   - Adicionar labels apropriadas (`technical-debt`, `testing`, `enhancement`)
   - Vincular ao arquivo específico e número da linha
   - Adicionar ao backlog do projeto com prioridade apropriada

2. **Atualizando este Documento**:
   - Marcar itens como "Issue Criado" com número do issue quando convertido
   - Remover itens completos ou mover para seção "Concluído"
   - Adicionar novos itens de débito técnico conforme identificados

3. **Referências de Código**:
   - Usar tag `[ISSUE]` em comentários TODO para indicar itens rastreados aqui
   - Incluir caminho do arquivo e números de linha para navegação fácil
   - Manter descrições específicas e acionáveis
---

## ⚠️ BAIXO: Alinhamento de Middleware entre UseSharedServices() e UseSharedServicesAsync()

**Arquivo**: `src/Shared/Extensions/ServiceCollectionExtensions.cs`  
**Linhas**: 96-100  
**Situação**: TODO #249  
**Severidade**: BAIXA  
**Sprint**: Sprint 5.5 (feature/refactor-and-cleanup) - Média prioridade  
**Issue**: [Será criado na Sprint 5.5]

**Descrição**: 
O caminho assíncrono `UseSharedServicesAsync()` não registra serviços de BusinessMetrics da mesma forma que o caminho síncrono `UseSharedServices()`, causando falha no middleware `UseAdvancedMonitoring` em ambientes de desenvolvimento.

**Problema Identificado**:
- Caminho assíncrono pula registro de BusinessMetrics
- UseAdvancedMonitoring falha quando invocado após UseSharedServicesAsync
- Ambientes de desenvolvimento usando caminho assíncrono não têm dashboards de métricas de negócio
- Inconsistência entre dois pontos de entrada para configuração de middleware

**Impacto**:
- **Desenvolvimento**: Perda de visibilidade de métricas de negócio em dev/local
- **Testes**: Potencial para comportamento divergente entre ambientes
- **Manutenção**: Duplicação de lógica de configuração de middleware

**Solução Proposta** (do TODO):
1. Extrair registro compartilhado de middleware para método `ConfigureSharedMiddleware()`
2. Chamar de ambos os caminhos (síncrono e assíncrono)
3. OU aplicar monitoramento condicionalmente baseado em verificações do IServiceCollection

**Alternativas**:
- Deprecar um dos caminhos e padronizar em apenas um
- Criar interface comum para registro de middleware
- Usar builder pattern para configuração consistente

**Prioridade**: BAIXA (funciona em produção, afeta apenas dev)  
**Sprint Planejado**: Sprint 5 ou posterior  
**Dependências**: Nenhuma  
**Prazo**: Próxima refatoração de middleware

**Critérios de Aceitação**:
- [ ] Ambos UseSharedServices() e UseSharedServicesAsync() registram BusinessMetrics
- [ ] UseAdvancedMonitoring funciona corretamente em ambos os caminhos
- [ ] Testes de integração validam ambos os cenários
- [ ] Documentação atualizada com padrão escolhido
- [ ] TODO #249 removido do código

**Documentação**:
- Código: `src/Shared/Extensions/ServiceCollectionExtensions.cs` (linhas 96-100)
- Roadmap: Adicionado em "Média Prioridade (6-12 meses - Fase 2)"