# Débito Técnico e Rastreamento de Melhorias

Este documento rastreia **débitos técnicos e seu histórico de otimização**. Itens podem aparecer como PENDENTES ou OTIMIZADO/FECHADO para histórico.

---

## 🔮 Melhorias Futuras (Backlog)

### 🧪 Testes & Qualidade

**Severidade**: BAIXA  
**Sprint**: Backlog

- [ ] Perfilagem de memória em produção

### 🏗️ Arquitetura & Código

**Severidade**: ALTA  
**Sprint**: Backlog

- [ ] **Auditoria e Implementação de Consumidores de Eventos de Integração**: Foi realizada uma normalização na emissão de eventos (`IntegrationEvents`) para os módulos `Bookings`, `Payments`, `Providers` e `Locations`. É necessário realizar uma auditoria completa de todos os eventos de integração para garantir que cada um possua ao menos um consumidor (`IntegrationEventHandler`) válido, removendo eventos "órfãos". Além disso, deve-se revisar todos os módulos para implementar seus respectivos consumidores e garantir que os testes unitários e de integração cubram o consumo desses eventos, completando o ciclo de vida da arquitetura orientada a eventos.
- [ ] **Revisão da Arquitetura de Eventos**: Revisar o balanço entre chamadas diretas via `Contracts` (API) e comunicação reativa (Eventos). Identificar pontos onde o acoplamento síncrono pode ser reduzido convertendo chamadas de ação/escrita em consumo de eventos de integração.
- [ ] **Contratos de Cliente (Client Contracts) Incompletos**: O projeto `MeAjudaAi.Client.Contracts` carece de interfaces `Refit` para módulos críticos (`Bookings`, `Users`, `Payments`, `Communications`, `Ratings`, `SearchProviders`). Isso força implementações manuais ou duplicadas de consumo de API em diferentes clientes, comprometendo a segurança de tipos e a facilidade de integração. É necessário criar as interfaces equivalentes para todos os módulos restantes para padronizar o consumo de API em toda a solução.

---

## 📋 Histórico

### 🔄 Sincronização de Dados Desnormalizados via Eventos (Sincronização de Nomes de Serviços)

**Resolvido em**: Jun 2026 (Sprint 14) | **Severidade original**: ALTA
Implementação de `ServiceNameUpdatedIntegrationEvent` para garantir a consistência eventual de nomes de serviços sem dependência física de banco de dados entre os módulos `ServiceCatalogs` e `Providers`. A funcionalidade foi coberta por testes no módulo de origem (`ServiceCatalogs`).

### 🎨 UI/UX Admin Portal - Cores da Marca

**Resolvido em**: Abr 2026 (Sprint 13) | **Severidade original**: BAIXA  
Aplicação de cores da marca (laranja #D96704, brand #E0702B, cream #FDFBF7) no CSS do Admin, variáveis CSS para cores secundárias, ring atualizado para cor da marca, e correção de `data-testid` duplicados em CardTitle.

### 🌍 i18n Frontend Apps (Admin/Provider/Customer)

**Resolvido em**: Abr 2026 (Sprint 13) | **Severidade original**: MÉDIA  
Implementação de `useTranslation` no dashboard Admin, tradução de labels via i18n, e mocks de i18next para testes em todos os apps (Admin/Provider/Customer) com suporte a `defaultValue` (incluindo strings vazias).

### 🚀 Infraestrutura & Messaging (RabbitMQ Excellence)

**Resolvido em**: Abr 2026 (Sprint 13) | **Severidade original**: MÉDIA  
A infraestrutura do RabbitMQ foi consolidada com a implementação real do `RabbitMqInfrastructureManager`, eliminando stubs e garantindo a declaração automática de exchanges (Topic), filas de domínio e bindings. O sistema foi atualizado para o `RabbitMQ.Client` 7.x utilizando o novo padrão assíncrono. Cobertura de testes unitários e de integração (via Testcontainers) superior a 90% no fechamento da Sprint 13. O uso direto do RabbitMQ agora é gerenciado de forma centralizada e resiliente.

### ⚠️ Hangfire + Npgsql 10.x Compatibility Risk

**Resolvido em**: Abr 2026 (Sprint 11) | **Severidade original**: CRÍTICA  
Hangfire.PostgreSql atualizado para 1.21.1 com compatibilidade validada. Risco de incompatibilidade com Npgsql 10.x monitorado e mitigado; entrada removida do backlog ativo.

### 🔐 Keycloak Client - Configuração Manual

**Resolvido em**: Jan 2026 (Sprint 7, PR `#107`) | **Severidade original**: MÉDIA  
Automatização da configuração do Keycloak Client implementada.

### ⚡ Otimização de Testes de Integração

**Resolvido em**: Jan 2026 (Sprint 7.6) | **Severidade original**: ALTA  
`TestModule` enum implementado — 83% de melhoria no tempo de execução; timeouts e erros 57P01 eliminados.

### 🎨 Admin Portal - Warnings de Analyzers (SonarLint/MudBlazor)

**Resolvido em**: Mar 2026 (Sprint 8B.2, PR `#151`) | **Severidade original**: BAIXA  
Warnings S2094, S2953, S2933 e MUD0002 resolvidos no portal Admin.

### 🌍 Localization Enhancements

**Resolvido em**: Abr 2026 (Sprint 11) | **Severidade original**: BAIXA  
Item reavaliado e removido do backlog ativo após implementação da i18n no `MeAjudaAi.Web.Customer` (Sprint 11).

### 🛡️ Error Handling & Resilience

**Resolvido em**: Abr 2026 (Sprint 11) | **Severidade original**: BAIXA  
Item reavaliado e removido do backlog ativo; resiliência coberta pelas estratégias de compensação e inbox pattern implementadas no módulo Payments.

### 📅 Bookings Funcionalidade e Integração (Sprint 12 Gaps)

**Resolvido em**: Abr 2026 (Sprint 12) | **Severidade original**: MÉDIA  
Implementados os command handlers de Reject e Complete, queries de listagem, automação com Domain Events, integração frontend de agenda, e cobertura E2E do módulo.

> Para histórico completo anterior, consultar: `git log --oneline -- docs/technical-debt.md`

---

## 📖 Instruções para Mantenedores

- Sempre documentar novos débitos técnicos identificados.
- Ao resolver um débito, movê-lo para a seção de Histórico com a nota de resolução.
- Manter a severidade atualizada para auxiliar na priorização de Sprints.
