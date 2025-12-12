# Vulnerabilidades em Pacotes NuGet

Este documento rastreia vulnerabilidades de segurança conhecidas em dependências transitivas de pacotes NuGet.

## Status Atual

**Última Atualização:** 2025-11-20  
**Versão .NET:** 10.0 (Preview/RC)  
**Supressões Ativas:** Nenhuma ✅

Todas as vulnerabilidades detectadas são:
- **Dependências transitivas** (não referenciadas diretamente)
- **Apenas em build-time** (pacotes MSBuild, bibliotecas JWT de teste)  
- **Sem exposição em runtime** em ambientes de produção

Estas vulnerabilidades são monitoradas mas não requerem supressão ativa, pois não representam risco de segurança em runtime.

---

## Vulnerabilidades Conhecidas (Apenas Informativo)

### 1. Microsoft.Build.Tasks.Core & Microsoft.Build.Utilities.Core

**Pacote**: `Microsoft.Build.Tasks.Core`, `Microsoft.Build.Utilities.Core`  
**Versão Atual**: 17.14.8  
**Severidade**: Alta  
**Advisory**: [GHSA-w3q9-fxm7-j8fq](https://github.com/advisories/GHSA-w3q9-fxm7-j8fq)  
**CVE**: CVE-2024-43485

**Descrição**: Vulnerabilidade de Denial of Service na execução de tasks do MSBuild.

**Avaliação de Impacto**:
- Esta é uma dependência transitiva incluída por ferramentas de build
- A vulnerabilidade afeta execução em build-time, não em runtime
- Projetos não executam tasks MSBuild não confiáveis em produção
- Risco limitado a máquinas de desenvolvedores e ambientes CI/CD com acesso controlado

**Status de Mitigação**: ⏳ **Pendente**
- **Ação**: Monitorar versões atualizadas nos releases .NET 10 RC/RTM
- **Cronograma**: Correção esperada no .NET 10 RTM (meta: Q2 2025)
- **Workaround**: Todos os ambientes de build são confiáveis e com acesso controlado

**Justificativa para Aceitação Temporária**:
- Vulnerabilidade apenas em build-time
- Sem impacto em runtime de produção
- Ambientes CI/CD e desenvolvimento controlados
- Será resolvida automaticamente quando o .NET 10 SDK for atualizado

---

### 2. Microsoft.IdentityModel.JsonWebTokens & System.IdentityModel.Tokens.Jwt

**Pacote**: `Microsoft.IdentityModel.JsonWebTokens`, `System.IdentityModel.Tokens.Jwt`  
**Versão Atual**: 6.8.0  
**Severidade**: Moderada  
**Advisory**: [GHSA-59j7-ghrg-fj52](https://github.com/advisories/GHSA-59j7-ghrg-fj52)  
**CVE**: CVE-2024-21319

**Descrição**: Vulnerabilidade de Denial of Service na validação de tokens JWT.

**Avaliação de Impacto**:
- Afeta apenas projetos de teste (não código de produção)
- Projetos usando isto: `MeAjudaAi.Providers.Tests`, `MeAjudaAi.Shared.Tests`, `MeAjudaAi.Documents.Tests`, `MeAjudaAi.SearchProviders.Tests`, `MeAjudaAi.ServiceCatalogs.Tests`
- Tokens JWT de teste são gerados e controlados localmente
- Sem processamento de JWT externo em cenários de teste

**Status de Mitigação**: ⏳ **Pendente**
- **Ação**: Atualizar para `System.IdentityModel.Tokens.Jwt >= 8.0.0` quando versão compatível do framework de teste estiver disponível
- **Cronograma**: Monitorar pacotes atualizados de infraestrutura de teste
- **Bloqueio Atual**: Framework de autenticação de testes depende de versão antiga

**Justificativa para Aceitação Temporária**:
- Dependência apenas de testes
- Sem impacto em produção
- Ambiente de teste controlado
- Tokens são gerados localmente, não de fontes externas

---

## Monitoramento

- **Semanalmente**: Verificar atualizações de pacotes via `dotnet list package --outdated --include-transitive`
- **Semanalmente**: Re-executar scan de vulnerabilidades via `dotnet list package --vulnerable --include-transitive`
- **Antes de cada release**: Auditoria completa de segurança
- **Assinar**: GitHub Security Advisories para repositórios .NET

## Referências

- [Documentação NuGet Audit](https://learn.microsoft.com/nuget/concepts/auditing-packages)
- [GitHub Security Advisories](https://github.com/advisories)
- [Anúncios de Segurança .NET](https://github.com/dotnet/announcements/labels/security)
