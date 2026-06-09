# Technical Debt

Este documento lista dívidas técnicas identificadas e o plano de mitigação.

## ⚠️ Dívidas Técnicas Ativas

| Área | Dívida | Prioridade | Descrição |
| :--- | :--- | :--- | :--- |
| Messaging | Otimização de Performance | Resolvido | Refatorado `RabbitMqInfrastructureManager` para suportar `[HighVolumeEvent]` configurando QoS/prefetch. |
| Messaging | Quorum Queues | Resolvido | Implementado suporte a Quorum Queues para `[CriticalEvent]`. |
| Testing | E2E Tests | Média | Expandir cobertura de testes de fluxo ponta a ponta (CrossModuleFlowTests). |
| Domain | Mapeamento de Eventos | Baixa | Verificar se eventos como `AllowedCityCreatedIntegrationEvent` são realmente necessários ou se podem ser removidos da matriz. |
| Infrastructure | Migrations | Média | Revisar aplicação de migrations em testes de integração para evitar erros de pool de conexões sob carga. |
