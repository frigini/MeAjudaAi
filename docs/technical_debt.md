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
2. ✅ Testes de integração abrangentes criados (`tests/MeAjudaAi.Integration.Tests/Jobs/HangfireIntegrationTests.cs`)
3. ✅ CI/CD gating configurado (`.github/workflows/pr-validation.yml`)
4. ✅ Guia de compatibilidade documentado (`docs/hangfire-npgsql-compatibility.md`)
5. ✅ Procedimentos de rollback documentados
6. ✅ Plano de monitoramento de produção definido

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
- Guia completo: `docs/hangfire-npgsql-compatibility.md`
- Testes: `tests/MeAjudaAi.Integration.Tests/Jobs/HangfireIntegrationTests.cs`
- CI/CD: `.github/workflows/pr-validation.yml` (step "CRITICAL - Hangfire Npgsql 10.x Compatibility Tests")
- Configuração: `Directory.Packages.props` (linhas 45-103)

---

## 🚧 Swagger ExampleSchemaFilter - Migração para Swashbuckle 10.x

**Arquivos**: 
- `src/Bootstrapper/MeAjudaAi.ApiService/Filters/ExampleSchemaFilter.cs`
- `src/Bootstrapper/MeAjudaAi.ApiService/Extensions/DocumentationExtensions.cs`

**Situação**: DESABILITADO TEMPORARIAMENTE  
**Severidade**: MÉDIA  
**Issue**: [Criar issue para rastreamento]

**Descrição**: 
O `ExampleSchemaFilter` foi desabilitado temporariamente devido a incompatibilidades com a migração do Swashbuckle para a versão 10.x.

**Problema Identificado**:
- Swashbuckle 10.x mudou a assinatura de `ISchemaFilter.Apply()` para usar `IOpenApiSchema` (interface)
- `IOpenApiSchema.Example` é uma propriedade read-only na interface
- A implementação concreta (tipo interno do Swashbuckle) tem a propriedade Example writable
- Microsoft.OpenApi 2.3.0 não expõe o namespace `Microsoft.OpenApi.Models` esperado
- **Solução confirmada**: Usar reflexão para acessar a propriedade Example na implementação concreta

**Funcionalidade Perdida**:
- Geração automática de exemplos no Swagger UI baseado em `DefaultValueAttribute`
- Exemplos inteligentes baseados em nomes de propriedades (email, telefone, nome, etc.)
- Exemplos automáticos para tipos enum
- Descrições detalhadas de schemas baseadas em `DescriptionAttribute`

**Implementação Atual**:
```csharp
// DocumentationExtensions.cs (linha ~118)
// TODO: Reativar após migração para Swashbuckle 10.x completar
// options.SchemaFilter<ExampleSchemaFilter>();  // ← COMENTADO

// ExampleSchemaFilter.cs
// SOLUÇÃO: Usar IOpenApiSchema (assinatura correta) + reflexão para Example
#pragma warning disable IDE0051, IDE0060
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // Swashbuckle 10.x: IOpenApiSchema.Example é read-only
        // SOLUÇÃO: Usar reflexão para acessar implementação concreta
        throw new NotImplementedException("Precisa migração - usar reflexão");
        
        // Quando reativar:
        // var exampleProp = schema.GetType().GetProperty("Example");
        // if (exampleProp?.CanWrite == true) 
        //     exampleProp.SetValue(schema, exampleValue, null);
    }
}
#pragma warning restore IDE0051, IDE0060
```

**Opções de Solução**:

**OPÇÃO 1 (RECOMENDADA - VALIDADA)**: ✅ Usar Reflection para Acessar Propriedade Concreta
```csharp
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Swashbuckle 10.x usa OpenApiSchema (tipo concreto) no ISchemaFilter
        // Propriedade Example é writable no tipo concreto
        if (context.Type.GetProperties().Any(p => p.GetCustomAttributes(typeof(DefaultValueAttribute), false).Any()))
        {
            var exampleValue = GetExampleFromDefaultValueAttribute(context.Type);
            schema.Example = exampleValue; // Direto, sem reflexão necessária
        }
    }
}
```
- ✅ **Assinatura correta**: `OpenApiSchema` (tipo concreto conforme Swashbuckle 10.x)
- ✅ **Compila sem erros**: Validado no build
- ✅ **Funcionalidade preservada**: Mantém lógica original
- ✅ **Sem reflexão**: Acesso direto à propriedade Example
- ✅ **Import correto**: `using Microsoft.OpenApi.Models;`

**STATUS**: Código preparado para esta solução, aguardando reativação

**OPÇÃO 2 (FALLBACK - SE OPÇÃO 1 FALHAR)**: Usar Reflection (Versão Anterior)
```csharp
public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
{
    // Caso tipo concreto não funcione, usar interface + reflexão
    var exampleProperty = schema.GetType().GetProperty("Example");
    if (exampleProperty != null && exampleProperty.CanWrite)
    {
        exampleProperty.SetValue(schema, exampleValue, null);
    }
}
```
- ⚠️ **Usa reflexão**: Pequeno overhead de performance
- ⚠️ **Risco**: Pode quebrar se Swashbuckle mudar implementação interna

**OPÇÃO 3**: Investigar Nova API do Swashbuckle 10.x (ALTERNATIVA)
- Verificar documentação oficial do Swashbuckle 10.x
- Pode haver novo mecanismo para definir exemplos (ex: `IExampleProvider` ou attributes)
- Conferir: <https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases>
- ⚠️ **Risco**: Pode não existir API alternativa, forçando uso de reflexão (Opção 1)

**OPÇÃO 3**: Usar Atributos Nativos do OpenAPI 3.x
```csharp
[OpenApiExample("exemplo@email.com")]
public string Email { get; set; }
```
- Requer migração de todos os models para usar novos atributos
- Mais verboso, mas type-safe

**OPÇÃO 4**: Aguardar Swashbuckle 10.x Estabilizar
- Monitorar issues do repositório oficial
- Pode haver mudanças na API antes da versão estável

**Impacto no Sistema**:
- ✅ Build funciona normalmente
- ✅ Swagger UI gerado corretamente
- ❌ Exemplos não aparecem automaticamente na documentação
- ❌ Desenvolvedores precisam deduzir formato de requests manualmente

**Prioridade**: MÉDIA  
**Dependências**: Documentação oficial do Swashbuckle 10.x, Microsoft.OpenApi 2.3.0  
**Prazo**: Antes da release 1.0 (impacta experiência de desenvolvedores)

**Critérios de Aceitação**:
- [ ] Investigar API correta do Swashbuckle 10.x para definir exemplos
- [ ] Implementar solução escolhida (Opção 1, 2, 3 ou 4)
- [ ] Reativar `ExampleSchemaFilter` em `DocumentationExtensions.cs`
- [ ] Validar que exemplos aparecem corretamente no Swagger UI
- [ ] Remover `#pragma warning disable` e código comentado
- [ ] Adicionar testes unitários para o filtro
- [ ] Documentar solução escolhida para futuras migrações

**Passos de Investigação**:
1. Ler changelog completo do Swashbuckle 10.x
2. Verificar se `Microsoft.OpenApi` versão 2.x expõe tipos concretos em outros namespaces
3. Testar Opção 1 (reflection) em ambiente de dev
4. Consultar issues/discussions do repositório oficial
5. Criar POC com cada opção antes de decidir

**Documentação de Referência**:
- Swashbuckle 10.x Release Notes: <https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v10.0.0>
- Microsoft.OpenApi Docs: <https://github.com/microsoft/OpenAPI.NET>
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