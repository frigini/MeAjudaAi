# Sistema de Resiliência de API - MeAjudaAi Admin Portal

## Visão Geral

O portal administrativo implementa padrões de resiliência completos usando **Polly** para garantir uma experiência de usuário robusta mesmo quando há problemas de conectividade ou instabilidade da API.

## Componentes do Sistema

### 1. Políticas Polly (`PollyPolicies.cs`)

#### Política de Retry (Tentativas com Backoff Exponencial)
```csharp
// 3 tentativas: aguarda 2s, 4s, 8s entre tentativas
GetRetryPolicy(logger)
```

**Quando ativa:**
- Erros HTTP 5xx (500-599)
- Erro 408 (Request Timeout)
- Timeout do Polly

**Comportamento:**
- 1ª falha: aguarda 2 segundos, tenta novamente
- 2ª falha: aguarda 4 segundos, tenta novamente
- 3ª falha: aguarda 8 segundos, tenta novamente
- Se todas falharem: propaga erro para o Circuit Breaker

#### Circuit Breaker
```csharp
// Abre após 5 falhas consecutivas, aguarda 30s antes de tentar novamente
GetCircuitBreakerPolicy(logger)
```

**Estados:**
- **Closed** (Fechado): Operação normal, requisições passam
- **Open** (Aberto): 5 falhas consecutivas detectadas, todas as requisições falham imediatamente por 30s
- **Half-Open** (Meio-Aberto): Após 30s, testa uma requisição. Se sucesso → Closed, se falha → Open novamente

**Benefícios:**
- Previne sobrecarga do servidor quando está com problemas
- Falha rápido durante indisponibilidade (evita timeouts desnecessários)
- Auto-recuperação quando serviço volta

#### Timeout Policy
```csharp
// Timeout de 30s para operações normais
GetTimeoutPolicy()

// Timeout de 2 minutos para uploads
GetUploadTimeoutPolicy()
```

**Operações Normais:** 30 segundos
**Uploads de Arquivos:** 2 minutos (sem retry para evitar uploads duplicados)

### 2. Handler de Logging (`PollyLoggingHandler.cs`)

Intercepta todas as requisições HTTP e:
- Adiciona contexto ao Polly para logging detalhado
- Captura exceções de Circuit Breaker
- Atualiza status de conexão
- Retorna respostas HTTP 503 quando circuit breaker está aberto

### 3. Serviço de Status de Conexão (`ConnectionStatusService.cs`)

Rastreia o estado atual da conexão com a API:

```csharp
public enum ConnectionStatus
{
    Connected,      // ✅ Conectado normalmente
    Reconnecting,   // 🟡 Tentando reconectar (retry ou half-open)
    Disconnected    // 🔴 Desconectado (circuit breaker open)
}
```

**Evento:**
```csharp
event EventHandler<ConnectionStatus> StatusChanged;
```

### 4. Mensagens de Erro Amigáveis (`ApiErrorMessages.cs`)

Traduz códigos HTTP e exceções em mensagens compreensíveis:

```csharp
// Exemplos:
400 Bad Request → "A operação contém dados inválidos..."
401 Unauthorized → "Sua sessão expirou..."
503 Service Unavailable → "O serviço está temporariamente indisponível..."
Circuit Breaker → "Aguarde alguns instantes enquanto tentamos restabelecer..."
```

### 5. Indicador Visual (`ConnectionStatusIndicator.razor`)

Componente Blazor que mostra o status da conexão em tempo real:

- ✅ **Verde (Cloud Done)**: Conectado
- 🟡 **Amarelo (Cloud Sync - Girando)**: Reconectando
- 🔴 **Vermelho (Cloud Off)**: Sem conexão

Localização: `MainLayout.razor` (AppBar)

### 6. Extensões Fluxor (`FluxorEffectExtensions.cs`)

Simplifica o tratamento de erros nos efeitos:

```csharp
var result = await dispatcher.ExecuteApiCallAsync(
    apiCall: () => _providersApi.GetProvidersAsync(page, size),
    snackbar: _snackbar,
    operationName: "Carregar provedores",
    onSuccess: data => { /* sucesso */ },
    onError: ex => { /* erro */ }
);
```

**Benefícios:**
- Notificações automáticas de erro no Snackbar
- Logging automático
- Tratamento consistente de todos os tipos de erro
- Código limpo e fácil de manter

## Configuração

### Program.cs

```csharp
// 1. Registrar serviços
builder.Services.AddSingleton<IConnectionStatusService, ConnectionStatusService>();
builder.Services.AddScoped<PollyLoggingHandler>();

// 2. Configurar clientes API com políticas Polly
builder.Services
    .AddApiClient<IProvidersApi>(apiUrl)           // Política padrão
    .AddApiClient<IDocumentsApi>(apiUrl, true);    // Política de upload (sem retry)
```

### ServiceCollectionExtensions.cs

```csharp
public static IServiceCollection AddApiClient<TClient>(
    this IServiceCollection services, 
    string baseUrl,
    bool useUploadPolicy = false)
{
    var builder = services.AddRefitClient<TClient>()
        .ConfigureHttpClient(c => c.BaseAddress = uri)
        .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
        .AddHttpMessageHandler<PollyLoggingHandler>();

    if (useUploadPolicy)
        builder.AddPolicyHandler(PollyPolicies.GetUploadPolicy(logger));
    else
        builder.AddPolicyHandler(PollyPolicies.GetCombinedPolicy(logger));
    
    return services;
}
```

