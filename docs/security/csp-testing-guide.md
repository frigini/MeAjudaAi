# Content Security Policy (CSP) Testing Guide

## Overview

Este guia documenta como testar a Content Security Policy (CSP) do MeAjudaAi Admin Portal usando Chrome/Edge DevTools para garantir que nenhum recurso legítimo seja bloqueado inadvertidamente.

## Por que Testar CSP?

CSP é uma camada crítica de segurança que previne:
- **XSS (Cross-Site Scripting)**: Execução de scripts maliciosos
- **Clickjacking**: Iframe malicioso
- **Data Injection**: Injeção de dados não autorizados

**Mas** uma configuração CSP muito restritiva pode:
- ❌ Bloquear bibliotecas necessárias (MudBlazor, Fluxor)
- ❌ Quebrar funcionalidades (themes, modals, tooltips)
- ❌ Impedir upload de arquivos

## Configuração CSP Atual

```csharp
// src/Modules/WebApi/Extensions/SecurityHeadersExtensions.cs
Content-Security-Policy:
  default-src 'self';
  script-src 'self' 'unsafe-inline' https://trusted-cdn.com;
  style-src 'self' 'unsafe-inline';
  img-src 'self' data: https:;
  font-src 'self' data:;
  connect-src 'self' https://api.meajudaai.com https://auth.meajudaai.com;
  frame-ancestors 'none';
```

