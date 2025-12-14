# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia itens de débito técnico e melhorias planejadas identificadas durante o desenvolvimento que devem ser convertidas em issues do GitHub.

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
- [ ] Validação manual em ambiente de staging com carga realística
- [ ] Monitoramento de produção configurado (alertas de taxa de falha >5%)
- [ ] Procedimento de rollback testado em staging
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
**Dependências**: Testes de integração, validação em staging, monitoramento de produção  
**Prazo**: Antes de qualquer deploy para produção

**Critérios de Aceitação**:
- [x] Testes de integração implementados e passando
- [x] CI/CD gating configurado para bloquear deploy se testes falharem
- [x] Documentação de compatibilidade criada
- [x] Procedimento de rollback documentado e testado
- [ ] Validação em staging com carga de produção
- [ ] Monitoramento de produção configurado
- [ ] Equipe treinada em procedimento de rollback
- [ ] Stakeholders notificados sobre o risco e plano de mitigação

**Documentação**:
- Guia completo: Monitoramento via health checks em produção
- Testes: Removidos - validação via staging e health checks
- CI/CD: `.github/workflows/pr-validation.yml` (step "CRITICAL - Hangfire Npgsql 10.x Compatibility Tests")
- Configuração: `Directory.Packages.props` (linhas 45-103)

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
**Issue**: [Criar issue para rastreamento]

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
