# [MONITOR] Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 - Aguardando release estável

## 📦 Bloqueando Migração EF Core 10.0.0 Stable

**Pacote**: Npgsql.EntityFrameworkCore.PostgreSQL  
**Versão atual**: 10.0.0-rc.2  
**Versão esperada**: 10.0.0 (stable)  
**Status**: ⏳ AGUARDANDO RELEASE  

### 🚨 Impacto

**BLOQUEIO CRÍTICO**: Não podemos atualizar para EF Core 10.0.0 stable até Npgsql 10.0.0 stable ser lançado.

**Pacotes bloqueados**:
- Microsoft.EntityFrameworkCore 10.0.0
- Microsoft.EntityFrameworkCore.Design 10.0.0
- Microsoft.EntityFrameworkCore.Relational 10.0.0
- Microsoft.EntityFrameworkCore.InMemory 10.0.0
- Microsoft.EntityFrameworkCore.Sqlite 10.0.0

**Motivo**: Npgsql 10.0.0-rc.2 requer **exatamente** `Microsoft.EntityFrameworkCore 10.0.0-rc.2.25502.107`. Atualizações parciais causam erro NU1107.

### ✅ Critérios para Fechar Issue

1. ✅ **Npgsql 10.0.0 stable lançado no NuGet**
2. ✅ **Dependabot cria PR automático** (não está bloqueado)
3. ✅ **Remover bloqueios do `.github/dependabot.yml`** (linhas 104-113)
4. ✅ **Atualizar todos os pacotes EF Core juntos**
5. ✅ **Build e testes passando**
6. ✅ **Testes de integração Hangfire OK**

### 📋 Checklist de Atualização

Quando Npgsql 10.0.0 stable for lançado:

- [ ] Dependabot cria PR para Npgsql (automático, não bloqueado)
- [ ] Editar `.github/dependabot.yml`: remover linhas 104-113 (bloqueios EF Core)
- [ ] Editar `Directory.Packages.props`: atualizar todos para 10.0.0 stable:
  ```xml
  <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
  <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
  <PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0" />
  <PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
  <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
  <PackageVersion Include="EFCore.NamingConventions" Version="10.0.0" />
  <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
  <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite" Version="10.0.0" />
  ```
- [ ] Executar: `dotnet restore --force-evaluate --locked-mode`
- [ ] Executar: `dotnet build`
- [ ] Executar: `dotnet test`
- [ ] Executar: `dotnet test --filter "Category=HangfireIntegration"`
- [ ] Validar em staging
- [ ] Atualizar documentação: remover TODOs sobre Npgsql
- [ ] Fechar Issue #42

### 🔔 Monitoramento Automatizado

**Configuração Atual**:
- ✅ Dependabot monitora Npgsql diariamente (não bloqueado)
- ✅ Quando 10.0.0 stable for lançado, PR automático será criado
- ✅ Workflows de monitoramento já configurados (commits b883cfd e 06703ce)

**Como saber quando lançar**:
1. **Dependabot criará PR automaticamente** 🎉
2. PR terá título: `chore: Bump the npgsql group with X updates`
3. PR incluirá upgrade para 10.0.0 stable

### 📌 Referências

- **NuGet**: https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL
- **GitHub**: https://github.com/npgsql/efcore.pg
- **Release Notes**: https://www.npgsql.org/doc/release-notes/10.0.html
- **Documentação**: `docs/ef-core-10-migration-status.md`
- **Configuração**: `Directory.Packages.props` (linhas 36-47)
- **Dependabot**: `.github/dependabot.yml` (linhas 98-118)

### 🔗 Issues Relacionadas

- Issue #38: Aspire.Npgsql.EntityFrameworkCore.PostgreSQL compatibility
- Issue #39: Hangfire.PostgreSql 2.x awaiting Npgsql 10 support

### ⏰ Timeline Estimado

- **Nov 11, 2025**: EF Core 10.0.0 stable lançado ✅
- **Dez 3, 2025**: Npgsql ainda em RC (10.0.0-rc.2) ⏳
- **Esperado Q1 2026**: Npgsql 10.0.0 stable
- **Após release**: Upgrade em 1-2 dias

### 🎯 Ação Imediata

**Nenhuma ação necessária agora**. Aguardar PR automático do Dependabot.

Quando PR do Npgsql 10.0.0 aparecer:
1. ⚠️ **NÃO fazer merge imediatamente**
2. ✅ Usar como gatilho para executar checklist acima
3. ✅ Remover bloqueios do Dependabot
4. ✅ Atualizar tudo junto (EF Core + Npgsql)
5. ✅ Testar completamente antes de merge

---

**Labels sugeridas**: `dependencies`, `monitoring`, `blocked`, `ef-core`, `npgsql`
