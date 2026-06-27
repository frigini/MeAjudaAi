# 🗺️ Roadmap MeAjudaAi

Este é o planejamento estratégico unificado da plataforma MeAjudaAi.

---

## 📊 Status Atual (Junho 2026)

**Sprint Atual**: Sprint 14.6 (Planejada)

**Status**: 📋 Planejamento

**Meta MVP**: 12 a 16 de maio de 2026 (Concluída)

**Stack Principal**: .NET 10 LTS + Aspire 13 + PostgreSQL + NX Monorepo + React 19 + Next.js 15 + Tailwind v4

---

## 🏃‍♂️ Sprint Atual: Planejamento Tático

| Sprint | Foco | Status |
|--------|------|--------|
| **14** | **Auditoria de Débitos Técnicos** | 🚀 Em Execução |
| **14.6** | **Padrão Builder — Domínio Compartilhado** | 📋 Planejada |

### Detalhes Sprint 14
- ✅ **Concluído**: Padronização de Infraestrutura e Autorização.
- ✅ **Concluído**: Normalização de Endpoints da API em todos os módulos.
- ✅ **Concluído**: Implementação Core de Eventos de Integração (Bookings, Payments, Providers, Locations).
- 🚀 **Em Execução (Plano de Auditoria e Implementação)**:
    - **Fase 1**: Auditoria (Matriz de Eventos) – ✅ Concluído. (Ver `docs/event-audit-matrix.md`)
    - **Fase 2**: Implementação de Handlers pendentes – ✅ Concluído. (Todos os 10 módulos auditados e handlers/APIs implementados)
    - **Fase 3**: Testes (Unitários e Integração) – ✅ Concluído.
    - **Fase 4**: Automação e Qualidade de Wiring – ✅ Concluído.
    - **Pendências Técnicas (Concluído)**:
        - ✅ Bug: [HasCompletedBookingAsync falha com >100 bookings](https://github.com/MeAjudaAi/MeAjudaAi/issues/1)
        - ✅ Task: [Aplicar atributos \[CriticalEvent\]/\[HighVolumeEvent\] nos eventos](https://github.com/MeAjudaAi/MeAjudaAi/issues/2)
    - **Extra**: Auditoria de Comunicação Síncrona vs Assíncrona realizada (Ver `docs/communication-audit.md`) e criação de APIs Públicas.
    - **Infraestrutura e Mensageria**:
        - **Desempenho do Service Bus (Concluído)**: ✅ Ajuste fino de paralelismo baseado no atributo `[HighVolumeEvent]` via QoS.
        - **Resiliência Crítica (Concluído)**: ✅ Persistência via Quorum Queues para eventos `[CriticalEvent]`.
        - **Roteamento por Atributo (Concluído)**: ✅ Suporte total a tópicos dedicados via `AttributeTopicNameConvention`.

### Detalhes Sprint 14.6 — Padrão Builder

**Objetivo**: Criar builders de domínio reutilizáveis em `MeAjudaAi.Shared.Tests` para eliminar criação inline de entidades nos testes e padronizar o padrão de construção.

| Fase | Escopo | Status |
|------|--------|--------|
| **Fase 1** | Builders novos: Bookings (BookingBuilder, ProviderScheduleBuilder, TimeSlotBuilder, AvailabilityBuilder) | 📋 Planejada |
| **Fase 2** | Builders novos: Communications (EmailTemplateBuilder, CommunicationLogBuilder, OutboxMessageBuilder, EmailOutboxPayloadBuilder, SmsOutboxPayloadBuilder, PushOutboxPayloadBuilder) | 📋 Planejada |
| **Fase 3** | Migração de builders existentes (Users, Providers, ServiceCatalogs) para Shared.Tests | 📋 Planejada |
| **Fase 4** | Validação, limpeza e padronização (DocumentBuilder, SerializerMockBuilder) | 📋 Planejada |

**Métricas**:
- ~164 ocorrências de criação inline a substituir
- 16 builders a criar
- ~50 arquivos de teste a atualizar
- Plano detalhado: `prompts/implementacao-padrao-builders.md`

---

## 🔮 Roadmaps Futuros (Além do MVP)

### Fase 4: Experiência e Engajamento

- **Sistema de Disputas**: Mediação administrativa para conflitos.
- **Melhorias em Bookings**: Sincronização com Google Calendar/Outlook e lembretes automáticos.
- **Pendências de Infraestrutura (SearchProviders)**:
    - [ ] `AllowedCityCreated/Updated/DeletedIntegrationEventHandler`: Stub/log implementado. Ativar lógica de reindexação regional quando o filtro de cidade estiver disponível em `SearchProviders`. (Marcado com `[ExcludeFromCodeCoverage]` por serem stubs temporários).
- **Novos Eventos de Integração (Roadmap)**:
    - `AllowedCity*` com ação real (quando `SearchProviders` filtrar por cidade/região).
    - `SubscriptionExpiringSoonIntegrationEvent` (comunicação preventiva de expiração).
    - `ReviewRejectedIntegrationEvent` (notificação de cliente/prestador sobre moderação).
    - `ProviderTierUpdatedIntegrationEvent` (se tier Gold/Standard afetar Search/Communications/Admin).
- **Evolução de APIs Públicas**:
    - `IPaymentsModuleApi`: Avaliar `GetSubscriptionStatusAsync` caso surja consumidor específico para apenas o status.
    - `ICommunicationsModuleApi`: Melhorar documentação de finalidade (uso interno vs admin).
    - `IUsersApi`: Implementar SDK para gestão de usuários (CRUD, perfil, autenticação).
- **Analytics & Reports (Novo)**: 
    - **Provedores**: Dashboards de conversão (cliques vs. agendamentos), métricas de faturamento e avaliação média temporal.
    - **Administração**: Relatórios de crescimento da plataforma, hotspots geográficos de demanda e exportação de dados para contabilidade (CSV/PDF).
    - **Inteligência**: Sugestões de precificação baseadas na demanda da região.

### Refatoração: ContentModerator → Azure AI Content Safety

**Problema Atual**: O `ContentModerator` (`src/Modules/Ratings/Application/Services/ContentModerator.cs`) usa uma lista hardcodada de palavras proibidas com regex. Esta abordagem é limitada e requer manutenção manual.

**Solução Proposta**: Substituir por Azure AI Content Safety API para moderação de conteúdo.

**Benefícios**:
- Detecção avançada de ofensas, hate speech, conteúdo sexual e autoagressão
- Suporte a múltiplos idiomas (incluindo português)
- Atualizações automáticas dos modelos de IA
- Rate limiting e escalabilidade gerenciada pela Azure
- Capacidade de customizar categorias de moderação

**Plano de Implementação**:
1. Criar `AzureContentModerator` implementando `IContentModerator`
2. Configurar Azure Content Safety resource no Aspire AppHost
3. Atualizar DI para usar a nova implementação
4. Adicionar testes de integração com mock do Azure SDK
5. Manter `ContentModerator` como fallback (feature flag)

**Custos Estimados Azure AI Content Safety**:

| Tier | Text API | Image API | Limite Mensal |
|------|----------|-----------|---------------|
| **Free (F0)** | $0 | $0 | 5.000 text records + 5.000 imagens |
| **Standard (S1)** | $0.38/1.000 text records | $0.75/1.000 imagens | Sem limite |

**Estimativa para o MeAjudaAi**:
- Se ~1.000 reviews/mês com comentários (~500 caracteres cada)
- **Custo mensal**: ~$0.19 (500 text records × $0.38/1.000)
- **Com tier Free**: Custo zero até 5.000 reviews/mês
- **Commitment Tier** (para alto volume): $207.765/ano para 720M text records

**Rate Limits**:
- Free: 5 RPS (requests per second)
- Standard: 1.000 RP10S (requests per 10 seconds)

**Referências**:
- [Azure AI Content Safety Pricing](https://azure.microsoft.com/en-us/pricing/details/content-safety/)
- [Documentação oficial](https://learn.microsoft.com/en-us/azure/ai-services/content-safety/overview)


---

## ✅ Concluído Recentemente

- **Sprint 14.5**: Code review e correções — regex Language validator (case-insensitive), remoção de testes duplicados, fix de mock de payload inválido em OutboxProcessorServiceTests, correção de 3 integration event handlers (BookingCreated, ProviderRegistered, UserProfileUpdated) para não envolver não-DbUpdateException, renomeação de teste misleading.
- **Sprint 14**: Infraestrutura e Acesso, Padronização de API, Core Integration Events.
- **Sprint 13**: RabbitMQ Excellence (infraestrutura real com RabbitMqInfrastructureManager, deadlocks corrigidos, dispose seguro), i18n mocks para testes (Provider/Admin/Customer), fail-fast em DI de Messaging.
- **Sprint 12**: Bookings Module completo, Command Handlers (Reject/Complete), queries de listagem, automação com Domain Events, integração frontend de agenda.
- **Sprint 11**: Monetização completa (Checkout, Webhooks, Billing Portal, Renovação Automática), Localização i18n Frontend, Skeleton Loaders e cobertura de testes abrangente.

---

## 📜 Histórico Completo

Para detalhes das sprints anteriores, consulte o [Histórico do Roadmap](./roadmap-history.md).
