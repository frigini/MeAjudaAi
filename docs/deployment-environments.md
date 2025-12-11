# Ambientes de Deploy

## Visão Geral
Este documento descreve os diferentes ambientes de deploy disponíveis para a plataforma MeAjudaAi e suas configurações.

## Tipos de Ambientes

### Ambiente de Desenvolvimento
- **Propósito**: Desenvolvimento local e testes
- **Configuração**: Setup simplificado com bancos de dados locais
- **Acesso**: Apenas máquinas de desenvolvedores
- **Banco de Dados**: Container PostgreSQL local
- **Autenticação**: Simplificada para desenvolvimento

### Ambiente de Staging
- **Propósito**: Testes e validação pré-produção
- **Configuração**: Setup similar à produção com dados de teste
- **Acesso**: Time de desenvolvimento e stakeholders
- **Banco de Dados**: Banco de dados dedicado para staging
- **Autenticação**: Sistema de autenticação completo

### Ambiente de Produção
- **Propósito**: Aplicação live servindo usuários reais
- **Configuração**: Totalmente segura e otimizada
- **Acesso**: Usuários finais e administradores autorizados
- **Banco de Dados**: PostgreSQL de produção com backups
- **Autenticação**: Autenticação completa com provedores externos

## Processo de Deploy

### ⚠️ CRÍTICO: Validação Pré-Deploy

**ANTES de fazer deploy em QUALQUER ambiente**, garanta que TODAS as validações críticas de compatibilidade passem.

Para procedimentos detalhados de validação de compatibilidade Hangfire + Npgsql 10.x, consulte a documentação de infraestrutura e execute testes em staging.

**Checklist Rápido**:
- [ ] ⚠️ **CRÍTICO**: Smoke tests em staging com execução de jobs Hangfire (Npgsql 10.x NÃO VALIDADO)
- [ ] Verificação manual do dashboard Hangfire em staging
- [ ] Monitoramento de health check configurado (HealthChecks.Hangfire)
- [ ] Monitoramento configurado (alertas, dashboards)
- [ ] Procedimento de rollback testado
- [ ] Time treinado e stakeholders notificados

---

### Setup de Infraestrutura
O processo de deploy usa templates Bicep para infraestrutura como código:

1. **Recursos Azure**: Definidos em `infrastructure/main.bicep`
2. **Service Bus**: Configurado em `infrastructure/servicebus.bicep`
3. **Docker Compose**: Configurações específicas por ambiente

### Pipeline CI/CD
Deploy automatizado via GitHub Actions:

1. **Build**: Compilar e testar a aplicação
2. **Scan de Segurança**: Detecção de vulnerabilidades e secrets
3. **Deploy**: Push para o ambiente apropriado
4. **Validação**: Health checks e smoke tests

### Variáveis de Ambiente
Cada ambiente requer configuração específica:

- **Conexões de banco de dados**
- **Provedores de autenticação**
- **Endpoints de serviços**
- **Níveis de logging**
- **Feature flags**

## Procedimentos de Rollback

### Rollback Hangfire + Npgsql (CRÍTICO)

**Condições de Gatilho** (execute rollback se QUALQUER ocorrer):
- Taxa de falha de jobs Hangfire excede 5% por >1 hora
- Jobs críticos de background falham repetidamente
- Erros de conexão Npgsql aumentam nos logs
- Dashboard indisponível ou mostra corrupção de dados
- Performance do banco de dados degrada significativamente

Para procedimentos detalhados de rollback e troubleshooting, veja documentação de health checks do Hangfire.

**Passos Rápidos de Rollback**:

1. **Parar Aplicação** (~5 min)
   ```bash
   az webapp stop --name $APP_NAME --resource-group $RESOURCE_GROUP
   ```

2. **Backup de Banco** (~10 min, se necessário)
   ```bash
   pg_dump -h $DB_HOST -U $DB_USER --schema=hangfire -Fc > hangfire_backup.dump
   ```

3. **Downgrade de Pacotes** (~15 min)
   - Reverter para EF Core 9.x + Npgsql 8.x em `Directory.Packages.props`

4. **Rebuild & Redeploy** (~30 min)
   ```bash
   dotnet test --filter Category=HangfireIntegration  # Validar
   ```

5. **Verificar Saúde** (~30 min)
   - Verificar dashboard Hangfire: `$API_ENDPOINT/hangfire`
   - Monitorar processamento de jobs e logs

**Procedimento Completo de Rollback**: Veja o guia de compatibilidade dedicado para comandos agnósticos de ambiente e troubleshooting detalhado.

## Monitoramento e Manutenção

### Monitoramento Crítico

Para monitoramento abrangente de Hangfire + jobs de background, monitore via health checks e logs da aplicação.

**Métricas Chave**:
1. **Taxa de Falha de Jobs**: Alerta se >5% → Investigar e considerar rollback
2. **Erros de Conexão Npgsql**: Monitorar logs da aplicação
3. **Saúde do Dashboard**: Verificar endpoint `/hangfire` a cada 5 minutos
4. **Tempo de Processamento de Jobs**: Alerta se aumento >50% da baseline

### Health Checks
- Endpoints de saúde da aplicação
- Conectividade do banco de dados
- Disponibilidade de serviços externos

### Logging
- Logging estruturado com Serilog
- Integração com Application Insights
- Rastreamento e alertas de erros

### Backup e Recuperação
- Backups regulares de banco de dados
- Backups de estado de infraestrutura
- Procedimentos de recuperação de desastres

## Documentação Relacionada

- [Setup de CI/CD](./ci-cd.md)
- [Documentação de Infraestrutura](./infrastructure.md)
- [Diretrizes de Desenvolvimento](./development.md)