## Ordem de Execução das Políticas

A ordem é crítica para funcionamento correto:

```
Request → Timeout → Retry → Circuit Breaker → API
                ↓       ↓           ↓
            30s max  3 tentativas  5 falhas = open
```

1. **Timeout** (externa): Garante que toda a operação não demore mais que 30s
2. **Retry** (meio): Tenta até 3 vezes com backoff exponencial
3. **Circuit Breaker** (interna): Rastreia falhas e previne sobrecarga

## Logs Detalhados

### Retry
```
⚠️ Retry 1/3 after 2s delay. Request: /api/providers. Reason: 503 Service Unavailable
⚠️ Retry 2/3 after 4s delay. Request: /api/providers. Reason: Timeout
```

### Circuit Breaker
```
🔴 Circuit breaker opened! Will retry after 30s. Reason: Too many failures
🟡 Circuit breaker half-open - testing connection
✅ Circuit breaker reset - connection restored
```

### HTTP Errors
```
❌ Unexpected error during HTTP request: /api/providers
```

## Exemplo de Uso em Effects

### Antes (Sem Resiliência)
```csharp
[EffectMethod]
public async Task HandleLoadAction(LoadAction action, IDispatcher dispatcher)
{
    try
    {
        var result = await _api.GetDataAsync();
        if (result.IsSuccess)
            dispatcher.Dispatch(new LoadSuccessAction(result.Value));
        else
            dispatcher.Dispatch(new LoadFailureAction(result.Error.Message));
    }
    catch (Exception ex)
    {
        dispatcher.Dispatch(new LoadFailureAction("Erro desconhecido"));
    }
}
```

### Depois (Com Resiliência)
```csharp
[EffectMethod]
public async Task HandleLoadAction(LoadAction action, IDispatcher dispatcher)
{
    await dispatcher.ExecuteApiCallAsync(
        apiCall: () => _api.GetDataAsync(),
        snackbar: _snackbar,
        operationName: "Carregar dados",
        onSuccess: data => dispatcher.Dispatch(new LoadSuccessAction(data.Items)),
        onError: ex => dispatcher.Dispatch(new LoadFailureAction(ex.Message))
    );
}
```

**Melhorias:**
- ✅ Retry automático (3 tentativas)
- ✅ Circuit breaker previne sobrecarga
- ✅ Timeout de 30s
- ✅ Mensagens de erro amigáveis
- ✅ Notificações automáticas no Snackbar
- ✅ Logging detalhado
- ✅ Atualização de status de conexão
- ✅ Código mais limpo e legível

## Testes de Cenários

### Cenário 1: Servidor Temporariamente Indisponível
1. API retorna 503
2. Polly tenta 3 vezes (2s, 4s, 8s)
3. Se todas falharem: Circuit Breaker conta como 1 falha
4. Usuário vê: "Reconectando..." no indicador
5. Snackbar: "O serviço está temporariamente indisponível..."

### Cenário 2: Múltiplas Falhas Consecutivas
1. 5 requisições falham seguidas
2. Circuit Breaker abre
3. Próximas requisições falham imediatamente (sem esperar timeout)
4. Indicador mostra: "Sem conexão" (vermelho)
5. Após 30s: Circuit Breaker tenta uma requisição (half-open)
6. Se sucesso: Volta ao normal

### Cenário 3: Timeout na Requisição
1. Requisição demora mais de 30s
2. Polly cancela a requisição
3. Conta como falha para retry
4. Usuário vê: "A operação demorou muito tempo..."

### Cenário 4: Upload de Arquivo Grande
1. Usa política de upload (timeout 2min, sem retry)
2. Se falhar: Não tenta novamente (evita upload duplicado)
3. Circuit Breaker ainda ativo para prevenir múltiplas tentativas do usuário

## Benefícios do Sistema

### Para o Usuário
- ✅ Experiência mais suave durante instabilidade
- ✅ Feedback visual claro do status da conexão
- ✅ Mensagens de erro compreensíveis
- ✅ Auto-recuperação transparente

### Para o Sistema
- ✅ Previne sobrecarga do servidor
- ✅ Falha rápido quando necessário
- ✅ Logs detalhados para diagnóstico
- ✅ Métricas de saúde da API

### Para Desenvolvedores
- ✅ Código limpo e consistente
- ✅ Fácil de testar
- ✅ Padrão reutilizável
- ✅ Documentação clara

## Monitoramento

### Logs para Observar
```csharp
// Sucesso após retry
Successfully loaded 10 providers after 1 retry attempt

// Circuit breaker events
Circuit breaker opened at 2026-01-16 14:30:00
Circuit breaker reset at 2026-01-16 14:30:30

// Timeouts
Request timeout after 30s: GET /api/providers
```

### Métricas Importantes
- Taxa de retry (quantas requisições precisam de retry)
- Taxa de circuit breaker open (frequência de abertura)
- Duração média das requisições
- Taxa de erro por endpoint

## Próximos Passos

1. **Métricas Avançadas**: Integrar com Application Insights ou Prometheus
2. **Políticas Customizadas**: Políticas diferentes por endpoint
3. **Fallback**: Respostas de cache quando API está indisponível
4. **Bulkhead**: Isolar falhas de diferentes serviços
5. **Rate Limiting**: Prevenir sobrecarga do lado do cliente

## Referências

- [Polly Documentation](https://www.pollydocs.org/)
- [Microsoft.Extensions.Http.Polly](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly)
- [Resilience Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/category/resiliency)
