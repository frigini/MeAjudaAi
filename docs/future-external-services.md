# External Services - Roadmap de Integração

Este documento lista serviços externos que serão integrados no futuro. Não implemente health checks para estes serviços agora - documente apenas quando a integração for desenvolvida.

## 📋 Status Atual

### ✅ Implementados (com Health Checks)

1. **Keycloak**
   - **Propósito**: Autenticação e autorização (OAuth2/OIDC)
   - **Health Check**: `ExternalServicesHealthCheck` - verifica `/realms/meajudaai`
   - **Tags**: `ready`, `external`
   - **Documentação**: [docs/authentication-and-authorization.md](authentication-and-authorization.md)

2. **IBGE API**
   - **Propósito**: Validação de localização geográfica (estados, municípios)
   - **Health Check**: `ExternalServicesHealthCheck` - verifica `/api/v1/localidades/estados/MG`
   - **Tags**: `ready`, `external`
   - **Endpoint**: `https://servicodados.ibge.gov.br/api/v1/localidades`
   - **Módulo**: `Locations`
   - **Cliente**: `IbgeClient.cs`

3. **Redis**
   - **Propósito**: Cache distribuído
   - **Health Check**: `AddRedis()` - health check nativo do AspNetCore.HealthChecks.Redis
   - **Tags**: `ready`, `cache`
   - **Documentação**: Configurado via Aspire

## 🔮 Serviços Futuros (NÃO Implementados)

### Sprint 5-6: OCR e Validação de Documentos

#### Azure Document Intelligence (OCR)
- **Propósito**: Extração de texto de documentos escaneados/fotos
- **Quando Implementar**: Quando módulo Documents estiver processando uploads de imagens
- **Health Check Futuro**: 
  - Endpoint: `POST /formrecognizer/documentModels/{modelId}:analyze`
  - Verificar autenticação e quota disponível
  - Tags: `ready`, `external`, `ocr`
- **Pacote**: `Azure.AI.DocumentIntelligence 1.0.0` (já instalado)
- **Configuração Necessária**:
  - `Azure:DocumentIntelligence:Endpoint`
  - `Azure:DocumentIntelligence:ApiKey`
- **Critérios para Health Check**:
  - [ ] Módulo Documents aceita uploads de imagens
  - [ ] OCR implementado em `DocumentVerificationJob.cs`
  - [ ] Azure Document Intelligence configurado em ambiente

#### Azure Blob Storage
- **Propósito**: Armazenamento de documentos e fotos
- **Quando Implementar**: Quando uploads de documentos forem habilitados
- **Health Check Futuro**:
  - Verificar conectividade com container
  - Validar permissões de leitura/escrita
  - Tags: `ready`, `external`, `storage`
- **Pacote**: `Azure.Storage.Blobs 12.26.0` (já instalado)
- **Configuração Necessária**:
  - `Azure:Storage:ConnectionString`
  - `Azure:Storage:ContainerName`

### Sprint 7-8: Validação de Prestadores

#### API Receita Federal (CNPJ/CPF)
- **Propósito**: Validação de documentos de prestadores (background checks)
- **Quando Implementar**: Quando verificação de prestadores for automatizada
- **Health Check Futuro**:
  - Endpoint público ou API privada (a definir)
  - Validar quota e rate limits
  - Tags: `ready`, `external`, `validation`
- **Observações**:
  - API pública da Receita pode ter rate limits agressivos
  - Considerar alternativas: Serviços terceiros (ex: BrasilAPI, ReceitaWS)
  - Implementar cache agressivo para consultas de CNPJ/CPF

#### BrasilAPI (Alternativa Receita Federal)
- **Propósito**: Validação de CNPJ, CEP, bancos
- **Quando Implementar**: Como alternativa à API da Receita
- **Health Check Futuro**:
  - Endpoint: `https://brasilapi.com.br/api/status`
  - Tags: `ready`, `external`, `validation`
- **Vantagens**:
  - API pública gratuita
  - Rate limits mais generosos
  - Múltiplos endpoints úteis (CNPJ, CEP, bancos)

### Sprint 9-10: Pagamentos (Futuro Distante)

#### Gateway de Pagamento (Ex: PagSeguro, Mercado Pago)
- **Propósito**: Processar doações e pagamentos (se aplicável)
- **Quando Implementar**: Quando modelo de monetização for definido
- **Health Check Futuro**:
  - Verificar autenticação com gateway
  - Validar saldo e permissões
  - Tags: `ready`, `external`, `payment`
- **Observações**:
  - Aguardar decisão de modelo de negócio
  - Pode nunca ser implementado se plataforma for 100% gratuita

### Sprint 11+: Notificações

#### SendGrid / AWS SES (Email)
- **Propósito**: Envio de emails transacionais (confirmação, notificações)
- **Quando Implementar**: Quando notificações por email forem necessárias
- **Health Check Futuro**:
  - Verificar autenticação e quota
  - Tags: `ready`, `external`, `email`

#### Twilio / AWS SNS (SMS)
- **Propósito**: Envio de SMS para notificações críticas
- **Quando Implementar**: Quando notificações por SMS forem necessárias
- **Health Check Futuro**:
  - Verificar autenticação e saldo
  - Tags: `ready`, `external`, `sms`

## 🎯 Decisões de Implementação

### Quando Adicionar Health Check para Novo Serviço Externo

✅ **ADICIONE Health Check se:**
- Serviço é crítico para funcionalidade principal da aplicação
- Falha do serviço impacta experiência do usuário
- Serviço é usado de forma síncrona (ex: validação em tempo real)
- Serviço está configurado em ambiente de produção

❌ **NÃO ADICIONE Health Check se:**
- Serviço é opcional ou experimental
- Serviço ainda não está configurado/implementado
- Serviço é usado apenas em jobs assíncronos (Hangfire já monitora)
- Falha do serviço não impacta disponibilidade da aplicação

### Template para Implementação Futura

Quando implementar health check para novo serviço:

```csharp
// Em ExternalServicesHealthCheck.cs

// Verificar [Nome do Serviço]
try
{
    var serviceUrl = configuration["[ServiceConfig:Url]"];
    if (!string.IsNullOrEmpty(serviceUrl))
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        using var response = await httpClient.GetAsync($"{serviceUrl}/[health-endpoint]", cancellationToken);
        stopwatch.Stop();

        results["[service_name]"] = new
        {
            status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
            response_time_ms = stopwatch.ElapsedMilliseconds,
            endpoint = "[health-endpoint]"
        };

        if (!response.IsSuccessStatusCode)
            allHealthy = false;
    }
}
catch (Exception ex)
{
    results["[service_name]"] = new { status = "unhealthy", error = ex.Message };
    allHealthy = false;
}
```

### Checklist para Nova Integração

Antes de adicionar health check para novo serviço externo:

- [ ] Serviço está implementado e funcional no código
- [ ] Configuração do serviço existe em `appsettings.json`
- [ ] Cliente HTTP ou SDK está configurado no DI container
- [ ] Serviço está disponível em ambiente de desenvolvimento
- [ ] Endpoint de health check do serviço foi identificado
- [ ] Timeout apropriado está configurado (padrão: 5s)
- [ ] Testes unitários foram criados para o health check
- [ ] Documentação foi atualizada neste arquivo

## 📚 Referências

- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- [Health UI Dashboard](http://localhost:5193/health-ui) (Development)
- [Roadmap Geral](roadmap.md)