> ⚠️ **Nota**: `'unsafe-inline'` é usado temporariamente para MudBlazor styles. Ver [minor-improvements-roadmap.md](../minor-improvements-roadmap.md#part-5-implement-nonce-based-csp) para migração futura.

---

## Passo a Passo: Testes Manuais com DevTools

### 1. Abrir DevTools

**Chrome/Edge**:
- Pressione `F12` ou `Ctrl+Shift+I` (Windows/Linux)
- Ou `Cmd+Option+I` (Mac)
- Navegue para a tab **Console**

### 2. Ativar Visualização de Violations

1. Clique no ícone de **Settings** (⚙️) no canto superior direito do DevTools
2. Na seção **Console**, marque:
   - ✅ **Show violations**
   - ✅ **Log XMLHttpRequests**
3. Feche Settings

Agora violations aparecerão no console como warnings laranja.

### 3. Recarregar Página e Observar

1. Pressione `Ctrl+Shift+R` (hard reload sem cache)
2. Observe o console para mensagens de CSP

**Exemplo de violation legítima** (pode ignorar):
```
[Report Only] Refused to load the script 
'chrome-extension://boadgeojelhgndaghljhdicfkmllpafd/cast_sender.js' 
because it violates the following Content Security Policy directive: "script-src 'self'".
```
> ✅ **OK**: Extensões do Chrome são sempre bloqueadas, comportamento esperado.

**Exemplo de violation crítica** (CORRIGIR):
```
Refused to load the stylesheet 'https://fonts.googleapis.com/css?family=Roboto:300,400,500,700'
because it violates the following Content Security Policy directive: "style-src 'self'".
```
> ❌ **ERRO**: MudBlazor precisa de Google Fonts. Adicionar ao CSP:
```csharp
style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
font-src 'self' https://fonts.gstatic.com;
```

---

## Fluxos de Teste Obrigatórios

Execute TODOS os fluxos abaixo e verifique que não há violations críticas:

### ✅ Checklist de Testes

| # | Fluxo | Status | Violations? |
|---|-------|--------|-------------|
| 1 | **Login/Logout** | ☐ | |
| 2 | **Navegação entre páginas** (Providers, Service Catalogs, Locations) | ☐ | |
| 3 | **Upload de documentos** | ☐ | |
| 4 | **Formulários** (Create Provider, Add Document) | ☐ | |
| 5 | **MudBlazor Components**: Snackbar, Dialog, Tooltip, Menu | ☐ | |
| 6 | **Dark/Light Mode Toggle** | ☐ | |
| 7 | **Fluxor Redux DevTools** (apenas dev) | ☐ | |
| 8 | **API Calls** (fetch/XHR para backend) | ☐ | |

### Teste 1: Login/Logout

1. Navegar para `/authentication/login`
2. Fazer login com Keycloak
3. Verificar redirect de volta para app
4. Clicar em **Sair** no menu de usuário
5. Verificar redirect para Keycloak logout

**Violations esperadas**: Nenhuma  
**Se houver**: Adicionar domínio do Keycloak ao `connect-src`

### Teste 2: Navegação entre Páginas

1. Home → Providers → Service Catalogs → Locations
2. Usar links na NavMenu
3. Verificar que todas as páginas carregam CSS/JS corretamente

**Violations esperadas**: Nenhuma  
**Se houver**: Recursos estáticos podem estar vindo de CDN não permitido

### Teste 3: Upload de Documentos

1. Ir para `/providers/{id}/documents`
2. Clicar em "Adicionar Documento"
3. Fazer upload de arquivo (PDF, imagem)
4. Submeter formulário

**Violations esperadas**: Nenhuma  
**Se houver**: Verificar `img-src data:` para preview de imagens

### Teste 4: Formulários

1. Create Provider form
2. Preencher todos os campos
3. Submit e validar resposta

**Violations esperadas**: Nenhuma  
**Se houver**: Validações de FluentValidation podem precisar de `script-src`

### Teste 5: MudBlazor Components

**Snackbar**:
```csharp
// Injetar em qualquer página
@inject ISnackbar Snackbar

<MudButton OnClick="ShowSnackbar">Test</MudButton>

@code {
    void ShowSnackbar() => Snackbar.Add("Test message", Severity.Info);
}
```

**Dialog**:
- Abrir qualquer modal (ex: Delete confirmation)
- Verificar que overlay e conteúdo aparecem

**Tooltip**:
- Hover sobre ícones com tooltip
- Verificar que aparece

**Menu**:
- Abrir menu de usuário (Account Circle)
- Verificar que dropdown funciona

**Violations esperadas**: Nenhuma  
**Se houver**: MudBlazor usa styles inline, verificar `style-src 'unsafe-inline'`

### Teste 6: Dark/Light Mode

1. Clicar no ícone de sol/lua no AppBar
2. Verificar que tema muda instantaneamente
3. Verificar cores de todos os componentes

**Violations esperadas**: Nenhuma  
**Se houver**: Dynamic styles do MudBlazor bloqueados

### Teste 7: Fluxor Redux DevTools (Dev Only)

1. Abrir Redux DevTools extension
2. Disparar uma action (ex: LoadProviders)
3. Verificar que state aparece no DevTools

**Violations esperadas**: 
```
[Violation] Added non-passive event listener to a scroll-blocking 'touchstart' event.
```
> ✅ OK: Warning de performance, não CSP.

### Teste 8: API Calls

1. Abrir **Network** tab no DevTools
2. Navegar para `/providers`
3. Verificar que chamadas para `/api/providers` funcionam

**Violations esperadas**: Nenhuma  
**Se houver**: Adicionar domínio da API ao `connect-src`

---

## Tipos de Violations e Como Resolver

### Violation: `script-src`

**Mensagem**:
```
Refused to execute inline script because it violates the following Content Security Policy directive: "script-src 'self'".
```

**Causa**: Script inline (`<script>alert('test')</script>`) ou script de CDN externo

**Solução**:
```csharp
// Adicionar 'unsafe-inline' (temporário) OU nonce (recomendado)
script-src 'self' 'unsafe-inline';

// OU permitir CDN específico
script-src 'self' https://cdn.jsdelivr.net;
```

### Violation: `style-src`

**Mensagem**:
```
Refused to apply inline style because it violates the following Content Security Policy directive: "style-src 'self'".
```

**Causa**: MudBlazor gera styles dinâmicos inline

**Solução Atual**:
```csharp
style-src 'self' 'unsafe-inline';
```

**Solução Futura** (nonce-based):
```csharp
style-src 'self' 'nonce-{random-value}';
```

### Violation: `connect-src`

**Mensagem**:
```
Refused to connect to 'https://api.external.com' because it violates the following Content Security Policy directive: "connect-src 'self'".
```

**Causa**: Fetch/XHR para domínio não permitido

**Solução**:
```csharp
connect-src 'self' https://api.meajudaai.com https://auth.meajudaai.com;
```

### Violation: `img-src`

**Mensagem**:
```
Refused to load the image 'data:image/png;base64,...' because it violates the following Content Security Policy directive: "img-src 'self'".
```

**Causa**: Imagens em data URI (base64 inline)

**Solução**:
```csharp
img-src 'self' data: https:;
```

### Violation: `frame-ancestors`

**Mensagem**:
```
Refused to frame 'https://admin.meajudaai.com' because an ancestor violates the following Content Security Policy directive: "frame-ancestors 'none'".
```

**Causa**: Alguém tentou colocar o app em iframe (clickjacking)

**Solução**: ✅ **MANTER `frame-ancestors 'none'`** (segurança contra clickjacking)

---

## Testes Automatizados

### Playwright CSP Violation Detection

```csharp
// tests/MeAjudaAi.E2E.Tests/Security/CspViolationTests.cs
using Microsoft.Playwright;
using NUnit.Framework;

[TestFixture]
public class CspViolationTests
{
    private IPage _page;
    private List<string> _cspViolations;

    [SetUp]
    public async Task Setup()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        _page = await browser.NewPageAsync();
        
        _cspViolations = new List<string>();
        
        // Escutar mensagens de console
        _page.Console += (_, msg) =>
        {
            var text = msg.Text;
            if (text.Contains("Content Security Policy") || 
                text.Contains("Refused to") ||
                text.Contains("violates the following"))
            {
                _cspViolations.Add(text);
            }
        };
    }

    [Test]
    public async Task ProvidersPage_ShouldNotHaveCspViolations()
    {
        // Act
        await _page.GotoAsync("https://localhost:5001/providers");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        Assert.That(_cspViolations, Is.Empty, 
            $"CSP violations detected:\n{string.Join("\n", _cspViolations)}");
    }
    
    [Test]
    public async Task UploadDocument_ShouldNotHaveCspViolations()
    {
        await _page.GotoAsync("https://localhost:5001/providers/123/documents");
        
        // Fazer upload
        await _page.SetInputFilesAsync("input[type='file']", "test-document.pdf");
        await _page.ClickAsync("button:text('Upload')");
        
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        Assert.That(_cspViolations, Is.Empty);
    }
    
    [Test]
    public async Task DarkModeToggle_ShouldNotHaveCspViolations()
    {
        await _page.GotoAsync("https://localhost:5001");
        
        // Toggle dark mode
        await _page.ClickAsync("[aria-label='Toggle dark mode']");
        await Task.Delay(1000); // Aguardar transição
        
        Assert.That(_cspViolations, Is.Empty);
    }
    
    [Test]
    public async Task AllCriticalFlows_ShouldBeTestedForCsp()
    {
        var pages = new[]
        {
            "/",
            "/providers",
            "/service-catalogs",
            "/locations"
        };
        
        foreach (var page in pages)
        {
            _cspViolations.Clear();
            await _page.GotoAsync($"https://localhost:5001{page}");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Assert.That(_cspViolations, Is.Empty, 
                $"Page {page} has CSP violations");
        }
    }
}
```

### Executar Testes

```bash
# Rodar todos os testes de CSP
dotnet test --filter Category=CSP

# Rodar com verbosidade
dotnet test --filter Category=CSP --logger "console;verbosity=detailed"
```

---

## CI/CD Pipeline Integration

### GitHub Actions

```yaml
# .github/workflows/csp-tests.yml
name: CSP Validation

on:
  pull_request:
    paths:
      - 'src/Modules/WebApi/Extensions/SecurityHeadersExtensions.cs'
      - 'src/Web/MeAjudaAi.Web.Admin/**'

jobs:
  csp-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Install Playwright
        run: |
          dotnet build tests/MeAjudaAi.E2E.Tests
          pwsh tests/MeAjudaAi.E2E.Tests/bin/Debug/net10.0/playwright.ps1 install
      
      - name: Run CSP Tests
        run: dotnet test tests/MeAjudaAi.E2E.Tests --filter Category=CSP
      
      - name: Upload Test Results
        if: failure()
        uses: actions/upload-artifact@v3
        with:
          name: csp-test-results
          path: TestResults/
```

---

## Production Monitoring

Para ambiente de produção, configurar monitoramento contínuo de violations:

### 1. CSP Report-URI

Ver [docs/minor-improvements-roadmap.md](../minor-improvements-roadmap.md#suggestion-2-csp-violation-monitoring-⏳-sprint-6) para implementação completa.

**Resumo**:
```csharp
// Adicionar ao CSP header
Content-Security-Policy: 
  default-src 'self';
  report-uri /api/csp/violations;
  report-to csp-endpoint;
```

### 2. Application Insights Query

```kusto
// Query KQL para dashboard
customEvents
| where name == "CspViolation"
| summarize Count = count() by BlockedUri, ViolatedDirective, bin(timestamp, 1h)
| order by timestamp desc, Count desc
| take 100
```

### 3. Alertas

**Email** se violation rate > 100/hour:
```kusto
customEvents
| where name == "CspViolation"
| summarize Count = count() by bin(timestamp, 1h)
| where Count > 100
```

**Slack webhook** para violations críticas (scripts maliciosos):
```kusto
customEvents
| where name == "CspViolation"
| where ViolatedDirective == "script-src"
| where BlockedUri !contains "chrome-extension"
| where BlockedUri !contains "trusted-cdn.com"
```

---

## Troubleshooting

### Problema: "Muitas violations de chrome-extension"

**Causa**: Extensões do Chrome tentam injetar scripts  
**Solução**: ✅ Ignorar, é esperado. Adicionar filtro no console:
```
-chrome-extension
```

### Problema: "MudBlazor styles não aplicam"

**Causa**: CSP bloqueando `style-src 'unsafe-inline'`  
**Solução**: Verificar que CSP tem:
```csharp
style-src 'self' 'unsafe-inline';
```

### Problema: "Font Roboto não carrega"

**Causa**: Google Fonts bloqueado  
**Solução**:
```csharp
style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
font-src 'self' https://fonts.gstatic.com;
```

### Problema: "Redux DevTools não funciona"

**Causa**: CSP bloqueando comunicação com extension  
**Solução**: Em development, relaxar CSP:
```csharp
#if DEBUG
  script-src 'self' 'unsafe-inline' 'unsafe-eval';
#endif
```

---

## Referências

- [MDN: Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [CSP Evaluator (Google)](https://csp-evaluator.withgoogle.com/)
- [Report URI (CSP Reporting)](https://report-uri.com/)
- [OWASP CSP Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html)

## Próximos Passos

1. ✅ Executar checklist manual de testes (este guia)
2. ⏳ Implementar testes automatizados Playwright
3. ⏳ Configurar CSP violation reporting endpoint
4. ⏳ Adicionar monitoramento no Application Insights
5. ⏳ Migrar para nonce-based CSP (Sprint 7)

Ver [docs/minor-improvements-roadmap.md](../minor-improvements-roadmap.md) para roadmap completo.
